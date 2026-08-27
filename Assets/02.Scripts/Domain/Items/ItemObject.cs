using UnityEngine;
using Interfaces;
using Domain.Player;
using GameData;
using Systems;

namespace Domain.Items
{
    public class ItemObject : MonoBehaviour, ICollectible
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount = 1;

        public ItemData ItemData => itemData;
        public int Amount => amount;

        public void Collect(GameObject collector)
        {
            if (Managers.Inventory != null && itemData != null)
            {
                Managers.Inventory.AddItem(itemData, amount);
            }
            Debug.Log($"[ItemObject] {collector.name} picked up item object.");
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                Collect(collision.gameObject);
            }
        }
    }
}