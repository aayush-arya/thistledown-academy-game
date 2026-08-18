using Godot;

namespace ThistledownHollowBell.Player;

// Simple grounded third-person controller: WASD relative to camera yaw,
// mouse-look via a SpringArm3D, and a short interact raycast from the
// camera. This is not an action game, so there's deliberately no sprint
// stamina, jump puzzles, or combat — just walk, look, interact.
public partial class PlayerController : CharacterBody3D
{
	[Export] public float WalkSpeed = 3.5f;
	[Export] public float RunSpeed = 5.5f;
	[Export] public float MouseSensitivity = 0.003f;
	[Export] public float InteractRange = 2.5f;

	private const float Gravity = 9.8f;

	private SpringArm3D _springArm = null!;
	private Camera3D _camera = null!;
	private RayCast3D _interactRay = null!;

	private Node? _currentInteractable;

	[Signal]
	public delegate void InteractionTargetChangedEventHandler(string promptText);

	public override void _Ready()
	{
		_springArm = GetNode<SpringArm3D>("SpringArm3D");
		_camera = _springArm.GetNode<Camera3D>("Camera3D");
		_interactRay = _camera.GetNode<RayCast3D>("InteractRay");
		_interactRay.TargetPosition = new Vector3(0, 0, -InteractRange);

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);

			float pitch = _springArm.RotationDegrees.X - mouseMotion.Relative.Y * MouseSensitivity * 100f;
			pitch = Mathf.Clamp(pitch, -60f, 20f);
			_springArm.RotationDegrees = new Vector3(pitch, _springArm.RotationDegrees.Y, _springArm.RotationDegrees.Z);
		}

		if (@event.IsActionPressed("interact"))
		{
			TryInteract();
		}

		if (@event.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
				? Input.MouseModeEnum.Visible
				: Input.MouseModeEnum.Captured;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y -= Gravity * (float)delta;
		}

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		float speed = Input.IsActionPressed("run") ? RunSpeed : WalkSpeed;

		if (direction.LengthSquared() > 0.01f)
		{
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, speed);
		}

		Velocity = velocity;
		MoveAndSlide();

		UpdateInteractionTarget();
	}

	private void UpdateInteractionTarget()
	{
		Node? target = null;
		if (_interactRay.IsColliding())
		{
			var collider = _interactRay.GetCollider();
			if (collider is Node node && node is IInteractable)
			{
				target = node;
			}
		}

		if (target != _currentInteractable)
		{
			_currentInteractable = target;
			string prompt = target is IInteractable interactable ? interactable.GetInteractionPrompt() : "";
			EmitSignal(SignalName.InteractionTargetChanged, prompt);
		}
	}

	private void TryInteract()
	{
		if (_currentInteractable is IInteractable interactable)
		{
			interactable.Interact(this);
		}
	}
}
