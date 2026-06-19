// using UnityEngine;

// public class MonsterHealer : BasicMonster
// {
//     [Header("Healer Settings")]
//     public float healRadius = 10f;       
//     public float healCooldown = 3f;    
//     public float healAmount = 10f;

//     private float healTimer = 0f;
    
//     public float orbsRotationSpeed = 50f;
//     public GameObject Orbs;


//     private void RotateOrbs(){
//         Orbs.transform.Rotate(Vector3.up * orbsRotationSpeed * Time.deltaTime);
//     }

//     protected override void Update()
//     {
    
//         base.Update();

//         RotateOrbs();

     
//         healTimer += Time.deltaTime;
//         if (healTimer >= healCooldown)
//         {
//             HealNearbyMonsters();
//             healTimer = 0f;
//         }
//     }

//     private void HealNearbyMonsters()
//     {
       
//         Collider[] hitColliders = Physics.OverlapSphere(transform.position, healRadius);

//         foreach (Collider hitCollider in hitColliders)
//         {
          
//             BasicMonster nearbyMonster = hitCollider.GetComponent<BasicMonster>();

//             if (nearbyMonster != null && nearbyMonster is not MonsterHealer)
//             {
//                 nearbyMonster.Heal(healAmount);
//             }
//         }
//     }

//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.green;
//         Gizmos.DrawWireSphere(transform.position, healRadius);
//     }
// }