using UnityEngine;

namespace Interfaces
{
    /// <summary>
    /// Implemented by objects requiring active key-press interaction (e.g., Doors, Curtains, NPCs).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Triggered when an entity interacts with this object using the interaction key.
        /// </summary>
        /// <param name="interactor">The GameObject performing the interaction.</param>
        void Interact(GameObject interactor);
    }
}
