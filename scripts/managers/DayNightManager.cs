using Godot;

namespace ThistledownHollowBell.Managers;

public enum DaySlot
{
	Morning,
	Afternoon,
	Dusk,
	Night
}

// Drives the day loop: a handful of class/social slots per day, a dusk
// Omen Glass beat, and an optional night investigation window. Locations
// and dialogue read CurrentSlot to decide what's available; night slots
// carry a higher chance of a stealth encounter (see Phase 5).
public partial class DayNightManager : Node
{
	public static DayNightManager Instance { get; private set; } = null!;

	[Signal]
	public delegate void SlotChangedEventHandler(DaySlot newSlot);

	[Signal]
	public delegate void DayAdvancedEventHandler(int newDay);

	[Signal]
	public delegate void DuskReachedEventHandler();

	public int CurrentDay { get; private set; } = 1;
	public DaySlot CurrentSlot { get; private set; } = DaySlot.Morning;

	public override void _Ready()
	{
		Instance = this;
	}

	// Advances one slot; Night -> Morning rolls the day counter over.
	public void AdvanceSlot()
	{
		switch (CurrentSlot)
		{
			case DaySlot.Morning:
				CurrentSlot = DaySlot.Afternoon;
				break;
			case DaySlot.Afternoon:
				CurrentSlot = DaySlot.Dusk;
				EmitSignal(SignalName.DuskReached);
				break;
			case DaySlot.Dusk:
				CurrentSlot = DaySlot.Night;
				break;
			case DaySlot.Night:
				CurrentSlot = DaySlot.Morning;
				CurrentDay += 1;
				EmitSignal(SignalName.DayAdvanced, CurrentDay);
				break;
		}

		EmitSignal(SignalName.SlotChanged, (int)CurrentSlot);
	}

	// Used by the stealth system: getting spotted costs a day rather than
	// ending the game — jump straight to the next morning.
	public void ForceSendToDorm()
	{
		CurrentSlot = DaySlot.Morning;
		CurrentDay += 1;
		EmitSignal(SignalName.DayAdvanced, CurrentDay);
		EmitSignal(SignalName.SlotChanged, (int)CurrentSlot);
	}

	// NOTE: signal args are passed as (int) above — Godot's C# signal
	// marshalling wants Variant-compatible types, and int is the safe
	// choice for an enum payload. Handlers should cast back to DaySlot.

	public void LoadFromSave(int day, DaySlot slot)
	{
		CurrentDay = day;
		CurrentSlot = slot;
	}
}
