using System;
using UnityEngine;
using Interfaces;
using Domain.Player;
using Systems;

namespace Domain.Interactables
{
    /// <summary>
    /// Handles automatic potion pickup upon collision overlap.
    /// Implements ICollectible.
    /// </summary>
    public class PotionPickup : MonoBehaviour, ICollectible
    {
        [SerializeField] private float healAmount = 25f;
        [SerializeField] private AudioClip pickupSound;

        /// <summary>
        /// Event fired when any potion is collected. Passes heal amount.
        /// </summary>
        public static event Action<float> OnPotionCollected;

        /// <summary>
        /// Event fired when a potion is collected. Passes (PotionPickup instance, collector GameObject).
        /// </summary>
        public static event Action<PotionPickup, GameObject> OnPotionPickedUp;

        public float HealAmount => healAmount;

        public void Collect(GameObject collector)
        {
            OnPotionCollected?.Invoke(healAmount);
            OnPotionPickedUp?.Invoke(this, collector);

            PlayerController player = collector.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Heal(healAmount);
            }

            if (pickupSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(pickupSound);
            }

            Debug.Log($"[PotionPickup] {collector.name} collected potion (+{healAmount} HP).");
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                Collect(collision.gameObject);
            }
        }
    }
}
