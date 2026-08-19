using Godot;
using ThistledownHollowBell.Player;

namespace ThistledownHollowBell.Dialogue;

// A world NPC the player can talk to. Attach to an Area3D (with a
// CollisionShape3D child) placed in a level, same convention as
// CluePickup, so the interact raycast can find it.
public partial class Npc : Area3D, IInteractable
{
	[Export] public string DisplayName = "";
	[Export] public string StartNodeId = "";

	public string GetInteractionPrompt() => string.IsNullOrEmpty(DisplayName) ? "Talk" : $"Talk to {DisplayName}";

	public void Interact(Node interactor)
	{
		if (string.IsNullOrEmpty(StartNodeId))
		{
			GD.PushWarning($"Npc on {GetPath()} has no StartNodeId set");
			return;
		}

		var controller = GetTree().GetFirstNodeInGroup("dialogue_controller") as Managers.DialogueController;
		if (controller == null)
		{
			GD.PushWarning("Npc: no DialogueController found in this scene (group 'dialogue_controller').");
			return;
		}

		if (controller.IsOpen) return;
		controller.StartDialogue(StartNodeId);
	}
}
