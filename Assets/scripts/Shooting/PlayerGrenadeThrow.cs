using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrenadeThrow : MonoBehaviour
{
    [Header("Grenade Throw Settings")]
    public GameObject flyingGrenadePrefab;
    public Transform throwPoint;
    public float throwForce = 15f;
    public float regainDelay = 1f;

    [Header("Weapon Visuals (Dodatkowe czêœci, np. Pin)")]
    public GameObject[] extraGrenadeParts;

    private WeaponManager weaponManager;
    private bool isReadyToThrow = true;
    private MeshRenderer mainBodyRenderer;

    void Start()
    {
        weaponManager = WeaponManager.Instance; // Pobranie instancji managera
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
            // SPRAWDZENIE CZY MAMY GRANATY (INDEKS 3)
            if (weaponManager != null && weaponManager.HasAmmo(3))
            {
                StartCoroutine(ThrowSequence());

                // ZU¯YCIE GRANATU
                weaponManager.UseAmmo(3);
            }
        }
    }

    private System.Collections.IEnumerator ThrowSequence()
    {
        isReadyToThrow = false;

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

        // POWRÓT TYLKO JEŒLI PO STRZALE NADAL MAMY JAKIŒ GRANAT W ZAPASIE
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