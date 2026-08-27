using System;
using UnityEngine;
using Interfaces;
using Systems;

namespace Domain.Interactables
{
    /// <summary>
    /// NPC interactable object.
    /// Pressing the Interact key triggers NPC dialogue lines.
    /// Implements IInteractable.
    /// </summary>
    public class NPCInteractable : MonoBehaviour, IInteractable
    {
        [Header("NPC Profile")]
        [SerializeField] private string npcName = "Villager";
        [TextArea(2, 5)]
        [SerializeField] private string[] dialogueLines;

        [Header("Audio")]
        [SerializeField] private AudioClip talkSound;

        /// <summary>
        /// Event fired when dialogue starts. Passes (NPC Name, Array of Dialogue Lines).
        /// UI systems can subscribe to display text on screen.
        /// </summary>
        public static event Action<string, string[]> OnDialogueTriggered;

        /// <summary>
        /// Event fired when dialogue conversation completes or is closed.
        /// </summary>
        public static event Action OnDialogueEnded;

        public string NpcName => npcName;
        public string[] DialogueLines => dialogueLines;

        public void Interact(GameObject interactor)
        {
            Debug.Log($"[NPCInteractable] Interacting with NPC: {npcName}");

            OnDialogueTriggered?.Invoke(npcName, dialogueLines);

            if (dialogueLines != null && dialogueLines.Length > 0)
            {
                for (int i = 0; i < dialogueLines.Length; i++)
                {
                    Debug.Log($"[NPC:{npcName}] line {i + 1}: {dialogueLines[i]}");
                }
            }
            else
            {
                Debug.Log($"[NPC:{npcName}] Hello, traveller!");
            }

            if (talkSound != null && Managers.Sound != null)
            {
                Managers.Sound.Play(talkSound);
            }
        }

        public void EndDialogue()
        {
            OnDialogueEnded?.Invoke();
        }
    }
}
