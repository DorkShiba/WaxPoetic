using UnityEngine;

/// <summary>
/// Handles automatic coin pickup upon collision overlap.
/// Implements ICollectible.
/// </summary>
public class CoinPickup : MonoBehaviour, ICollectible
{
    [SerializeField] private int coinAmount = 1;
    [SerializeField] private AudioClip pickupSound;

    public int CoinAmount => coinAmount;

    public void Collect(GameObject collector)
    {
        if (pickupSound != null && Managers.Sound != null)
        {
            Managers.Sound.Play(pickupSound);
        }

        Debug.Log($"[CoinPickup] {collector.name} collected {coinAmount} coin(s).");
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
