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
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;

        [Header("Audio")]
        [SerializeField] private AudioClip curtainSound;

        private static readonly int IsOpenHash = Animator.StringToHash("isOpen");

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateVisualsAndCollision();
        }

        public void Interact(GameObject interactor)
        {
            isOpen = !isOpen;
            UpdateVisualsAndCollision();

            if (curtainSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(curtainSound);
            }

            Debug.Log($"[CurtainInteractable] {gameObject.name} toggled state to: {(isOpen ? "Open" : "Closed")}");
        }

        private void UpdateVisualsAndCollision()
        {
            if (blockingCollider != null)
            {
                blockingCollider.enabled = !isOpen;
            }

            if (animator != null)
            {
                animator.SetBool(IsOpenHash, isOpen);
            }
            else if (spriteRenderer != null && openSprite != null && closedSprite != null)
            {
                spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
            }
        }
    }
}
