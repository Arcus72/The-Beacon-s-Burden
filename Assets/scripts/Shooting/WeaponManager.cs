using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Lista Broni (w kolejnosci 1, 2, 3, 4)")]
    public GameObject[] weapons;

    // Zamiast tablicy bool, stan odblokowania zale�y teraz od ilo�ci amunicji!
    private int[] weaponAmmo;

    private int currentWeaponIndex = 0;

    public static WeaponManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        weaponAmmo = new int[weapons.Length];

        // Pistolet (indeks 0) na starcie ma niesko�czono��, reszta ma 0 naboi
        weaponAmmo[0] = 9999;
        for (int i = 1; i < weapons.Length; i++)
            weaponAmmo[i] = 0;

        SelectWeapon(0);
    }

    // Metoda wywo�ywana przez sklep
    public void AddAmmo(int weaponIndex, int amount)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Length) return;

        // Je�li gracz nie mia� amunicji, zakup odblokowuje bro�
        weaponAmmo[weaponIndex] += amount;

        // Automatycznie wyci�gamy nowo kupion� bro�, dla wygody gracza
        SelectWeapon(weaponIndex);
    }

    // Metoda zu�ywaj�ca amunicj� przy strzale
    public void UseAmmo(int weaponIndex)
    {
        if (weaponIndex == 0) return; // Pistolet ma niesko�czono��

        if (weaponIndex >= 0 && weaponIndex < weaponAmmo.Length)
        {
            weaponAmmo[weaponIndex]--;
            Debug.Log($"Bro� {weaponIndex} pozosta�a amunicja: {weaponAmmo[weaponIndex]}");

            if (weaponAmmo[weaponIndex] <= 0)
            {
                weaponAmmo[weaponIndex] = 0;
                Debug.Log("Koniec amunicji! Powr�t do pistoletu.");
                SelectWeapon(0); // Wymuszony powr�t do pistoletu
            }
        }
    }

    // Metoda sprawdzaj�ca, czy bro� posiada naboje
    public bool HasAmmo(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponAmmo.Length) return false;
        return weaponAmmo[weaponIndex] > 0;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Wykrywanie klikni�cia klawiszy 1, 2, 3, 4 � z warunkiem posiadania amunicji
        if (keyboard.digit1Key.wasPressedThisFrame) { TrySelectWeapon(0); }
        if (keyboard.digit2Key.wasPressedThisFrame) { TrySelectWeapon(1); }
        if (keyboard.digit3Key.wasPressedThisFrame) { TrySelectWeapon(2); }
        if (keyboard.digit4Key.wasPressedThisFrame) { TrySelectWeapon(3); }
    }

    void TrySelectWeapon(int index)
    {
        if (HasAmmo(index))
        {
            SelectWeapon(index);
        }
        else
        {
            Debug.Log($"Nie mo�esz wybra� broni {index} � brak amunicji!");
        }
    }

    void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length || weapons[index] == null)
        {
            Debug.LogWarning("Brak przypisanej broni na indeksie: " + index);
            return;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].SetActive(i == index);
            }
        }

        currentWeaponIndex = index;
        Debug.Log("Wybrano bron: " + weapons[index].name);
    }

    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }

    public int GetCurrentAmmo(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weaponAmmo.Length)
        {
            return weaponAmmo[weaponIndex];
        }
        return 0;
    }


    public void Restart()
    {
        weaponAmmo = new int[weapons.Length];
        weaponAmmo[0] = 9999;

        currentWeaponIndex = 0;
        SelectWeapon(0);
    }
}