using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InventoryManager>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public int quantity;
        public Sprite icon;

        public InventoryItem(string name, int qty, Sprite itemIcon)
        {
            itemName = name;
            quantity = qty;
            icon = itemIcon;
        }
    }

    public List<InventoryItem> inventory = new List<InventoryItem>();

    public void AddItem(string itemName, int quantity, Sprite icon)
    {
        // Check if item already exists in inventory
        InventoryItem existingItem = inventory.Find(item => item.itemName == itemName);
        
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
        }
        else
        {
            inventory.Add(new InventoryItem(itemName, quantity, icon));
        }

        Debug.Log($"Added {quantity} {itemName}(s) to inventory");
    }

    public bool RemoveItem(string itemName, int quantity)
    {
        InventoryItem existingItem = inventory.Find(item => item.itemName == itemName);
        
        if (existingItem != null && existingItem.quantity >= quantity)
        {
            existingItem.quantity -= quantity;
            
            if (existingItem.quantity <= 0)
            {
                inventory.Remove(existingItem);
            }
            
            return true;
        }
        
        return false;
    }
} 