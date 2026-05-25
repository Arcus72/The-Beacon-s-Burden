using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShotgunShooting : MonoBehaviour
{
    [Header("Shotgun Stats")]
    public float damagePerPellet = 7f;   // Obra¿enia zadawane przez JEDEN od³amek
    public int pelletCount = 8;          // Ile od³amków wylatuje przy jednym strzale
    public float range = 25f;            // Krótki zasiêg typowy dla strzelby
    public float spreadSpread = 0.1f;    // Jak bardzo pociski siê rozpraszaj¹ (rozrzut)
    public LayerMask ignoreLayer;

    [Header("Weapon Visuals & Animation")]
    public Animator gunAnimator;
    public string shootAnimationName = "Shotgun_Shoot"; // Nazwa Twojej animacji shotguna
    public Transform muzzlePoint;        // Pusty obiekt na koñcu lufy strzelby

    [Header("Custom Effects (Prefabs)")]
    public GameObject muzzleFlashPrefab; // Wiêkszy rozb³ysk œwiat³a
    public GameObject bulletPrefab;      // Prefab smugi (bêdzie klonowany wielokrotnie)

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("B£¥D: Nie znaleziono obiektu z tagiem 'MainCamera'!");
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("STRZA£ Z SHOTGUNA: " + gameObject.name);
            ShootShotgun();
        }
    }

    void ShootShotgun()
    {
        if (mainCamera == null) return;

        // 1. ANIMACJA SHOTGUNA
        if (gunAnimator != null)
        {
            gunAnimator.ResetTrigger("Fire");
            gunAnimator.Play(shootAnimationName, 0, 0f);
        }

        // 2. DU¯Y ROZB£YSK (Muzzle Flash)
        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            flash.transform.SetParent(muzzlePoint);
            Destroy(flash, 0.05f);
        }

        // Baza kierunku strza³u z kamery
        Vector3 rayOrigin = mainCamera.transform.position;
        Vector3 baseDirection = mainCamera.transform.forward;

        // 3. LOGIKA ROZPRYSKU (Pêtla generuj¹ca wiele od³amków)
        for (int i = 0; i < pelletCount; i++)
        {
            // Obliczamy losowy rozrzut dla ka¿dego od³amka osobno
            Vector3 spreadFactor = mainCamera.transform.right * Random.Range(-spreadSpread, spreadSpread)
                                 + mainCamera.transform.up * Random.Range(-spreadSpread, spreadSpread);

            // Koñcowy, lekko przekrzywiony kierunek lotu od³amka
            Vector3 pelletDirection = (baseDirection + spreadFactor).normalized;

            RaycastHit hit;
            Vector3 targetPoint;

            // Sprawdzamy trafienie dla konkretnego od³amka
            if (Physics.Raycast(rayOrigin, pelletDirection, out hit, range, ~ignoreLayer))
            {
                targetPoint = hit.point;

                // Zadawanie obra¿eñ potworowi (ka¿dy od³amek rani osobno!)
                Monster monster = hit.transform.GetComponentInParent<Monster>();
                if (monster != null)
                {
                    monster.TakeDamage(damagePerPellet);
                }
            }
            else
            {
                // Jeœli od³amek w nic nie trafi³, leci na maksymalny (ale krótki) zasiêg strzelby
                targetPoint = rayOrigin + pelletDirection * range;
            }

            // 4. SPAWN SMUGI DLA KA¯DEGO OD£AMKA
            if (bulletPrefab != null && muzzlePoint != null)
            {
                GameObject tracer = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
                StartCoroutine(MoveTracer(tracer, targetPoint));
            }
        }
    }

    private System.Collections.IEnumerator MoveTracer(GameObject tracer, Vector3 target)
    {
        Vector3 startPoint = tracer.transform.position;
        float time = 0;
        float speed = 0.02f; // Od³amki strzelby mog¹ lecieæ odrobinê szybciej dla dynamiki

        while (time < 1)
        {
            time += Time.deltaTime / speed;
            if (tracer != null)
            {
                tracer.transform.position = Vector3.Lerp(startPoint, target, time);
            }
            yield return null;
        }

        Destroy(tracer);
    }
}