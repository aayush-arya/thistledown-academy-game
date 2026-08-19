using Godot;
using ThistledownHollowBell.Player;
using ThistledownHollowBell.UI;

namespace ThistledownHollowBell.Managers;

// Opens/closes the Omen Glass overlay on the "omen_glass" input action.
// Same sibling-of-Player pattern as CorkboardController/DialogueController.
public partial class OmenGlassController : Node
{
	[Export] public NodePath PlayerPath = "../Player";

	private const string OmenGlassScenePath = "res://scenes/ui/omen_glass.tscn";

	private PackedScene _scene = null!;
	private PlayerController _player = null!;
	private OmenGlassUI? _activeUi;

	public override void _Ready()
	{
		_scene = GD.Load<PackedScene>(OmenGlassScenePath);
		_player = GetNode<PlayerController>(PlayerPath);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("omen_glass"))
		{
			if (_activeUi == null) Open(); else Close();
		}
	}

	private void Open()
	{
		var instance = _scene.Instantiate<OmenGlassUI>();
		AddChild(instance);
		_activeUi = instance;
		instance.CloseRequested += Close;

		_player.SetGameplayInputEnabled(false);
	}

	private void Close()
	{
		if (_activeUi != null)
		{
			_activeUi.QueueFree();
			_activeUi = null;
		}
		_player.SetGameplayInputEnabled(true);
	}
}
