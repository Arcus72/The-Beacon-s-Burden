using UnityEngine;
using System.Collections.Generic;
using TMPro; 

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();

    [Header("UI Reference")]
    public TextMeshProUGUI inventoryText;

    [Header("Cheats")]
    [Tooltip("When enabled, everything is free - items are never actually removed.")]
    public bool freeMode = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateInventoryUI(); 
    }

    public void RestartInventory(){
        items = new Dictionary<ItemData, int>();
        UpdateInventoryUI();
    }

    public void AddItem(ItemData item, int amount = 1)
    {
        if (items.ContainsKey(item))
            items[item] += amount;
        else
            items.Add(item, amount);
     
        Debug.Log($"Masz teraz {items[item]} sztuk: {item.itemName}");
        
        UpdateInventoryUI(); 
    }

  public bool TryRemoveItem(ItemData item, int amount)
{
    if (freeMode)
        return true;

    if (items.ContainsKey(item))
    {
        int itemAmount = items[item];

        if (amount > itemAmount) 
        {
            return false;
        }else if (amount == itemAmount)
        {
            items.Remove(item); 
             UpdateInventoryUI(); 
            return true;
        }


        items[item] -= amount;
        UpdateInventoryUI(); 
      
        return true;
    }

    return false;
}

    public void ToogleFreeShop(){
        freeMode = !freeMode;
    }

    public void UpdateInventoryUI()
    {
        if (inventoryText == null) return; 
        if (items.Count == 0)
        {
            inventoryText.text = "Inventory: Empty";
            return;
        }

        string newText = "Inventory:\n";

        foreach (KeyValuePair<ItemData, int> pair in items)
        {
            newText += $"{pair.Key.itemName}: x{pair.Value}\n";
        }

        inventoryText.text = newText;
    }
}