using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public float speed = 2.0f;
    public float rotationSpeed = 150f;

    public GameObject target1;
    public GameObject target2;

    private GameObject[] targets;
    private GameObject closestTarget;

    void Start()
    {
        targets = new GameObject[2];
        targets[0] = target1;
        targets[1] = target2;
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

    void Update()
    {
        FindClosestTarget();

        if (closestTarget != null)
        {
            // 1. Tworzymy pomocniczy punkt docelowy na TEJ SAMEJ wysokoœci co kapsu³a
            Vector3 flatTargetPosition = closestTarget.transform.position;
            flatTargetPosition.y = transform.position.y; // <--- To blokuje lewitacjê!

            float step = speed * Time.deltaTime;

            // 2. Poruszamy siê do "p³askiego" celu
            transform.position = Vector3.MoveTowards(transform.position, flatTargetPosition, step);

            // 3. Logika obrotu (u¿ywaj¹c ju¿ przygotowanego flatTargetPosition)
            Vector3 direction = flatTargetPosition - transform.position;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}