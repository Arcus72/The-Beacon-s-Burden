using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUDDisplay : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlotUI
    {
        public Image slotBackground;      // Obiekt Slot1, Slot2 itd.
        public Sprite weaponIcon;         // Plik PNG broni
        public TextMeshProUGUI ammoText;
        public TextMeshProUGUI numberText;
    }

    [Header("Konfiguracja Slotów UI")]
    public WeaponSlotUI[] uiSlots;

    [Header("USTAWIENIA DLA WYBRANEJ BRONI (AKTYWNEJ)")]
    public Color activeBackground = new Color(1f, 1f, 1f, 0.4f); // Lekko przezroczyste tło slotu
    public Color activeWeaponAndText = new Color(1f, 1f, 1f, 1f); // 1f na końcu = 100% widoczności (zero przezroczystości!)
    public Color activeNumber = Color.yellow;

    [Header("USTAWIENIA DLA NIEWYBRANEJ BRONI (W EQ)")]
    public Color standbyBackground = new Color(0.3f, 0.3f, 0.3f, 0.2f);
    public Color standbyWeaponAndText = new Color(0.6f, 0.6f, 0.6f, 0.6f); // Przygaszona broń
    public Color standbyNumber = Color.white;

    [Header("USTAWIENIA DLA BRAKU AMUNICJI (WYSZARZONA)")]
    public Color inactiveColor = new Color(0.15f, 0.15f, 0.15f, 0.4f);

    void Update()
    {
        if (WeaponManager.Instance == null) return;

        int currentSelectedWeapon = WeaponManager.Instance.GetCurrentWeaponIndex();

        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (uiSlots[i] == null) continue;

            int ammo = WeaponManager.Instance.GetCurrentAmmo(i);
            bool hasAmmo = WeaponManager.Instance.HasAmmo(i);

            // Aktualizacja tekstów
            if (i == 0) uiSlots[i].ammoText.text = "∞";
            else uiSlots[i].ammoText.text = ammo.ToString();

            if (uiSlots[i].slotBackground != null && uiSlots[i].weaponIcon != null)
            {
                uiSlots[i].slotBackground.sprite = uiSlots[i].weaponIcon;
            }

            // Logika stanów i przezroczystości
            if (!hasAmmo && i != 0)
            {
                // Stan: Pusta
                SetSlotColors(uiSlots[i], inactiveColor, inactiveColor, inactiveColor);
            }
            else
            {
                if (i == currentSelectedWeapon)
                {
                    // Stan: Wybrana (Broń i napisy na 100% widoczności)
                    SetSlotColors(uiSlots[i], activeBackground, activeWeaponAndText, activeNumber);
                }
                else
                {
                    // Stan: Schowana w kieszeni
                    SetSlotColors(uiSlots[i], standbyBackground, standbyWeaponAndText, standbyNumber);
                }
            }
        }
    }

    private void SetSlotColors(WeaponSlotUI slot, Color bgCol, Color weaponAndTextCol, Color numCol)
    {
        if (slot.slotBackground != null) slot.slotBackground.color = bgCol;
        if (slot.ammoText != null) slot.ammoText.color = weaponAndTextCol;
        if (slot.numberText != null) slot.numberText.color = numCol;
    }
}