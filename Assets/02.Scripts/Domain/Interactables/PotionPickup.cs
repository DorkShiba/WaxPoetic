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
        [SerializeField] private float hpHealAmount = 25f;
        [SerializeField] private float staminaHealAmount = 25f;
        [SerializeField] private AudioClip pickupSound;

        /// <summary>
        /// Event fired when any potion is collected. Passes (hpHealAmount, staminaHealAmount).
        /// </summary>
        public static event Action<float, float> OnPotionCollected;

        /// <summary>
        /// Event fired when a potion is collected. Passes (PotionPickup instance, collector GameObject).
        /// </summary>
        public static event Action<PotionPickup, GameObject> OnPotionPickedUp;

        public float HPHealAmount => hpHealAmount;
        public float StaminaHealAmount => staminaHealAmount;

        public void Collect(GameObject collector)
        {
            OnPotionCollected?.Invoke(hpHealAmount, staminaHealAmount);
            OnPotionPickedUp?.Invoke(this, collector);

            PlayerController player = collector.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Heal(hpHealAmount);
                player.RestoreStamina(staminaHealAmount);
            }

            if (pickupSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(pickupSound);
            }

            Debug.Log($"[PotionPickup] {collector.name} collected potion (+{hpHealAmount} HP, +{staminaHealAmount} Stamina).");
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
