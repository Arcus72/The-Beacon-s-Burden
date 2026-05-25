using UnityEngine;

[CreateAssetMenu(fileName = "Medical Kit Data", menuName = "Inventory/Items/Medical Kit")]
public class MedicalKitItem : BaseItem
{
    public int healthAmount = 50;

    public override void UseItem()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Player playerScript = player.GetComponent<Player>();
            playerScript.Heal(healthAmount);
            Debug.Log($"Uleczono gracza o {healthAmount} punktów!");
        }
    }
}