using Godot;
using ThistledownHollowBell.Player;

namespace ThistledownHollowBell.Managers;

// Minimal HUD: a single label that shows/hides based on the player's
// current interact target. Placeholder until Phase 7 builds real UI theming.
public partial class InteractPromptHud : CanvasLayer
{
	[Export] public NodePath PlayerPath = "../Player";
	[Export] public NodePath LabelPath = "PromptLabel";

	private Label _label = null!;

	public override void _Ready()
	{
		_label = GetNode<Label>(LabelPath);
		_label.Visible = false;

		var player = GetNode<PlayerController>(PlayerPath);
		player.InteractionTargetChanged += OnInteractionTargetChanged;
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
}
