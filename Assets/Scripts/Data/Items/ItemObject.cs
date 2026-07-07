using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    public void Collect()
    {
        Destroy(gameObject);
    }
}