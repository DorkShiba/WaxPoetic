using UnityEngine;
using Interfaces;
using Systems;
using Domain.Player;

namespace Domain.Interactables
{
    /// <summary>
    /// Door interactable object.
    /// Supports two-way teleportation (Outside <-> Inside) based on player position.
    /// Responds to PlayerController interaction detection for visual opening/closing.
    /// Implements IInteractable.
    /// </summary>
    public class DoorInteractable : MonoBehaviour, IInteractable
    {
        [Header("Destination Settings")]
        [SerializeField] private Transform outsideTransform;
        [SerializeField] private Transform insideTransform;

        [Header("Visual Settings")]
        [SerializeField] private Animator animator;

        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip teleportSound;

        private static readonly int IsOpenHash = Animator.StringToHash("isOpen");
        private bool _isOpen = false;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            SetDoorVisualOpen(false);
        }

        private void OnEnable()
        {
            PlayerController.OnInteractableDetected += HandleInteractableDetected;
        }

        private void OnDisable()
        {
            PlayerController.OnInteractableDetected -= HandleInteractableDetected;
            SetDoorVisualOpen(false);
        }

        public void Interact(GameObject interactor)
        {
            if (outsideTransform == null || insideTransform == null)
            {
                Debug.LogWarning($"[DoorInteractable] {gameObject.name} requires both outsideTransform and insideTransform to be assigned!");
                return;
            }

            Vector3 playerPos = interactor.transform.position;
            float distToOutside = Vector3.Distance(playerPos, outsideTransform.position);
            float distToInside = Vector3.Distance(playerPos, insideTransform.position);

            // If player is closer to outsideTransform, teleport to insideTransform; otherwise teleport to outsideTransform.
            Vector3 targetPosition = (distToOutside < distToInside) ? insideTransform.position : outsideTransform.position;

            interactor.transform.position = targetPosition;

            if (teleportSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(teleportSound);
            }

            Debug.Log($"[DoorInteractable] Teleported {interactor.name} to {targetPosition}");
        }

        private void HandleInteractableDetected(IInteractable detectedTarget)
        {
            bool isTarget = ReferenceEquals(detectedTarget, this) || 
                           (detectedTarget is Component comp && comp.gameObject == gameObject);
            SetDoorVisualOpen(isTarget);
        }

        private void SetDoorVisualOpen(bool open)
        {
            if (_isOpen == open) return;
            _isOpen = open;

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

