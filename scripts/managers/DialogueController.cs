using Godot;
using ThistledownHollowBell.Dialogue;
using ThistledownHollowBell.Player;
using ThistledownHollowBell.UI;

namespace ThistledownHollowBell.Managers;

// Drives one conversation at a time in a hub/location scene: instantiates
// the dialogue box, freezes player movement while it's open, and steps
// through DialogueManager's node graph as the player picks options. NPCs
// find this via the "dialogue_controller" group rather than a direct
// reference, so any NPC prefab works in any scene that has one of these.
public partial class DialogueController : Node
{
	[Export] public NodePath PlayerPath = "../Player";

	private const string DialogueBoxScenePath = "res://scenes/ui/dialogue_box.tscn";
	private const string ControllerGroup = "dialogue_controller";

	private PackedScene _dialogueBoxScene = null!;
	private PlayerController _player = null!;
	private DialogueBoxUI? _activeBox;

	public bool IsOpen => _activeBox != null;

	public override void _Ready()
	{
		_dialogueBoxScene = GD.Load<PackedScene>(DialogueBoxScenePath);
		_player = GetNode<PlayerController>(PlayerPath);
		AddToGroup(ControllerGroup);
	}

	public void StartDialogue(string startNodeId)
	{
		if (_activeBox != null) return;

		_activeBox = _dialogueBoxScene.Instantiate<DialogueBoxUI>();
		AddChild(_activeBox);
		_activeBox.OptionChosen += OnOptionChosen;
		_activeBox.CloseRequested += Close;

		_player.SetGameplayInputEnabled(false);

		AdvanceTo(startNodeId);
	}

	private void AdvanceTo(string nodeId)
	{
		var node = Dialogue.DialogueManager.Instance?.GetNode(nodeId);
		if (node == null)
		{
			GD.PushWarning($"DialogueController: no dialogue node '{nodeId}'");
			Close();
			return;
		}

		if (node.EndFlag != null)
		{
			GameManager.Instance?.SetFlag(node.EndFlag);
		}

		var visibleOptions = Dialogue.DialogueManager.Instance!.GetVisibleOptions(node);
		_activeBox?.ShowNode(node, visibleOptions);
	}

	private void OnOptionChosen(DialogueOption option)
	{
		var next = Dialogue.DialogueManager.Instance?.ChooseOption(option);
		if (next == null)
		{
			Close();
		}
		else
		{
			AdvanceTo(next);
		}
	}

	private void Close()
	{
		if (_activeBox != null)
		{
			_activeBox.OptionChosen -= OnOptionChosen;
			_activeBox.CloseRequested -= Close;
			_activeBox.QueueFree();
			_activeBox = null;
		}
		_player.SetGameplayInputEnabled(true);
	}
}
