using UnityEngine;

public class ItemObject : MonoBehaviour, ICollectible
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    public void Collect(GameObject collector)
    {
        if (Managers.Inventory != null)
        {
            Managers.Inventory.AddItem(itemData, amount);
        }
        Destroy(gameObject);
    }
}