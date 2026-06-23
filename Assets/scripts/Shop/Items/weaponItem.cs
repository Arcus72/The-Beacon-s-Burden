using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Inventory/Items/Weapon")]
public class ShotgunItem : BaseItem
{
    public int weaponNumber; // 1 = Shotgun, 2 = M4, 3 = Granat

    public override void UseItem()
    {
        int ammoAmount = 0;
        if (weaponNumber == 1) ammoAmount = 12;
        else if (weaponNumber == 2) ammoAmount = 45;
        else if (weaponNumber == 3) ammoAmount = 3;

        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.AddAmmo(weaponNumber, ammoAmount);
            Debug.Log($"Zakupiono amunicję do broni {weaponNumber}. Dodano: {ammoAmount}");
        }
    }
}