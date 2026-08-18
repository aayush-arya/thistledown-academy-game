using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ThistledownHollowBell.Save;

// Flat, JSON-serializable snapshot of everything a save slot needs. Kept
// deliberately dumb (no Godot types) so it round-trips through
// System.Text.Json without custom converters.
public class SaveData
{
	[JsonPropertyName("saveVersion")]
	public int SaveVersion { get; set; } = 1;

	[JsonPropertyName("day")]
	public int Day { get; set; } = 1;

	[JsonPropertyName("slot")]
	public string Slot { get; set; } = "Morning";

	[JsonPropertyName("act")]
	public int Act { get; set; } = 1;

	[JsonPropertyName("flags")]
	public List<string> Flags { get; set; } = new();

	[JsonPropertyName("unlockedLocations")]
	public List<string> UnlockedLocations { get; set; } = new();

	[JsonPropertyName("discoveredClues")]
	public List<string> DiscoveredClues { get; set; } = new();

	// Corkboard connections, each stored as "clueIdA|clueIdB" (alphabetical).
	[JsonPropertyName("corkboardConnections")]
	public List<string> CorkboardConnections { get; set; } = new();

	[JsonPropertyName("relationships")]
	public Dictionary<string, int> Relationships { get; set; } = new();
}
