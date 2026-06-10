using UnityEngine;

public class FlyingGrenade : MonoBehaviour
{
    [Header("Explosion Stats")]
    public float damage = 50f;
    public float explosionRadius = 5f;
    public GameObject explosionEffect;

    private bool hasExploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Ignoruj uderzenie w gracza przy wylocie
        if (collision.gameObject.CompareTag("Player")) return;

        Debug.Log("FIZYCZNE ZDERZENIE Z: " + collision.gameObject.name);

        if (!hasExploded)
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;
        Debug.Log(" FUNKCJA EXPLODE URUCHOMIONA!");

        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        // Szukamy wszystkich collider�w w strefie wybuchu
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            // 1. REAKCJA POTWORA
            if (hit.CompareTag("Monster"))
            {
                BasicMonster monster = hit.GetComponent<BasicMonster>() ?? hit.GetComponentInParent<BasicMonster>();
                if (monster != null)
                {
                    monster.TakeDamage(damage);
                    Debug.Log(" ZADANO DMG POTWOROWI: " + hit.name);
                }
                continue;
            }

            // 2. REAKCJA GRACZA (Naprawiona)
            // Sprawdza czy trafiony obiekt (lub jego rodzic, je�li collider jest na dziecku) ma tag "Player"
            if (hit.CompareTag("Player") || (hit.transform.parent != null && hit.transform.parent.CompareTag("Player")))
            {
                // Szukamy Twojego skryptu Player na trafionym obiekcie lub u rodzica
                Player playerScript = hit.GetComponent<Player>() ?? hit.GetComponentInParent<Player>();

                if (playerScript != null)
                {
                    playerScript.TakeDamage(10f);
                    Debug.Log(" GRANAT ZADA� OBRA�ENIA GRACZOWI! Aktualne HP: " + playerScript.health);
                }
                else
                {
                    Debug.LogError(" Znaleziono obiekt Gracza, ale nie ma na nim skryptu 'Player'!");
                }
            }
        }

        Destroy(gameObject);
    }
}