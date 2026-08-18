using Godot;
using System.Collections.Generic;
using System.Text.Json;
using ThistledownHollowBell.Clues;
using ThistledownHollowBell.Managers;
using ThistledownHollowBell.Relationships;

namespace ThistledownHollowBell.Dialogue;

// Loads every data/dialogue/*.json file and resolves which options are
// currently visible for a given node based on flags/clues/relationships.
// The actual dialogue-box UI (Phase 3) drives a conversation through
// GetNode/GetVisibleOptions/ChooseOption; this class owns no UI state.
public partial class DialogueManager : Node
{
	public static DialogueManager Instance { get; private set; } = null!;

	[Signal]
	public delegate void DialogueEndedEventHandler(string finalNodeId);

	private const string DialogueDataDir = "res://data/dialogue";

	private readonly Dictionary<string, DialogueNode> _nodes = new();

	public override void _Ready()
	{
		Instance = this;
		LoadAllDialogueFiles();
	}

	private void LoadAllDialogueFiles()
	{
		using var dir = DirAccess.Open(DialogueDataDir);
		if (dir == null)
		{
			GD.PushWarning($"DialogueManager: could not open {DialogueDataDir}");
			return;
		}

		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (fileName != "")
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
			{
				LoadDialogueFile($"{DialogueDataDir}/{fileName}");
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
	}

	private void LoadDialogueFile(string path)
	{
		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PushWarning($"DialogueManager: failed to open {path}");
			return;
		}

		string json = file.GetAsText();
		DialogueFile? parsed;
		try
		{
			parsed = JsonSerializer.Deserialize<DialogueFile>(json);
		}
		catch (JsonException e)
		{
			GD.PushError($"DialogueManager: malformed JSON in {path}: {e.Message}");
			return;
		}

		if (parsed == null) return;

		foreach (var node in parsed.Nodes)
		{
			if (_nodes.ContainsKey(node.Id))
			{
				GD.PushWarning($"DialogueManager: duplicate node id '{node.Id}' in {path}");
				continue;
			}
			_nodes[node.Id] = node;
		}
	}

	public DialogueNode? GetNode(string id) => _nodes.TryGetValue(id, out var n) ? n : null;

	public List<DialogueOption> GetVisibleOptions(DialogueNode node)
	{
		var visible = new List<DialogueOption>();
		foreach (var opt in node.Options)
		{
			if (opt.RequiresFlag != null && GameManager.Instance?.HasFlag(opt.RequiresFlag) != true)
				continue;
			if (opt.RequiresClue != null && ClueDatabase.Instance?.IsDiscovered(opt.RequiresClue) != true)
				continue;
			if (opt.RequiresRelationship != null &&
				RelationshipManager.Instance?.MeetsThreshold(opt.RequiresRelationship, opt.RequiresRelationshipValue) != true)
				continue;

			visible.Add(opt);
		}
		return visible;
	}

	// Applies an option's effects and returns the id of the next node, or
	// null if the conversation should end.
	public string? ChooseOption(DialogueOption option)
	{
		if (option.SetFlag != null)
			GameManager.Instance?.SetFlag(option.SetFlag);

		if (option.RelationshipTarget != null && option.RelationshipDelta != 0)
			RelationshipManager.Instance?.AddValue(option.RelationshipTarget, option.RelationshipDelta);

		if (option.GivesClue != null)
			ClueDatabase.Instance?.DiscoverClue(option.GivesClue);

		if (option.Next == null)
		{
			EmitSignal(SignalName.DialogueEnded, option.Text);
			return null;
		}

		return option.Next;
	}
}
