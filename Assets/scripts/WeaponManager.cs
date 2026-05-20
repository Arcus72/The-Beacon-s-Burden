using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Lista Broni (w kolejnosci 1, 2, 3, 4)")]
    public GameObject[] weapons;

    private int currentWeaponIndex = 0;

    void Start()
    {
        // Na starcie aktywujemy tylko pierwsza bron (pistolet), reszte chowamy
        SelectWeapon(0);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Wykrywanie klikniecia klawiszy 1, 2, 3, 4
        if (keyboard.digit1Key.wasPressedThisFrame) { SelectWeapon(0); }
        if (keyboard.digit2Key.wasPressedThisFrame) { SelectWeapon(1); }
        if (keyboard.digit3Key.wasPressedThisFrame) { SelectWeapon(2); }
        if (keyboard.digit4Key.wasPressedThisFrame) { SelectWeapon(3); }
    }

    void SelectWeapon(int index)
    {
        // Zabezpieczenie na wypadek, gdybysmy nie mieli jeszcze przypisanych wszystkich 4 broni
        if (index < 0 || index >= weapons.Length || weapons[index] == null)
        {
            Debug.LogWarning("Brak przypisanej broni na indeksie: " + index);
            return;
        }

        // Petla, ktora wylacza wszystkie bronie, a wlacza tylko te wybrana
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                // Jesli i jest rowne wybranemu indeksowi, wlacza obiekt (true), w przeciwnym razie wylacza (false)
                weapons[i].SetActive(i == index);
            }
        }

        currentWeaponIndex = index;
        Debug.Log("Wybrano bron: " + weapons[index].name);
    }
}