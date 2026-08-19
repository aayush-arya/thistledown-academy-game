using Godot;
using ThistledownHollowBell.Managers;

namespace ThistledownHollowBell.Player;

// A door/path between locations. Attach to an Area3D (with a
// CollisionShape3D child), same convention as CluePickup. If
// RequiredLocationId is unlocked on GameManager, interacting changes
// scene; otherwise the prompt itself explains why it's not accessible yet
// — no popup, no hard block, just nothing happens on interact.
public partial class LocationTransition : Area3D, IInteractable
{
	[Export] public string RequiredLocationId = "";
	[Export] public string TargetScenePath = "";
	[Export] public string UnlockedPrompt = "Go";
	[Export] public string LockedPrompt = "Not yet — you don't know where to look.";

	public string GetInteractionPrompt() =>
		string.IsNullOrEmpty(RequiredLocationId) || (GameManager.Instance?.IsLocationUnlocked(RequiredLocationId) ?? false)
			? UnlockedPrompt
			: LockedPrompt;

	public void Interact(Node interactor)
	{
		if (string.IsNullOrEmpty(TargetScenePath))
		{
			GD.PushWarning($"LocationTransition on {GetPath()} has no TargetScenePath set");
			return;
		}

		bool unlocked = string.IsNullOrEmpty(RequiredLocationId) ||
			(GameManager.Instance?.IsLocationUnlocked(RequiredLocationId) ?? false);
		if (!unlocked) return;

		GetTree().ChangeSceneToFile(TargetScenePath);
	}
}
