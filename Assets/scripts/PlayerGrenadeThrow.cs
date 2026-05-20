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
        weaponManager = GetComponentInParent<WeaponManager>();
        mainBodyRenderer = GetComponent<MeshRenderer>();
    }

    // ==========================================
    // NOWOŒÆ: ZABEZPIECZENIE PRZED ZMIAN¥ BRONI
    // ==========================================
    void OnEnable()
    {
        // Za ka¿dym razem, gdy gracz wyci¹gnie granat na nowo:
        isReadyToThrow = true; // Odblokowujemy mo¿liwoœæ rzutu

        // W³¹czamy z powrotem widocznoœæ cia³a
        if (mainBodyRenderer != null) mainBodyRenderer.enabled = true;

        // W³¹czamy z powrotem widocznoœæ wszystkich dodatkowych czêœci (Pin itp.)
        SetExtraPartsActive(true);
    }

    void Update()
    {
        if (!isReadyToThrow) return;
        if (weaponManager != null && weaponManager.GetCurrentWeaponIndex() != 3) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(ThrowSequence());
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

        // UKRYWANIE GRANATU
        if (mainBodyRenderer != null) mainBodyRenderer.enabled = false;
        SetExtraPartsActive(false);

        // Czekamy na odnowienie
        yield return new WaitForSeconds(regainDelay);

        // POWRÓT GRANATU DO RÊKI
        if (mainBodyRenderer != null) mainBodyRenderer.enabled = true;
        SetExtraPartsActive(true);

        isReadyToThrow = true;
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