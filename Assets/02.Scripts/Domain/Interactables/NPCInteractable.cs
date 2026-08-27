using UnityEngine;

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

    public string NpcName => npcName;
    public string[] DialogueLines => dialogueLines;

    public void Interact(GameObject interactor)
    {
        Debug.Log($"[NPCInteractable] Interacting with NPC: {npcName}");

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
}
