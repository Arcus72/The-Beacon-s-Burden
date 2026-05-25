using UnityEngine;

public abstract class BaseItem : ScriptableObject
{
    [Header("Podstawowe Dane")]
    public string itemName;  
    public int price;  
    public ItemData price_type;     

    public abstract void UseItem();

    public string GetFullPrice(){
        return price.ToString() + " " + price_type.itemName;
    }
}