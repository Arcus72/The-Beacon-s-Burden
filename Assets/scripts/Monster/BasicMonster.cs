using UnityEngine;
using UnityEngine.AI; // WYMAGANE do obsługi NavMeshAgent
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))] // Zmieniono z CharacterController na NavMeshAgent
public class BasicMonster : MonoBehaviour, IMonster
{
    public string name;

    [Header("Monster's movement")]
    public float maxHealth = 100f;
    public float speed = 4.0f;
    public float rotationSpeed = 150f;

    public GameObject[] targets = new GameObject[2];
    private GameObject closestTarget;

    public MonsterHealthBar _healthbar;
    public float _currentHealth;

    [Header("Attack details")]
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackSpeed = 1.5f;
    private float attackTimer = 0f;

    [Header("Odds")]
    public float lootDropChance = 1f;
    public float spawningChance = 0.2f;
    public int spawnMultiplayer = 1;

    [Header("Sound Settings (Single AudioSource Setup)")]
    [Tooltip("The ONLY Audio Source component on this monster (Set it to 3D!)")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip attackSound;
    public AudioClip deathSound;

    [Header("Sound Volumes (0.0 to 1.0)")]
    [Range(0f, 1f)] public float walkVolume = 0.5f;
    [Range(0f, 1f)] public float attackVolume = 1.0f;
    [Range(0f, 1f)] public float deathVolume = 1.0f;

    [Header("Animator parameters")]
    private string walkParameter = "Walk";
    private string attackTrigger = "Attack";
    private string deadTrigger = "Dead";
    public int totalAttacks = 2;
    public int totalWalk = 2;

    private float targetSearchTimer = 0f;

    // Zmiana komponentu fizycznego na agenta AI
    private NavMeshAgent agent;

    private Animator animator;
    private bool isDead = false;
    private bool isMovingThisFrame = false;
    private bool wasMoving = false;

    private bool hasAttackIndex;
    private bool hasWalkIndex;
    private float oneShotEndTime;

    // ---- IMonster ----
    public GameObject[] Targets { get => targets; set => targets = value; }
    public int SpawnMultiplier => spawnMultiplayer;
    public float SpawningChance => spawningChance;

    private void Start()
    {
        // Pobieramy i konfigurujemy NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
            agent.angularSpeed = rotationSpeed;
            // Zasięg ataku definiuje, jak blisko celu agent ma się zatrzymać
            agent.stoppingDistance = attackRange - 0.2f;
        }

        _currentHealth = maxHealth;
        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"Brak komponentu Animator na obiekcie {gameObject.name}!");
        }
        else
        {
            hasAttackIndex = HasAnimatorParameter(animator, "AttackIndex", AnimatorControllerParameterType.Int);
            hasWalkIndex = HasAnimatorParameter(animator, "WalkIndex", AnimatorControllerParameterType.Int);
        }

        // Ignorowanie kolizji z własnymi colliderami potomnymi
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
        {
            // Ponieważ nie ma CharacterControllera, ignorujemy kolizje między innymi colliderami na potworze
            if (c != GetComponent<Collider>())
                Physics.IgnoreCollision(GetComponent<Collider>(), c);
        }

        if (audioSource != null && walkSound != null)
        {
            audioSource.clip = walkSound;
            audioSource.volume = walkVolume;
            audioSource.loop = true;
        }

        SelectWalkStyle();
    }

    private bool HasAnimatorParameter(Animator anim, string paramName, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == type) return true;
        }
        return false;
    }

    void PerformRandomAttack()
    {
        int randomIndex = Random.Range(0, totalAttacks);
        animator.ResetTrigger(walkParameter);

        if (hasAttackIndex) animator.SetInteger("AttackIndex", randomIndex);

        animator.SetTrigger(attackTrigger);
        PlayOneShotSound(attackSound, attackVolume);
    }

    private void PlayOneShotSound(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null) return;
        if (audioSource.isPlaying) audioSource.Stop();

        audioSource.PlayOneShot(clip, volume);
        oneShotEndTime = Time.time + clip.length;
    }

    void SelectWalkStyle()
    {
        int randomIndex = Random.Range(0, totalWalk);
        if (hasWalkIndex) animator.SetInteger("WalkIndex", randomIndex);
    }

    private void Update()
    {
        if (isDead) return;

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        isMovingThisFrame = false;

        targetSearchTimer += Time.deltaTime;
        if (targetSearchTimer >= 0.5f)
        {
            FindClosestTarget();
            targetSearchTimer = 0f;
        }

        if (closestTarget != null)
        {
            Collider targetCollider = closestTarget.GetComponent<Collider>();
            Vector3 targetPoint = targetCollider != null
                ? targetCollider.ClosestPoint(transform.position)
                : closestTarget.transform.position;

            float distanceToSurface = Vector3.Distance(transform.position, targetPoint);

            if (distanceToSurface <= attackRange)
            {
                // Jesteśmy blisko celu -> zatrzymaj się i atakuj
                agent.ResetPath();
                AttackTarget(closestTarget);
            }
            else
            {
                // Jesteśmy daleko -> inteligentnie nawiguj omijając przeszkody
                MoveTowardsPoint(targetPoint);

                if (attackTimer < attackSpeed)
                    attackTimer += Time.deltaTime;
            }
        }

        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);

        // Kontrola animacji chodu
        if (animator != null && isMovingThisFrame && !wasMoving)
            animator.SetTrigger(walkParameter);

        wasMoving = isMovingThisFrame;
        HandleWalkAudio();
    }

    private void HandleWalkAudio()
    {
        if (audioSource == null || walkSound == null) return;
        if (Time.time < oneShotEndTime) return;

        if (isMovingThisFrame)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = walkVolume;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying) audioSource.Stop();
        }
    }

    private void FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        GameObject tempTarget = null;

        foreach (GameObject target in targets)
        {
            if (target == null) continue;

            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider == null) continue;

            Vector3 closestPointOnSurface = targetCollider.ClosestPoint(transform.position);
            float distance = Vector3.Distance(transform.position, closestPointOnSurface);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                tempTarget = target;
            }
        }
        closestTarget = tempTarget;
    }

    private void MoveTowardsPoint(Vector3 goal)
    {
        if (isDead || agent == null || !agent.enabled) return;

        // Zamiast matematycznego przesuwania, mówimy agentowi gdzie ma dotrzeć.
        // Komponent sam ominie skały widoczne na upieczonym NavMesh.
        agent.SetDestination(goal);

        // Sprawdzamy czy agent faktycznie się porusza, żeby kontrolować animację/dźwięk chodu
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            isMovingThisFrame = true;
        }
    }

    private void AttackTarget(GameObject target)
    {
        if (isDead) return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackSpeed)
        {
            if (animator != null) PerformRandomAttack();

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(attackDamage);

            attackTimer = 0f;
        }
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth < 0) _currentHealth = 0;

        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);

        Debug.Log("Potwór dostał obrażenia! HP: " + _currentHealth);
    }

    public void Heal(float amount)
    {
        if (_currentHealth <= 0) return;

        _currentHealth += amount;
        if (_currentHealth > maxHealth) _currentHealth = maxHealth;

        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayOneShotSound(deathSound, deathVolume);

        // Bezpieczne wyłączenie agenta AI po śmierci
        if (agent != null)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        // Wyłączenie kolizji zwłok
        foreach (Collider c in GetComponentsInChildren<Collider>(true))
        {
            c.enabled = false;
        }

        if (animator != null)
            animator.SetTrigger(deadTrigger);

        if (LootManager.Instance != null)
            LootManager.Instance.SpawnLoot(transform.position, lootDropChance);

        StartCoroutine(DestroyAfterAnimation(3.0f));
    }

    private IEnumerator DestroyAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}