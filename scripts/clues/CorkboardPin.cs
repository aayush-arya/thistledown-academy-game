using Godot;

namespace ThistledownHollowBell.Clues;

// One card on the corkboard. Doesn't do its own dragging — it only reports
// "someone pressed my body" or "someone pressed my knob" via signals, and
// CorkboardUI (which can see every pin and covers the whole screen) owns
// the actual drag-tracking, since a drag routinely leaves this control's
// own bounds.
public partial class CorkboardPin : Panel
{
	[Export] public string ClueId = "";

	private Label _titleLabel = null!;
	private Control _knob = null!;

	[Signal]
	public delegate void BodyDragStartedEventHandler(string clueId);

	[Signal]
	public delegate void ConnectionDragStartedEventHandler(string clueId);

	public override void _Ready()
	{
		_titleLabel = GetNode<Label>("TitleLabel");
		_knob = GetNode<Control>("Knob");

		GuiInput += OnBodyGuiInput;
		_knob.GuiInput += OnKnobGuiInput;

		var clue = ClueDatabase.Instance?.GetClue(ClueId);
		_titleLabel.Text = clue != null ? clue.Title : ClueId;
	}

	private void OnBodyGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
		{
			EmitSignal(SignalName.BodyDragStarted, ClueId);
		}
	}

	private void OnKnobGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
		{
			EmitSignal(SignalName.ConnectionDragStarted, ClueId);
		}
	}

	// In the same local coordinate space as CorkboardUI's own _Draw (the
	// pin's Position is already parent-relative, and the pin's parent sits
	// at the CorkboardUI's origin with no offset/scale).
	public Vector2 GetConnectorPosition() => Position + _knob.Position + _knob.Size / 2f;
}
