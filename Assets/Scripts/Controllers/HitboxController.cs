using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to a child GameObject that holds the attack Collider2D(s).
/// Animation Events call Enable/Disable on this to open/close the hitbox window.
/// </summary>
public class HitboxController : MonoBehaviour
{
    [SerializeField] private Collider2D hitbox;  // the trigger collider on this child object

    private float _damage;
    private HashSet<GameObject> _alreadyHit = new();  // prevents hitting the same enemy twice per swing

    void Awake()
    {
        if (hitbox == null)
            hitbox = GetComponent<Collider2D>();

        hitbox.isTrigger = true;
        hitbox.enabled = false;  // starts closed
    }

    /// <summary>
    /// Call from Animation Event (or AttackController) to open the hitbox.
    /// </summary>
    public void EnableHitbox(float damage)
    {
        Debug.Log($"[Hitbox] Enabled with {damage} dmg");
        _damage = damage;
        _alreadyHit.Clear();
        hitbox.enabled = true;
    }

    /// <summary>
    /// Call from Animation Event (or AttackController) to close the hitbox.
    /// </summary>
    public void DisableHitbox()
    {
        Debug.Log("[Hitbox] Disabled");
        hitbox.enabled = false;
        _alreadyHit.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (_alreadyHit.Contains(other.gameObject)) return;

        _alreadyHit.Add(other.gameObject);

        IDamageable damageable = other.GetComponent<IDamageable>();
        damageable?.TakeDamage(_damage);

        Debug.Log($"[Hitbox] Hit {other.gameObject.name} for {_damage} dmg");
    }
}
