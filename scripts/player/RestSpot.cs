using Godot;
using ThistledownHollowBell.Managers;

namespace ThistledownHollowBell.Player;

// A real, in-fiction way to advance the day/night slot — sitting down,
// waiting, moving on to the next thing — rather than relying on the
// debug hotkey. Attach to an Area3D (with a CollisionShape3D child),
// same convention as CluePickup.
public partial class RestSpot : Area3D, IInteractable
{
	[Export] public string ActivityLabel = "Wait";

	public string GetInteractionPrompt()
	{
		if (DayNightManager.Instance == null) return ActivityLabel;

		DaySlot next = NextSlot(DayNightManager.Instance.CurrentSlot);
		return next == DaySlot.Morning
			? $"{ActivityLabel} (turn in for the night)"
			: $"{ActivityLabel} (until {next})";
	}

	public void Interact(Node interactor)
	{
		DayNightManager.Instance?.AdvanceSlot();
	}

	private static DaySlot NextSlot(DaySlot current) => current switch
	{
		DaySlot.Morning => DaySlot.Afternoon,
		DaySlot.Afternoon => DaySlot.Dusk,
		DaySlot.Dusk => DaySlot.Night,
		_ => DaySlot.Morning,
	};
}
