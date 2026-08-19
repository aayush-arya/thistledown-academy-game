using Godot;
using System.Collections.Generic;
using ThistledownHollowBell.Managers;

namespace ThistledownHollowBell.Player;

// Simple, low-complexity night stealth (see brief §8): walks a loop of
// waypoints, only "active" during the Night slot, and builds up a
// detection meter while the player stays within range. Getting caught
// costs a day via DayNightManager.ForceSendToDorm — never a hard fail.
public partial class PatrolAgent : Node3D
{
	[Export] public NodePath WaypointsPath = "Waypoints";
	[Export] public NodePath PlayerPath = "../Player";
	[Export] public float MoveSpeed = 1.2f;
	[Export] public float DetectionRadius = 3.5f;
	[Export] public float TimeToDetect = 1.2f;
	[Export] public string ReturnScenePath = "res://scenes/hub/greenhouse.tscn";
	[Export] public string CaughtMessage = "Someone's out there. You bolt before they get a good look — but the night's gone.";

	private readonly List<Node3D> _waypoints = new();
	private int _targetIndex;
	private Node3D _player = null!;
	private float _detectionMeter;
	private bool _triggered;

	public override void _Ready()
	{
		var container = GetNodeOrNull(WaypointsPath);
		if (container != null)
		{
			foreach (Node child in container.GetChildren())
			{
				if (child is Node3D marker) _waypoints.Add(marker);
			}
		}

		_player = GetNode<Node3D>(PlayerPath);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_triggered) return;

		if (DayNightManager.Instance?.CurrentSlot != DaySlot.Night)
		{
			_detectionMeter = Mathf.Max(0f, _detectionMeter - (float)delta);
			return;
		}

		Patrol((float)delta);
		UpdateDetection((float)delta);
	}

	private void Patrol(float delta)
	{
		if (_waypoints.Count == 0) return;

		Vector3 target = _waypoints[_targetIndex].GlobalPosition;
		Vector3 toTarget = target - GlobalPosition;
		toTarget.Y = 0;

		if (toTarget.Length() < 0.2f)
		{
			_targetIndex = (_targetIndex + 1) % _waypoints.Count;
			return;
		}

		Vector3 dir = toTarget.Normalized();
		GlobalPosition += dir * MoveSpeed * delta;
		LookAt(new Vector3(target.X, GlobalPosition.Y, target.Z), Vector3.Up);
	}

	private void UpdateDetection(float delta)
	{
		float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (dist <= DetectionRadius)
		{
			_detectionMeter += delta;
			if (_detectionMeter >= TimeToDetect)
			{
				TriggerCaught();
			}
		}
		else
		{
			_detectionMeter = Mathf.Max(0f, _detectionMeter - delta * 0.5f);
		}
	}

	private void TriggerCaught()
	{
		_triggered = true;

		var hud = GetTree().GetFirstNodeInGroup("hud") as InteractPromptHud;
		hud?.ShowToast(CaughtMessage);

		DayNightManager.Instance?.ForceSendToDorm();

		GetTree().CreateTimer(1.2).Timeout += () => GetTree().ChangeSceneToFile(ReturnScenePath);
	}
}
