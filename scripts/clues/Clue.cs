using System.Text.Json.Serialization;

namespace ThistledownHollowBell.Clues;

// Mirrors the schema of data/clues/*.json. One entry per discoverable clue.
public class Clue
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = "";

	[JsonPropertyName("title")]
	public string Title { get; set; } = "";

	[JsonPropertyName("text")]
	public string Text { get; set; } = "";

	[JsonPropertyName("location")]
	public string Location { get; set; } = "";

	// Ids of other clues this one is intended to connect to on the
	// corkboard. Used by ClueDatabase to validate a player-drawn connection.
	[JsonPropertyName("connections")]
	public string[] Connections { get; set; } = System.Array.Empty<string>();

	// Optional: a flag set on GameManager when this connection pair is
	// first correctly drawn (drives location/scene unlocks).
	[JsonPropertyName("unlockFlag")]
	public string? UnlockFlag { get; set; }

	// Optional: relationship trust required before this clue can be found.
	[JsonPropertyName("requiresRelationship")]
	public string? RequiresRelationship { get; set; }

	[JsonPropertyName("requiresRelationshipValue")]
	public int RequiresRelationshipValue { get; set; }

	// "Not a physical object — the player's own conclusion" (see
	// docs/mystery_flowchart.md). A conclusion clue has no CluePickup or
	// dialogue option granting it directly; instead ClueDatabase discovers
	// it automatically once every clue in Connections has been found.
	[JsonPropertyName("isConclusion")]
	public bool IsConclusion { get; set; }

	// Optional: location id unlocked on GameManager when this clue is
	// successfully connected to another clue on the corkboard.
	[JsonPropertyName("unlocksLocation")]
	public string? UnlocksLocation { get; set; }
}

public class ClueFile
{
	[JsonPropertyName("clues")]
	public Clue[] Clues { get; set; } = System.Array.Empty<Clue>();
}
