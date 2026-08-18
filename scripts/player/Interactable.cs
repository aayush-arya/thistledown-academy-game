using Godot;

namespace ThistledownHollowBell.Player;

// Implemented by anything the player can interact with via the interact
// prompt (clue pickups, doors, NPCs that open dialogue). Nodes implementing
// this should be reachable from the player's interact raycast and be in the
// "interactable" group so PlayerController can find them.
public interface IInteractable
{
	// Short label shown in the interact prompt, e.g. "Read the clipping".
	string GetInteractionPrompt();

	void Interact(Node interactor);
}
