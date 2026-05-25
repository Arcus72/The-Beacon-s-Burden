using UnityEngine;

[CreateAssetMenu(fileName = "Shield Data", menuName = "Inventory/Items/Shield")]
public class ShieldItem : BaseItem
{
    public int shieldAmount = 50;

    public override void UseItem()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Player playerScript = player.GetComponent<Player>();
            playerScript.RepairShield(shieldAmount);
            Debug.Log($"Naprawiono tarcze");
        }
    }
}