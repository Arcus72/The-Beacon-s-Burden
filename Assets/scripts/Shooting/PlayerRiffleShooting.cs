using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRifleShooting : MonoBehaviour
{
    [Header("Rifle Stats")]
    public float damage = 12f;           // Obra�enia jednego pocisku
    public float range = 10f;           // Du�y zasi�g karabinu
    public float fireRate = 0.1f;        // Przerwa mi�dzy strza�ami w sekundach (0.1s = 10 strza��w na sekund�!)
    public float spread = 0.03f;         // Delikatny rozrzut automatyczny przy serii
    public LayerMask ignoreLayer;

    [Header("Weapon Visuals & Animation")]
    public Animator gunAnimator;
    public string shootAnimationName = "Rifle_Shoot"; // Nazwa animacji strza�u karabinu
    public Transform muzzlePoint;        // Koniec lufy karabinu

    [Header("Custom Effects (Prefabs)")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;

    private Camera mainCamera;
    private float nextTimeToFire = 0f;   // Licznik odmierzaj�cy czas do kolejnego strza�u
    private bool isShooting = false;     // Czy gracz trzyma przycisk strza�u

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("B��D: Nie znaleziono obiektu z tagiem 'MainCamera'!");
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // Sprawdzamy, czy gracz W�A�NIE NACISN�� lub TRZYMA lewy przycisk myszy
        if (Mouse.current.leftButton.isPressed)
        {
            // Sprawdzamy, czy min�o do�� czasu od ostatniego strza�u (funkcja Time.time)
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + fireRate; // Ustawiamy czas kolejnego strza�u
                ShootRifle();
            }
        }
    }

    void ShootRifle()
    {
        if (mainCamera == null) return;

        // 1. ANIMACJA KARABINU
        if (gunAnimator != null)
        {
            gunAnimator.ResetTrigger("Fire");
            gunAnimator.Play(shootAnimationName, 0, 0f);
        }

        // 2. ROZB�YSK (Muzzle Flash)
        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            flash.transform.SetParent(muzzlePoint);
            Destroy(flash, 0.05f);
        }

        // Przygotowanie wektor�w do strza�u z losowym rozrzutem serii
        Vector3 rayOrigin = mainCamera.transform.position;
        Vector3 baseDirection = mainCamera.transform.forward;

        // Ma�y rozrzut imituj�cy odrzut karabinu (Recoil/Spread)
        Vector3 spreadFactor = mainCamera.transform.right * Random.Range(-spread, spread)
                             + mainCamera.transform.up * Random.Range(-spread, spread);

        Vector3 rifleDirection = (baseDirection + spreadFactor).normalized;

        RaycastHit hit;
        Vector3 targetPoint;

        // 3. LOGIKA TRAFIENIA (Raycast)
        if (Physics.Raycast(rayOrigin, rifleDirection, out hit, range, ~ignoreLayer))
        {
            targetPoint = hit.point;
            Debug.Log("KARABIN TRAFI� W: " + hit.transform.name);

            IMonster monster = hit.transform.GetComponentInParent<IMonster>();
            if (monster != null)
            {
                monster.TakeDamage(damage);
            }
        }
        else
        {
            targetPoint = rayOrigin + rifleDirection * range;
        }

        // 4. SPAWN SMUGI POCISKU
        if (bulletPrefab != null && muzzlePoint != null)
        {
            GameObject tracer = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
            StartCoroutine(MoveTracer(tracer, targetPoint));
        }
    }

    private System.Collections.IEnumerator MoveTracer(GameObject tracer, Vector3 target)
    {
        Vector3 startPoint = tracer.transform.position;
        float time = 0;
        float speed = 0.015f; // Pociski z karabinu lataj� najszybciej, wr�cz b�yskawicznie

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