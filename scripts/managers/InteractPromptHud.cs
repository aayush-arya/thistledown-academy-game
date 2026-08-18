using Godot;
using ThistledownHollowBell.Clues;
using ThistledownHollowBell.Player;

namespace ThistledownHollowBell.Managers;

// Minimal HUD: an interact prompt label plus a "clue found" toast.
// Placeholder until Phase 7 builds real UI theming and Phase 2 gives clues
// a proper journal/corkboard to land in.
public partial class InteractPromptHud : CanvasLayer
{
	[Export] public NodePath PlayerPath = "../Player";
	[Export] public NodePath LabelPath = "PromptLabel";
	[Export] public NodePath ToastLabelPath = "ToastLabel";
	[Export] public float ToastDuration = 2.5f;

	private Label _label = null!;
	private Label _toastLabel = null!;
	private Timer _toastTimer = null!;

	public override void _Ready()
	{
		_label = GetNode<Label>(LabelPath);
		_label.Visible = false;

		_toastLabel = GetNode<Label>(ToastLabelPath);
		_toastLabel.Visible = false;

		_toastTimer = new Timer { OneShot = true, WaitTime = ToastDuration };
		AddChild(_toastTimer);
		_toastTimer.Timeout += () => _toastLabel.Visible = false;

		var player = GetNode<PlayerController>(PlayerPath);
		player.InteractionTargetChanged += OnInteractionTargetChanged;

		if (ClueDatabase.Instance != null)
		{
			ClueDatabase.Instance.ClueDiscovered += OnClueDiscovered;
		}
	}

	private void OnInteractionTargetChanged(string promptText)
	{
		if (string.IsNullOrEmpty(promptText))
		{
			_label.Visible = false;
		}
		else
		{
			_label.Visible = true;
			_label.Text = $"[E] {promptText}";
		}
	}

	private void OnClueDiscovered(string clueId)
	{
		var clue = ClueDatabase.Instance?.GetClue(clueId);
		_toastLabel.Text = clue != null ? $"Clue found: {clue.Title}" : $"Clue found: {clueId}";
		_toastLabel.Visible = true;
		_toastTimer.Start();
	}
}
