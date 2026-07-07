using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    // itemID -> count
    private Dictionary<int, int> _items = new Dictionary<int, int>();
    private Dictionary<int, string> _itemNames = new Dictionary<int, string>();

    public void AddItem(ItemData itemData, int amount = 1)
    {
        if (!_items.ContainsKey(itemData.itemID))
        {
            _items[itemData.itemID] = 0;
            _itemNames[itemData.itemID] = itemData.itemName;
        }

        _items[itemData.itemID] += amount;
        Debug.Log($"[Inventory] {itemData.itemName} collected! Total {itemData.itemName}: {_items[itemData.itemID]}");
    }

    public int GetCount(int itemID)
    {
        return _items.TryGetValue(itemID, out int count) ? count : 0;
    }
}