using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Inventory/Items/Weapon")]
public class ShotgunItem : BaseItem
{
    public int weaponNumber;
    public override void UseItem()
    {
      WeaponManager.Instance.AddWeapon( weaponNumber);
      Debug.Log($"Dodano brań {weaponNumber}");
      
    }
}