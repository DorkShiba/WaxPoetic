using System;
using UnityEngine;
using Interfaces;
using Systems;

namespace Domain.Interactables
{
    /// <summary>
    /// Handles coin rewards when the player collects it through proximity detection.
    /// Implements ICollectible.
    /// </summary>
    public class CoinPickup : MonoBehaviour, ICollectible
    {
        [SerializeField] private int coinAmount = 1;
        [SerializeField] private AudioClip pickupSound;

        /// <summary>
        /// Event fired when any coin is collected. Passes coin amount.
        /// </summary>
        public static event Action<int> OnCoinCollected;

        /// <summary>
        /// Event fired when a coin is collected. Passes (CoinPickup instance, collector GameObject).
        /// </summary>
        public static event Action<CoinPickup, GameObject> OnCoinPickedUp;

        public int CoinAmount => coinAmount;

        public void Collect(GameObject collector)
        {
            OnCoinCollected?.Invoke(coinAmount);
            OnCoinPickedUp?.Invoke(this, collector);

            if (pickupSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(pickupSound);
            }

            Debug.Log($"[CoinPickup] {collector.name} collected {coinAmount} coin(s).");
            Destroy(gameObject);
        }

    }
}
