using System.Text.Json.Serialization;

namespace ThistledownHollowBell.Dialogue;

// Mirrors the schema of data/dialogue/*.json: a flat map of node id -> node,
// so branches can jump to any node by id without nested JSON. Each node is
// one speaker line plus a list of player response options.
public class DialogueNode
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = "";

	[JsonPropertyName("speaker")]
	public string Speaker { get; set; } = "";

	[JsonPropertyName("text")]
	public string Text { get; set; } = "";

	[JsonPropertyName("options")]
	public DialogueOption[] Options { get; set; } = System.Array.Empty<DialogueOption>();

	// If set and Options is empty, the conversation ends and this flag is set.
	[JsonPropertyName("endFlag")]
	public string? EndFlag { get; set; }
}

public class DialogueOption
{
	// Player line. Keep these short and flat per the lore bible's voice rule.
	[JsonPropertyName("text")]
	public string Text { get; set; } = "";

	[JsonPropertyName("next")]
	public string? Next { get; set; }

	// Gating: option only shown if these are satisfied.
	[JsonPropertyName("requiresFlag")]
	public string? RequiresFlag { get; set; }

	[JsonPropertyName("requiresClue")]
	public string? RequiresClue { get; set; }

	[JsonPropertyName("requiresRelationship")]
	public string? RequiresRelationship { get; set; }

	[JsonPropertyName("requiresRelationshipValue")]
	public int RequiresRelationshipValue { get; set; }

	// Effects applied when this option is chosen.
	[JsonPropertyName("setFlag")]
	public string? SetFlag { get; set; }

	[JsonPropertyName("relationshipDelta")]
	public int RelationshipDelta { get; set; }

	[JsonPropertyName("relationshipTarget")]
	public string? RelationshipTarget { get; set; }

	[JsonPropertyName("givesClue")]
	public string? GivesClue { get; set; }
}

public class DialogueFile
{
	[JsonPropertyName("nodes")]
	public DialogueNode[] Nodes { get; set; } = System.Array.Empty<DialogueNode>();
}
