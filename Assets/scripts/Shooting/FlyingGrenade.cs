using UnityEngine;

public class FlyingGrenade : MonoBehaviour
{
    [Header("Explosion Stats")]
    public float damage = 50f;
    public float explosionRadius = 5f;
    public GameObject explosionEffect;

    [Header("Explosion Audio")]
    public AudioClip explosionSound;

    private bool hasExploded = false;

    private void OnCollisionEnter(Collision collision)
    {
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

        // ODTWARZANIE DŹWIĘKU EKSPLOZJI W PRZESTRZENI 3D
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        if (explosionEffect != null)
        {
            GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Monster"))
            {
                IMonster monster = hit.GetComponent<IMonster>() ?? hit.GetComponentInParent<IMonster>();
                if (monster != null)
                {
                    monster.TakeDamage(damage);
                    Debug.Log(" ZADANO DMG POTWOROWI: " + hit.name);
                }
                continue;
            }

            if (hit.CompareTag("Player") || (hit.transform.parent != null && hit.transform.parent.CompareTag("Player")))
            {
                Player playerScript = hit.GetComponent<Player>() ?? hit.GetComponentInParent<Player>();

                if (playerScript != null)
                {
                    playerScript.TakeDamage(10f);
                    Debug.Log(" GRANAT ZADAŁ OBRAŻENIA GRACZOWI! Aktualne HP: " + playerScript.health);
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