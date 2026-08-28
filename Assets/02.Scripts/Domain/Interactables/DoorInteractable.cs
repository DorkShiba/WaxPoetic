using UnityEngine;
using Interfaces;
using Systems;
using Domain.Player;

namespace Domain.Interactables
{
    /// <summary>
    /// Door interactable object.
    /// Opens visually when the player is near (OnTriggerEnter2D/Exit2D).
    /// Teleports the player to a target destination transform when the Interact key is pressed.
    /// Implements IInteractable.
    /// </summary>
    public class DoorInteractable : MonoBehaviour, IInteractable
    {
        [Header("Destination Settings")]
        [SerializeField] private Transform destinationTransform;
        [SerializeField] private Vector3 offset = Vector3.zero;

        [Header("Visual Settings")]
        [SerializeField] private Animator animator;

        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip teleportSound;

        private static readonly int IsOpenHash = Animator.StringToHash("isOpen");

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            SetDoorVisualOpen(false);
        }

        public void Interact(GameObject interactor)
        {
            if (destinationTransform == null)
            {
                Debug.LogWarning($"[DoorInteractable] {gameObject.name} has no destinationTransform assigned!");
                return;
            }

            Vector3 targetPos = destinationTransform.position + offset;
            interactor.transform.position = targetPos;

            if (teleportSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(teleportSound);
            }

            Debug.Log($"[DoorInteractable] Teleported {interactor.name} to {targetPos}");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                SetDoorVisualOpen(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                SetDoorVisualOpen(false);
            }
        }

        private void SetDoorVisualOpen(bool open)
        {
            if (animator != null)
            {
                animator.SetBool(IsOpenHash, open);
            }

            if (open && openSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(openSound);
            }
        }
    }
}
