using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ShopItemScript : MonoBehaviour
{
    public BaseItem itemToBuy;
    private bool isPlayerNearby = false;
    public TextMeshProUGUI nazwaTMP;
    public TextMeshProUGUI cenaTMP;

    [Header("Highlight Settings")]
    public GameObject targetHighlightObject;
    [ColorUsage(true, true)] // Pozwala na wybór intensywnego koloru HDR w edytorze
    public Color highlightColor = Color.white * 2f;

    private Renderer objectRenderer;
    private Color originalEmissionColor;
    private bool hasEmission = false;

    [Header("Sound Settings")]
    public AudioSource audioSource;

    void Start()
    {
        nazwaTMP.text = itemToBuy.itemName;
        cenaTMP.text = itemToBuy.GetFullPrice();

        if (targetHighlightObject != null)
        {
            objectRenderer = targetHighlightObject.GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                // Włączamy obsługę emisji na materiale i zapisujemy jej stan początkowy
                objectRenderer.material.EnableKeyword("_EMISSION");
                originalEmissionColor = objectRenderer.material.GetColor("_EmissionColor");
                hasEmission = true;
            }
        }
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current?.eKey.wasPressedThisFrame == true)
        {
            TryBuyItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            ToggleHighlight(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            ToggleHighlight(false);
        }
    }

    private void ToggleHighlight(bool shouldHighlight)
    {
        if (!hasEmission) return;

        // Zmieniamy tylko parametr odpowiedzialny za świecenie (Emission)
        if (shouldHighlight)
        {
            objectRenderer.material.SetColor("_EmissionColor", highlightColor);
        }
        else
        {
            objectRenderer.material.SetColor("_EmissionColor", originalEmissionColor);
        }
    }

    public void PlaySound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void TryBuyItem()
    {
        // ====================================================================
        // ZABEZPIECZENIE: SPRAWDZANIE LIMITU AMUNICJI (MAX 10 000)
        // ====================================================================
        if (itemToBuy is ShotgunItem weaponItem)
        {
            int index = weaponItem.weaponNumber;

            // Pobieramy aktualną amunicję bezpośrednio z managera broni
            if (WeaponManager.Instance != null)
            {
                // Pistolet (indeks 0) pomijamy, ale dla reszty sprawdzamy limit przed zakupem
                if (index > 0)
                {
                    // Określamy ile amunicji daje ten konkretny zakup
                    int ammoToAdd = 0;
                    if (index == 1) ammoToAdd = 12;
                    else if (index == 2) ammoToAdd = 45;
                    else if (index == 3) ammoToAdd = 3;

                    // Pobieramy informacje o obecnym stanie (musimy dodać małą publiczną pomoc do WeaponManager)
                    // Jeśli zakup sprawiłby, że przekroczymy 10 000, blokujemy transakcję
                    // (Założenie: Gracz ma już np. 9990 naboi)
                    if (WeaponManager.Instance.HasAmmo(index) &&
                        (index == 1 && WeaponManager.Instance.GetCurrentAmmo(1) + ammoToAdd > 10000 ||
                         index == 2 && WeaponManager.Instance.GetCurrentAmmo(2) + ammoToAdd > 10000 ||
                         index == 3 && WeaponManager.Instance.GetCurrentAmmo(3) + ammoToAdd > 10000))
                    {
                        Debug.Log("Masz już maksymalną ilość amunicji dla tej broni (Limit: 10 000)!");
                        return; // Przerywamy funkcję, gracz nie traci kasy, przedmiot nie jest kupowany
                    }
                }
            }
        }

        // ====================================================================
        // LOGIKA TRANSAKCJI (USUNIĘTO DESTROY)
        // ====================================================================
        if (PlayerInventory.Instance.TryRemoveItem(itemToBuy.price_type, itemToBuy.price))
        {
            PlaySound();
            itemToBuy.UseItem(); // Doda amunicję w WeaponManager

            // Usunięto: Destroy(gameObject); -> Stoisko zostaje w grze, można kupować ponownie!
            Debug.Log($"Pomyślnie zakupiono: {itemToBuy.itemName}. Stoisko jest gotowe na kolejny zakup.");
        }
    }
}