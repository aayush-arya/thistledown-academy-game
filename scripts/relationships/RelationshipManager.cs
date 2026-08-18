using Godot;
using System.Collections.Generic;

namespace ThistledownHollowBell.Relationships;

// Small trust meters for Junie and Priya (see brief §6: "not a full
// dating-sim system, just enough to gate a few unique clues/scenes").
// Values are clamped 0-10. Dialogue and clue data reference character ids
// ("junie", "priya") and a required trust threshold.
public partial class RelationshipManager : Node
{
	public static RelationshipManager Instance { get; private set; } = null!;

	public const int MinValue = 0;
	public const int MaxValue = 10;

	[Signal]
	public delegate void RelationshipChangedEventHandler(string characterId, int newValue);

	private readonly Dictionary<string, int> _values = new()
	{
		{ "junie", 0 },
		{ "priya", 0 },
	};

	public override void _Ready()
	{
		Instance = this;
	}

	public int GetValue(string characterId) => _values.TryGetValue(characterId, out var v) ? v : 0;

	public bool MeetsThreshold(string characterId, int threshold) => GetValue(characterId) >= threshold;

	public void AddValue(string characterId, int delta)
	{
		int current = GetValue(characterId);
		int clamped = Mathf.Clamp(current + delta, MinValue, MaxValue);
		_values[characterId] = clamped;
		EmitSignal(SignalName.RelationshipChanged, characterId, clamped);
	}

	// -- persistence helpers, called by SaveManager --

	public IReadOnlyDictionary<string, int> GetValuesForSave() => _values;

	public void LoadFromSave(Dictionary<string, int> values)
	{
		foreach (var kvp in values)
		{
			_values[kvp.Key] = Mathf.Clamp(kvp.Value, MinValue, MaxValue);
		}
	}
}
