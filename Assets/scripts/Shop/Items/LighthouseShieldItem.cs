using UnityEngine;

[CreateAssetMenu(fileName = "Lighthouse Shield Data", menuName = "Inventory/Items/Lighthouse Shield")]
public class LighthouseShieldItem : BaseItem
{
    public override void UseItem()
    {
        if (LighthouseScript.Instance)
        {
            LighthouseScript.Instance.RepairShield();
            Debug.Log($"Naprawiono latarnię"); 
        }
    }
}