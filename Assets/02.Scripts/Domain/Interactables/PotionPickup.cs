using UnityEngine;

/// <summary>
/// Handles automatic potion pickup upon collision overlap.
/// Implements ICollectible.
/// </summary>
public class PotionPickup : MonoBehaviour, ICollectible
{
    [SerializeField] private float healAmount = 25f;
    [SerializeField] private AudioClip pickupSound;

    public float HealAmount => healAmount;

    public void Collect(GameObject collector)
    {
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
