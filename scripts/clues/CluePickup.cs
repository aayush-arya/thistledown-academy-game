using Godot;
using ThistledownHollowBell.Player;

namespace ThistledownHollowBell.Clues;

// Drop this on an Area3D (with a CollisionShape3D child) placed in a level
// to make a world object grant a clue on interact. ClueId must match an
// id defined in data/clues/*.json.
public partial class CluePickup : Area3D, IInteractable
{
	[Export] public string ClueId = "";
	[Export] public string PromptOverride = "";

	public string GetInteractionPrompt()
	{
		if (!string.IsNullOrEmpty(PromptOverride)) return PromptOverride;

		var clue = ClueDatabase.Instance?.GetClue(ClueId);
		return clue != null ? $"Examine: {clue.Title}" : "Examine";
	}

	public void Interact(Node interactor)
	{
		if (string.IsNullOrEmpty(ClueId))
		{
			GD.PushWarning($"CluePickup on {GetPath()} has no ClueId set");
			return;
		}

		ClueDatabase.Instance?.DiscoverClue(ClueId);
	}
}
