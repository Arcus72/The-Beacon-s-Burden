using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Monster : MonoBehaviour
{
    public float maxHealth = 100f;
    public float speed = 2.0f;
    public float rotationSpeed = 150f;
    public float gravity = -9.81f;
    public float reduceingHealthTime = 1f;

    public GameObject[] targets = new GameObject[2];
    private GameObject closestTarget;

    public MonsterHealthBar _healthbar;
    public float _currentHealth;

    private float timer = 0f;
    private CharacterController controller;
    private float verticalVelocity;

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
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    tempTarget = target;
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

    void Update()
    {
      
        reduceHealth(reduceingHealthTime, 5f);
        if (_currentHealth <= 0)
            Destroy(gameObject);
        if (_healthbar)
            _healthbar.UpdateHealtBar(maxHealth, _currentHealth);

     
        FindClosestTarget();

        if (closestTarget != null)
        {
          
            Vector3 direction = closestTarget.transform.position - transform.position;
            direction.y = 0; 
            direction.Normalize();

           
            if (controller.isGrounded)
            {
                verticalVelocity = -0.5f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

       
            Vector3 moveVector = (direction * speed) + (Vector3.up * verticalVelocity);
            controller.Move(moveVector * Time.deltaTime);

         
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}