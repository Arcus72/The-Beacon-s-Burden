using UnityEngine;
using System.Collections;

// Self-contained Tank monster. Does NOT inherit from BasicMonster - all of the
// shared movement/attack/health logic is inlined here so the Tank lives in a
// single file. It implements IMonster so weapons and the spawner still recognise it.
[RequireComponent(typeof(CharacterController))]
public class BasicMonster : MonoBehaviour, IMonster
{
    public string name;

    [Header("Monster's movement")]
    public float maxHealth = 100f;
    public float speed = 4.0f;
    public float rotationSpeed = 150f;
    private float gravity = -9.81f;

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

    [Header("Animator parameters")]
    public string walkParameter = "Walk";
    public string attackTrigger = "Attack";
    public string deadTrigger = "Dead";

    private float targetSearchTimer = 0f;
    private CharacterController controller;
    private float verticalVelocity;

    private Animator animator;
    private bool isDead = false;
    private bool isMovingThisFrame = false;
    private bool wasMoving = false;

    // ---- IMonster ----
    public GameObject[] Targets { get => targets; set => targets = value; }
    public int SpawnMultiplier => spawnMultiplayer;
    public float SpawningChance => spawningChance;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        _currentHealth = maxHealth;
        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"Brak komponentu Animator na obiekcie {gameObject.name}!");
        }

         if (controller != null)
        {
            foreach (Collider c in GetComponentsInChildren<Collider>(true))
            {
                if (c != controller)
                    Physics.IgnoreCollision(controller, c);
            }
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        if (controller == null || !controller.enabled)
        {
            return;
        }

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
                AttackTarget(closestTarget);
            }
            else
            {
                MoveTowardsPoint(targetPoint);

                if (attackTimer < attackSpeed)
                    attackTimer += Time.deltaTime;
            }
        }

        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);

        // Walk is a trigger: fire it only on the rising edge (when movement
        // starts), not every frame, otherwise the Any State -> Walk transition
        // restarts the clip from frame 0 each frame and the animation stutters.
        if (animator != null && isMovingThisFrame && !wasMoving)
            animator.SetTrigger(walkParameter);

        wasMoving = isMovingThisFrame;
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
        if (isDead || controller == null || !controller.enabled) return;

        Vector3 direction = (goal - transform.position).normalized;
        direction.y = 0;

        if (controller.isGrounded)
            verticalVelocity = -0.5f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 moveVector = (direction * speed) + (Vector3.up * verticalVelocity);
        controller.Move(moveVector * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        isMovingThisFrame = true;
    }

    private void AttackTarget(GameObject target)
    {
        if (isDead) return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackSpeed)
        {
            if (animator != null)
            {
                // Clear any pending Walk trigger so it can't immediately pull us
                // back out of the Attack state before the attack clip plays.
                animator.ResetTrigger(walkParameter);
                animator.SetTrigger(attackTrigger);
            }

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

        Debug.Log(name + " został uleczony! HP: " + _currentHealth);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{name} umiera...");

        if (controller != null)
        {
            controller.stepOffset = 0f;
            controller.enabled = false; // Wyłączamy go bezpiecznie, bo Update już go nie dotknie
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
