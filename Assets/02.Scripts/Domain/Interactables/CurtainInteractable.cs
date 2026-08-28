using UnityEngine;
using Interfaces;
using Systems;

namespace Domain.Interactables
{
    /// <summary>
    /// Curtain interactable object.
    /// Pressing the Interact key toggles curtain visual state (open/close) and updates blocking collider.
    /// Implements IInteractable.
    /// </summary>
    public class CurtainInteractable : MonoBehaviour, IInteractable
    {
        [Header("Curtain Settings")]
        [SerializeField] private bool isOpen = false;

        [Header("Components")]
        [SerializeField] private Animator animator;

        [Header("Audio")]
        [SerializeField] private AudioClip curtainSound;

        private static readonly int IsOpenHash = Animator.StringToHash("isOpen");

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            UpdateVisuals();
        }

        public void Interact(GameObject interactor)
        {
            isOpen = !isOpen;
            UpdateVisuals();

            if (curtainSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(curtainSound);
            }

            Debug.Log($"[CurtainInteractable] {gameObject.name} toggled state to: {(isOpen ? "Open" : "Closed")}");
        }

        private void UpdateVisuals()
        {
            if (animator != null)
            {
                animator.SetBool(IsOpenHash, isOpen);
            }
        }
    }
}
