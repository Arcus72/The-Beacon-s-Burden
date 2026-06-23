using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Base Weapon Stats")]
    public float damage = 10f;
    public float range = 100f;
    public LayerMask ignoreLayer;

    [Header("Weapon Visuals & Animation")]
    public Animator gunAnimator;
    public Transform muzzlePoint;        // Pusty obiekt na końcu lufy pistoletu

    [Header("Custom Effects (Prefabs)")]
    public GameObject muzzleFlashPrefab; // Twoje stworzone światło (Point Light)
    public GameObject bulletPrefab;      // Twoja stworzona smuga (Trail Renderer)

    [Header("Weapon Audio")]
    public AudioSource pistolAudioSource; // Komponent AudioSource
    public AudioClip shootSound;          // Plik dźwiękowy wystrzału pistoletu

    private Camera mainCamera;           // Automatycznie znaleziona kamera gracza

    void Start()
    {
        // AUTOMATYCZNE ZABEZPIECZENIE: 
        // Skrypt sam szuka głównej kamery w grze, dzięki czemu celownik i strzał zawsze będą idealnie wyśrodkowane
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("BŁĄD: Nie znaleziono obiektu z tagiem 'MainCamera' w scenie! Upewnij się, że Twoja kamera ma ustawiony Tag jako MainCamera.");
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("KLIKNIĘCIE WYKRYTE NA BRONI: " + gameObject.name);
            Shoot();
        }
    }

    void Shoot()
    {
        if (mainCamera == null) return;

        // ODTWARZANIE DŹWIĘKU WYSTRZAŁU PISTOLETU
        if (pistolAudioSource != null && shootSound != null)
        {
            pistolAudioSource.PlayOneShot(shootSound);
        }

        // 1. ANIMACJA PISTOLETU
        if (gunAnimator != null)
        {
            gunAnimator.ResetTrigger("Fire");
            gunAnimator.Play("Pistol_Shoot", 0, 0f);
        }

        // 2. ROZBŁYSK (Muzzle Flash)
        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            flash.transform.SetParent(muzzlePoint); // Przypina rozbłysk do lufy
            Destroy(flash, 0.05f);
        }

        // Przygotowanie zmiennych do obsługi smugi pocisku
        RaycastHit hit;
        Vector3 targetPoint;

        // 3. LOGIKA TRAFIENIA (Raycast z pozycji i kierunku KAMERY, a nie pistoletu!)
        Vector3 rayOrigin = mainCamera.transform.position;
        Vector3 rayDirection = mainCamera.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, range, ~ignoreLayer))
        {
            targetPoint = hit.point; // Trafiliśmy w coś – smuga poleci do tego punktu
            Debug.Log("TRAFIONO W: " + hit.transform.name);

            IMonster monster = hit.transform.GetComponentInParent<IMonster>();
            if (monster != null)
            {
                monster.TakeDamage(damage);
            }
        }
        else
        {
            // Nie trafiliśmy w nic – smuga lecie przed siebie na maksymalny dystans broni
            targetPoint = rayOrigin + rayDirection * range;
        }

        // 4. SPAWN SMUGI POCISKU
        if (bulletPrefab != null && muzzlePoint != null)
        {
            GameObject tracer = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
            // Odpalamy pomocniczą funkcję (Corutynę), która przesunie smugę od lufy (muzzlePoint) do celu (targetPoint)
            StartCoroutine(MoveTracer(tracer, targetPoint));
        }
    }

    // Pomocnicza funkcja przesuwająca smugę w czasie rzeczywistym
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
            yield return null; // Czekamy na kolejną klatkę
        }

        Destroy(tracer); // Niszczymy smugę, gdy dotrze na miejsce
    }
}