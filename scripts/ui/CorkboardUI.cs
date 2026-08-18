using Godot;
using System.Collections.Generic;
using ThistledownHollowBell.Clues;

namespace ThistledownHollowBell.UI;

// The corkboard screen: pins one card per discovered clue, lets the player
// drag cards around (purely cosmetic, persisted via ClueDatabase pin
// positions) and drag a string from a card's knob to another card to
// attempt a connection. Validation lives in ClueDatabase.TryConnect — this
// class only visualizes the result and gives the soft-fail nudge text.
public partial class CorkboardUI : Control
{
	[Signal]
	public delegate void ClosedEventHandler();

	private const string PinScenePath = "res://scenes/ui/corkboard_pin.tscn";

	private PackedScene _pinScene = null!;
	private Control _pinLayer = null!;
	private Label _statusLabel = null!;
	private Button _closeButton = null!;

	private readonly Dictionary<string, CorkboardPin> _pins = new();

	private string? _draggingBodyId;
	private string? _connectingFromId;
	private Vector2 _dragCurrentPos;

	private static readonly Color BackgroundColor = new(0.30f, 0.24f, 0.17f);
	private static readonly Color StringColor = new(0.72f, 0.14f, 0.11f);
	private static readonly Color TempStringColor = new(0.72f, 0.14f, 0.11f, 0.5f);

	private const string DefaultStatus = "Drag a string from a card's tack to another card to connect them.";

	public override void _Ready()
	{
		_pinScene = GD.Load<PackedScene>(PinScenePath);
		_pinLayer = GetNode<Control>("PinLayer");
		_statusLabel = GetNode<Label>("StatusLabel");
		_closeButton = GetNode<Button>("CloseButton");

		_closeButton.Pressed += () => EmitSignal(SignalName.Closed);

		SpawnPins();

		if (ClueDatabase.Instance != null)
		{
			ClueDatabase.Instance.ClueDiscovered += _ => SpawnPins();
		}

		SetStatus(DefaultStatus);
	}

	private void SpawnPins()
	{
		if (ClueDatabase.Instance == null) return;

		int index = 0;
		foreach (var clue in ClueDatabase.Instance.GetDiscoveredClues())
		{
			if (!_pins.ContainsKey(clue.Id))
			{
				var pin = _pinScene.Instantiate<CorkboardPin>();
				pin.ClueId = clue.Id;
				_pinLayer.AddChild(pin);

				var saved = ClueDatabase.Instance.GetPinPosition(clue.Id);
				pin.Position = saved ?? DefaultScatterPosition(index);

				pin.BodyDragStarted += OnPinBodyDragStarted;
				pin.ConnectionDragStarted += OnPinConnectionDragStarted;

				_pins[clue.Id] = pin;
			}
			index++;
		}
		QueueRedraw();
	}

	// Deterministic grid-with-jitter so a first-time board doesn't stack
	// every new pin at the same spot.
	private static Vector2 DefaultScatterPosition(int index)
	{
		int col = index % 4;
		int row = index / 4;

		var rng = new RandomNumberGenerator();
		rng.Seed = (ulong)(index * 7919 + 13);
		float jitterX = rng.RandfRange(-20f, 20f);
		float jitterY = rng.RandfRange(-20f, 20f);

		return new Vector2(140 + col * 220 + jitterX, 140 + row * 160 + jitterY);
	}

	private void OnPinBodyDragStarted(string clueId)
	{
		if (_connectingFromId != null) return;

		_draggingBodyId = clueId;
		if (_pins.TryGetValue(clueId, out var pin))
		{
			_pinLayer.MoveChild(pin, _pinLayer.GetChildCount() - 1);
		}
	}

	private void OnPinConnectionDragStarted(string clueId)
	{
		_draggingBodyId = null;
		_connectingFromId = clueId;
		_dragCurrentPos = GetLocalMousePosition();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			if (_draggingBodyId != null && _pins.TryGetValue(_draggingBodyId, out var draggedPin))
			{
				draggedPin.Position += motion.Relative;
				QueueRedraw();
			}
			else if (_connectingFromId != null)
			{
				_dragCurrentPos = motion.Position;
				QueueRedraw();
			}
		}
		else if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } mouseButton)
		{
			if (_draggingBodyId != null)
			{
				FinishBodyDrag(_draggingBodyId);
				_draggingBodyId = null;
				QueueRedraw();
			}
			else if (_connectingFromId != null)
			{
				FinishConnectionDrag(_connectingFromId, mouseButton.Position);
				_connectingFromId = null;
				QueueRedraw();
			}
		}
	}

	private void FinishBodyDrag(string clueId)
	{
		if (_pins.TryGetValue(clueId, out var pin))
		{
			ClueDatabase.Instance?.SetPinPosition(clueId, pin.Position);
		}
	}

	private void FinishConnectionDrag(string fromId, Vector2 releasePos)
	{
		string? targetId = null;
		foreach (var kvp in _pins)
		{
			if (kvp.Key == fromId) continue;
			if (kvp.Value.GetRect().HasPoint(releasePos))
			{
				targetId = kvp.Key;
				break;
			}
		}

		if (targetId == null)
		{
			SetStatus(DefaultStatus);
			return;
		}

		if (ClueDatabase.Instance == null) return;

		if (ClueDatabase.Instance.IsConnected(fromId, targetId))
		{
			SetStatus("Already connected.");
			return;
		}

		bool correct = ClueDatabase.Instance.TryConnect(fromId, targetId);
		SetStatus(correct ? "Connected." : "That doesn't feel right yet.");
	}

	private void SetStatus(string text) => _statusLabel.Text = text;

	public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, Size), BackgroundColor);

		if (ClueDatabase.Instance != null)
		{
			foreach (var (a, b) in ClueDatabase.Instance.GetAllConnections())
			{
				if (_pins.TryGetValue(a, out var pinA) && _pins.TryGetValue(b, out var pinB))
				{
					DrawLine(pinA.GetConnectorPosition(), pinB.GetConnectorPosition(), StringColor, 3f, true);
				}
			}
		}

		if (_connectingFromId != null && _pins.TryGetValue(_connectingFromId, out var sourcePin))
		{
			DrawLine(sourcePin.GetConnectorPosition(), _dragCurrentPos, TempStringColor, 3f, true);
		}
	}
}
