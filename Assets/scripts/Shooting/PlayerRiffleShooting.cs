using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRifleShooting : MonoBehaviour
{
    [Header("Rifle Stats")]
    public float damage = 12f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public float spread = 0.03f;
    public LayerMask ignoreLayer;

    [Header("Weapon Visuals & Animation")]
    public Animator gunAnimator;
    public string shootAnimationName = "Rifle_Shoot";
    public Transform muzzlePoint;

    [Header("Custom Effects (Prefabs)")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletPrefab;

    [Header("Weapon Audio")]
    public AudioSource rifleAudioSource;
    public AudioClip shootSound;

    private Camera mainCamera;
    private WeaponManager weaponManager;
    private float nextTimeToFire = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        weaponManager = WeaponManager.Instance;
        if (mainCamera == null)
        {
            Debug.LogError("BŁĄD: Nie znaleziono obiektu z tagiem 'MainCamera'!");
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= nextTimeToFire)
            {
                if (weaponManager != null && weaponManager.HasAmmo(2))
                {
                    nextTimeToFire = Time.time + fireRate;
                    ShootRifle();
                    weaponManager.UseAmmo(2);
                }
            }
        }
    }

    void ShootRifle()
    {
        if (mainCamera == null) return;

        // ODTWARZANIE DŹWIĘKU WYSTRZAŁU
        if (rifleAudioSource != null && shootSound != null)
        {
            rifleAudioSource.PlayOneShot(shootSound);
        }

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

        Vector3 spreadFactor = mainCamera.transform.right * Random.Range(-spread, spread)
                             + mainCamera.transform.up * Random.Range(-spread, spread);

        Vector3 rifleDirection = (baseDirection + spreadFactor).normalized;

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(rayOrigin, rifleDirection, out hit, range, ~ignoreLayer))
        {
            targetPoint = hit.point;

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
        float speed = 0.015f;

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