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
        if (PlayerInventory.Instance.TryRemoveItem(itemToBuy.price_type, itemToBuy.price)) 
        {
            PlaySound();
            itemToBuy.UseItem();
            Destroy(gameObject);
        }
    }
}