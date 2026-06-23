using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShotgunShooting : MonoBehaviour
{
    [Header("Shotgun Stats")]
    public float damagePerPellet = 7f;
    public int pelletCount = 8;
    public float range = 25f;
    public float spreadSpread = 0.1f;
    public LayerMask ignoreLayer;

    public float fireRate = 0.8f;
    private float nextFireTime = 0f;

    [Header("Weapon Visuals & Animation")]
    public Animator gunAnimator;
    public string shootAnimationName = "Shotgun_Shoot";
    public Transform muzzlePoint;

    [Header("Custom Effects (Prefabs)")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;

    private Camera mainCamera;
    private WeaponManager weaponManager; // Odnośnik do managera

    void Start()
    {
        mainCamera = Camera.main;
        weaponManager = WeaponManager.Instance; // Pobranie instancji managera
        if (mainCamera == null)
        {
            Debug.LogError("BŁĄD: Nie znaleziono obiektu z tagiem 'MainCamera'!");
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time >= nextFireTime)
            {
                // SPRAWDZENIE AMUNICJI PRZED STRZAŁEM
                if (weaponManager != null && weaponManager.HasAmmo(1))
                {
                    Debug.Log("STRZAŁ Z SHOTGUNA: " + gameObject.name);
                    ShootShotgun();

                    // ZUŻYCIE AMUNICJI
                    weaponManager.UseAmmo(1);

                    nextFireTime = Time.time + fireRate;
                }
            }
        }
    }

    void ShootShotgun()
    {
        if (mainCamera == null) return;

        if (gunAnimator != null)
        {
            gunAnimator.ResetTrigger("Fire");
            gunAnimator.Play(shootAnimationName, 0, 0f);
        }

        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            flash.transform.SetParent(muzzlePoint);
            Destroy(flash, 0.05f);
        }

        Vector3 rayOrigin = mainCamera.transform.position;
        Vector3 baseDirection = mainCamera.transform.forward;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 spreadFactor = mainCamera.transform.right * Random.Range(-spreadSpread, spreadSpread)
                                 + mainCamera.transform.up * Random.Range(-spreadSpread, spreadSpread);

            Vector3 pelletDirection = (baseDirection + spreadFactor).normalized;

            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(rayOrigin, pelletDirection, out hit, range, ~ignoreLayer))
            {
                targetPoint = hit.point;

                IMonster monster = hit.transform.GetComponentInParent<IMonster>();
                if (monster != null)
                {
                    monster.TakeDamage(damagePerPellet);
                }
            }
            else
            {
                targetPoint = rayOrigin + pelletDirection * range;
            }

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
        float speed = 0.02f;

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