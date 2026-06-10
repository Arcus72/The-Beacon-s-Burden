using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BasicMonster :  MonoBehaviour
{   
    public string name;
    [Header("Monster's movement")]
    public float maxHealth = 100f;
    public float speed = 4.0f;
    public float rotationSpeed = 150f;
    public float gravity = -9.81f;

    public GameObject[] targets = new GameObject[2];
    protected GameObject closestTarget;

    public MonsterHealthBar _healthbar;
    public float _currentHealth;
    [Header("Attack details")]
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackSpeed = 1.5f;
    protected float attackTimer = 0f;

    [Header("Odds")]
    public float lootDropChance = 1f;
    public float spawningChance = 0.2f;
    public int spawnMultiplayer = 1;

    protected float timer = 0f;
    protected float targetSearchTimer = 0f;
    protected CharacterController controller;
    protected float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        _currentHealth = maxHealth;
        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);
    }

    void FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        GameObject tempTarget = null;

        foreach (GameObject target in targets)
        {
            if (target != null)
            {
                Collider targetCollider = target.GetComponent<Collider>();

                if (targetCollider != null)
                {
                
                    Vector3 closestPointOnSurface = targetCollider.ClosestPoint(transform.position);
                    float distance = Vector3.Distance(transform.position, closestPointOnSurface);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        tempTarget = target;
                    }
                }
            }
        }
        closestTarget = tempTarget;
    }

    void reduceHealth(float delay, float amount)
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            _currentHealth -= amount;
            if (_currentHealth < 0) _currentHealth = 0;
            timer = 0f;
        }
    }

    public virtual void Heal(float amount)
{
    if (_currentHealth <= 0) return; 

    _currentHealth += amount;
    if (_currentHealth > maxHealth) _currentHealth = maxHealth;

    if (_healthbar)
        _healthbar.UpdateHealtBar(maxHealth, _currentHealth);

    Debug.Log(name + " został uleczony! HP: " + _currentHealth);
}

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth < 0) _currentHealth = 0;

        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);

        Debug.Log("Potw�r dosta� obra�enia! HP: " + _currentHealth);
    }

    public void Die() {
        LootManager.Instance.SpawnLoot(transform.position, lootDropChance);
        Destroy(gameObject);
    }
    
    protected virtual void Update()
    {
       
        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

   
        targetSearchTimer += Time.deltaTime;
        if (targetSearchTimer >= 0.5f)
        {
            FindClosestTarget();
            targetSearchTimer = 0f;
        }

        if (closestTarget != null)
        {
            Collider targetCollider = closestTarget.GetComponent<Collider>();
            Vector3 targetPoint;

            if (targetCollider != null)
                targetPoint = targetCollider.ClosestPoint(transform.position);
            else
                targetPoint = closestTarget.transform.position;

            float distanceToSurface = Vector3.Distance(transform.position, targetPoint);

            if (distanceToSurface <= attackRange)
            {
                AttackTarget(closestTarget);
            }
            else
            {
                MoveTowardsPoint(targetPoint);

                if (attackTimer < attackSpeed)
                {
                    attackTimer += Time.deltaTime;
                }
            }
        }

        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);
    }

    
   

    void MoveTowardsPoint(Vector3 goal)
    {
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
    }



   void AttackTarget(GameObject target)
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackSpeed)
        {
       
            IDamageable damageable = target.GetComponent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(attackDamage);
          

            attackTimer = 0f;
        }
    }
}