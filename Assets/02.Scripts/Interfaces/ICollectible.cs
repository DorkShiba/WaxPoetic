using UnityEngine;

namespace Interfaces
{
    /// <summary>
    /// Implemented by objects collected automatically upon collision overlap (e.g., Coins, Potions).
    /// </summary>
    public interface ICollectible
    {
        /// <summary>
        /// Triggered when an entity (e.g. Player) overlaps and collects this object.
        /// </summary>
        /// <param name="collector">The GameObject collecting the item.</param>
        void Collect(GameObject collector);
    }
}
