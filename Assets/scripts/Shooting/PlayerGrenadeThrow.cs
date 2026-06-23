using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrenadeThrow : MonoBehaviour
{
    [Header("Grenade Throw Settings")]
    public GameObject flyingGrenadePrefab;
    public Transform throwPoint;
    public float throwForce = 15f;
    public float regainDelay = 1f;

    [Header("Weapon Visuals (Dodatkowe czÍúci, np. Pin)")]
    public GameObject[] extraGrenadeParts;

    [Header("Weapon Audio")]
    public AudioSource throwAudioSource;
    public AudioClip throwSound;

    private WeaponManager weaponManager;
    private bool isReadyToThrow = true;
    private MeshRenderer mainBodyRenderer;

    void Start()
    {
        weaponManager = WeaponManager.Instance;
        mainBodyRenderer = GetComponent<MeshRenderer>();
    }

    void OnEnable()
    {
        isReadyToThrow = true;
        if (mainBodyRenderer != null) mainBodyRenderer.enabled = true;
        SetExtraPartsActive(true);
    }

    void Update()
    {
        if (!isReadyToThrow) return;
        if (weaponManager != null && weaponManager.GetCurrentWeaponIndex() != 3) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (weaponManager != null && weaponManager.HasAmmo(3))
            {
                StartCoroutine(ThrowSequence());
                weaponManager.UseAmmo(3);
            }
        }
    }

    private System.Collections.IEnumerator ThrowSequence()
    {
        isReadyToThrow = false;

        // ODTWARZANIE DèWI KU RZUTU
        if (throwAudioSource != null && throwSound != null)
        {
            throwAudioSource.PlayOneShot(throwSound);
        }

        if (flyingGrenadePrefab != null && throwPoint != null)
        {
            GameObject grenade = Instantiate(flyingGrenadePrefab, throwPoint.position, throwPoint.rotation);
            Rigidbody rb = grenade.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);
            }
        }

        if (mainBodyRenderer != null) mainBodyRenderer.enabled = false;
        SetExtraPartsActive(false);

        yield return new WaitForSeconds(regainDelay);

        if (weaponManager != null && weaponManager.HasAmmo(3))
        {
            if (mainBodyRenderer != null) mainBodyRenderer.enabled = true;
            SetExtraPartsActive(true);
            isReadyToThrow = true;
        }
    }

    private void SetExtraPartsActive(bool state)
    {
        if (extraGrenadeParts == null) return;

        foreach (GameObject part in extraGrenadeParts)
        {
            if (part != null)
            {
                part.SetActive(state);
            }
        }
    }
}