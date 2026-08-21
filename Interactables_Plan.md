# Interactables System Specifications & Plan

This document details the specifications and current implementation status for all 6 interactables in the project.

---

## 1. Interactable Specifications & Requirements

| Interactable | Trigger Condition | Expected Action / Behavior |
| :--- | :--- | :--- |
| **Coin** | Player overlaps | Player acquires item/currency automatically. |
| **Potion** | Player overlaps | Player acquires potion (or restores HP/Stamina). |
| **Monster** | Player attack / Proximity | Player can attack monster (`IDamageable`); monster can attack player (`IDamageable`). |
| **Door** | Proximity + Interact Key | Goes near door $\rightarrow$ Door opens visually.<br>Presses Interact Key $\rightarrow$ Player teleports to target destination. |
| **Curtain** | Proximity + Interact Key | Presses Interact Key near curtain $\rightarrow$ Curtain opens (toggles state/collision). |
| **NPC** | Proximity + Interact Key | Presses Interact Key near NPC $\rightarrow$ Player and NPC talk (triggers dialogue). |

---

## 2. Current Codebase Audit Status

- **Coin**: Partially implemented in `PlayerController.cs` and `ItemObject.cs`.
- **Potion**: Not implemented. Needs potion item/pickup component.
- **Monster**: Player attacks enemy via `HitboxController` -> `DummyEnemy.cs` (`IDamageable`). Enemy attacks on player and `PlayerController` implementing `IDamageable` are missing.
- **Door**: Asset `obj_door.prefab` exists. Proximity opening and teleport script (`DoorInteractable`) missing.
- **Curtain**: Not implemented. `CurtainInteractable` script missing.
- **NPC**: Only `Debug.Log("NPC detected")` exists in `PlayerController.cs`. Dialogue system / `NPCInteractable` missing.

---

## 3. Proposed Architecture for Future Implementation

### Interfaces (`Assets/Scripts/Interfaces/`)
* **`ICollectible`**: For automatic overlap loot (Coins, Potions).
* **`IInteractable`**: For key-press interactions (Doors, Curtains, NPCs).

### Class Layout
* `DoorInteractable.cs`: Handles `OnTriggerEnter2D` (open/close) and `Interact()` (teleport transform).
* `CurtainInteractable.cs`: Handles `Interact()` (toggle visual state & blocking collider).
* `NPCInteractable.cs`: Handles `Interact()` (starts dialogue lines/UI).
* `PlayerController.cs`: Updated to implement `IDamageable` and interact with `ICollectible` / `IInteractable`.

---
*Saved for future implementation.*
