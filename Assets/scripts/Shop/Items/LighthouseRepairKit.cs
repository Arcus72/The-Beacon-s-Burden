using UnityEngine;

[CreateAssetMenu(fileName = "Lighthouse Repair Kit Data", menuName = "Inventory/Items/Lighthouse Repair Kit")]
public class LighthouseRepairKit : BaseItem
{
    public int RepairAmount = 50;

    public override void UseItem()
    {
        if (LighthouseScript.Instance)
        {
            LighthouseScript.Instance.Repair(RepairAmount);
            Debug.Log($"Naprawiono latarnię o {RepairAmount} punktów!"); 
        }
    }
}