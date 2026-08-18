using Godot;
using ThistledownHollowBell.Player;
using ThistledownHollowBell.UI;

namespace ThistledownHollowBell.Managers;

// Opens/closes the corkboard overlay on the "journal" input action. Every
// hub/location scene that wants the corkboard available adds one of these
// as a sibling of its Player node.
public partial class CorkboardController : Node
{
	[Export] public NodePath PlayerPath = "../Player";

	private const string CorkboardScenePath = "res://scenes/ui/corkboard.tscn";

	private PackedScene _corkboardScene = null!;
	private PlayerController _player = null!;
	private CorkboardUI? _activeCorkboard;

	public override void _Ready()
	{
		_corkboardScene = GD.Load<PackedScene>(CorkboardScenePath);
		_player = GetNode<PlayerController>(PlayerPath);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("journal"))
		{
			if (_activeCorkboard == null) Open(); else Close();
		}
	}

	private void Open()
	{
		var instance = _corkboardScene.Instantiate<CorkboardUI>();
		AddChild(instance);
		_activeCorkboard = instance;
		instance.Closed += Close;

		_player.SetGameplayInputEnabled(false);
	}

	private void Close()
	{
		if (_activeCorkboard != null)
		{
			_activeCorkboard.QueueFree();
			_activeCorkboard = null;
		}
		_player.SetGameplayInputEnabled(true);
	}
}
