using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Base Weapon Stats")]
    public float damage = 10f;
    public float range = 100f;
    public Transform camTransform;
    public LayerMask ignoreLayer;

    [Header("Weapon Visuals & Animation")]
    public Animator gunAnimator;
    public Transform muzzlePoint;        // Pusty obiekt na koñcu lufy pistoletu

    [Header("Custom Effects (Prefabs)")]
    public GameObject muzzleFlashPrefab; // Twoje stworzone œwiat³o (Point Light)
    public GameObject bulletPrefab;      // Twoja stworzona smuga (Trail Renderer)

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
        // 1. ANIMACJA PISTOLETU
        if (gunAnimator != null)
        {
            // Resetuje parametr (na wszelki wypadek)
            gunAnimator.ResetTrigger("Fire");

            // Wymusza odpalenie animacji od stanu "Pistol_Shoot" od klatki 0, na warstwie 0 (domyœlnej)
            gunAnimator.Play("Pistol_Shoot", 0, 0f);
        }

        // 2. ROZB£YSK (Muzzle Flash)
        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            flash.transform.SetParent(muzzlePoint); // Przypina rozb³ysk do lufy
            Destroy(flash, 0.05f);
        }

        // Przygotowanie zmiennych do obs³ugi smugi pocisku
        RaycastHit hit;
        Vector3 targetPoint;

        // 3. LOGIKA TRAFIENIA (Raycast)
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, range, ~ignoreLayer))
        {
            targetPoint = hit.point; // Trafiliœmy w coœ – smuga poleci do tego punktu
            Debug.Log("TRAFIONO W: " + hit.transform.name);

            Monster monster = hit.transform.GetComponentInParent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage);
            }
        }
        else
        {
            // Nie trafiliœmy w nic – smuga leci przed siebie na maksymalny dystans broni
            targetPoint = camTransform.position + camTransform.forward * range;
        }

        // 4. SPAWN SMUGI POCISKU
        if (bulletPrefab != null && muzzlePoint != null)
        {
            GameObject tracer = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            // Odpalamy pomocnicz¹ funkcjê (Corutynê), która przesunie smugê do celu
            StartCoroutine(MoveTracer(tracer, targetPoint));
        }
    }

    // Pomocnicza funkcja przesuwaj¹ca smugê w czasie rzeczywistym
    private System.Collections.IEnumerator MoveTracer(GameObject tracer, Vector3 target)
    {
        Vector3 startPoint = tracer.transform.position;
        float time = 0;
        float speed = 0.03f; // Czas dolotu (im mniejsza liczba, tym szybszy pocisk)

        while (time < 1)
        {
            time += Time.deltaTime / speed;
            if (tracer != null)
            {
                tracer.transform.position = Vector3.Lerp(startPoint, target, time);
            }
            yield return null; // Czekamy na kolejn¹ klatkê
        }

        Destroy(tracer); // Niszczymy smugê, gdy dotrze na miejsce
    }
}