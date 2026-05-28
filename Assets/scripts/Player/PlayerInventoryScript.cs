using UnityEngine;
using System.Collections.Generic;
using TMPro; 

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();

    [Header("UI Reference")]
    public TextMeshProUGUI inventoryText;

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

    public void UpdateInventoryUI()
    {
        if (inventoryText == null) return; 
        if (items.Count == 0)
        {
            inventoryText.text = "Ekwipunek: Pusty";
            return;
        }

        string newText = "Zasoby:\n";

        foreach (KeyValuePair<ItemData, int> pair in items)
        {
            newText += $"{pair.Key.itemName}: x{pair.Value}\n";
        }

        inventoryText.text = newText;
    }
}