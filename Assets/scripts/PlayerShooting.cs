using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public Transform camTransform;
    public LayerMask ignoreLayer;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("KLIKNIÊCIE WYKRYTE!"); 
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, range, ~ignoreLayer))
        {
            Monster monster = hit.transform.GetComponentInParent<Monster>();

            if (monster != null)
            {
                monster.TakeDamage(damage);
            }
        }
    }
}