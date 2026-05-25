using UnityEngine;


[CreateAssetMenu(fileName = "NewItem", menuName = "Loot/Item")]
public class ItemData : ScriptableObject
{
    public string itemName; 
    public Sprite icon;    
}