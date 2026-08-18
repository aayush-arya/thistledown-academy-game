using Godot;
using System.Collections.Generic;

namespace ThistledownHollowBell.Managers;

// Central story-progression state: which act the player is in and which
// narrative flags have been set (e.g. "met_priya", "halloway_suspected").
// Other systems (dialogue gating, Omen Glass question pool, location
// unlocks) read flags from here rather than tracking their own copies.
public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; } = null!;

	[Signal]
	public delegate void FlagSetEventHandler(string flagName);

	public int CurrentAct { get; private set; } = 1;

	private readonly HashSet<string> _flags = new();
	private readonly HashSet<string> _unlockedLocations = new() { "greenhouse", "dorm", "courtyard", "dining_hall" };

	public override void _Ready()
	{
		Instance = this;
	}

	public bool HasFlag(string flagName) => _flags.Contains(flagName);

	public void SetFlag(string flagName)
	{
		if (_flags.Add(flagName))
		{
			EmitSignal(SignalName.FlagSet, flagName);
		}
	}

	public void ClearFlag(string flagName) => _flags.Remove(flagName);

	public void AdvanceAct(int act)
	{
		if (act > CurrentAct)
		{
			CurrentAct = act;
			SetFlag($"act_{act}_reached");
		}
	}

	public bool IsLocationUnlocked(string locationId) => _unlockedLocations.Contains(locationId);

	public void UnlockLocation(string locationId) => _unlockedLocations.Add(locationId);

	// -- persistence helpers, called by SaveManager --

	public IEnumerable<string> GetFlagsForSave() => _flags;

	public IEnumerable<string> GetUnlockedLocationsForSave() => _unlockedLocations;

	public void LoadFromSave(int act, IEnumerable<string> flags, IEnumerable<string> unlockedLocations)
	{
		CurrentAct = act;
		_flags.Clear();
		foreach (var f in flags) _flags.Add(f);
		_unlockedLocations.Clear();
		foreach (var l in unlockedLocations) _unlockedLocations.Add(l);
	}
}
