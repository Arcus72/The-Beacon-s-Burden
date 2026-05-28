using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Lista Broni (w kolejnosci 1, 2, 3, 4)")]
    public GameObject[] weapons;
    private bool[] unlockedWeapons;

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
        unlockedWeapons = new bool[weapons.Length];
        unlockedWeapons[0] = true;
        for (int i = 1; i < weapons.Length; i++)
             unlockedWeapons[i] = false;
     
        // Na starcie aktywujemy tylko pierwsza bron (pistolet), reszte chowamy
        SelectWeapon(0);
    }

    public void AddWeapon(int weaponIndex){
        unlockedWeapons[weaponIndex] = true;
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
        if(!unlockedWeapons[index])
        return;
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

    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }
}