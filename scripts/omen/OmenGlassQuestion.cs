using System.Text.Json.Serialization;

namespace ThistledownHollowBell.Omen;

// Mirrors the schema of data/dialogue/omen_glass.json. The Glass never
// answers plainly — ResponseText should read as a fragment, not an
// instruction (see docs/mystery_flowchart.md's "Omen Glass question pool").
public class OmenGlassQuestion
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = "";

	[JsonPropertyName("promptText")]
	public string PromptText { get; set; } = "";

	[JsonPropertyName("responseText")]
	public string ResponseText { get; set; } = "";

	// Only offered once this flag is set; null means always available
	// (subject to not having been answered already).
	[JsonPropertyName("requiresFlag")]
	public string? RequiresFlag { get; set; }
}

public class OmenGlassFile
{
	[JsonPropertyName("questions")]
	public OmenGlassQuestion[] Questions { get; set; } = System.Array.Empty<OmenGlassQuestion>();
}
