using Godot;
using ThistledownHollowBell.Save;

namespace ThistledownHollowBell.Managers;

public partial class MainMenu : Control
{
	[Export] public NodePath StartButtonPath = "%StartButton";
	[Export] public NodePath ContinueButtonPath = "%ContinueButton";
	[Export] public NodePath QuitButtonPath = "%QuitButton";
	[Export] public string FirstScenePath = "res://scenes/hub/greenhouse.tscn";

	public override void _Ready()
	{
		var startButton = GetNode<Button>(StartButtonPath);
		var continueButton = GetNode<Button>(ContinueButtonPath);
		var quitButton = GetNode<Button>(QuitButtonPath);

		startButton.Pressed += OnStartPressed;
		continueButton.Pressed += OnContinuePressed;
		quitButton.Pressed += OnQuitPressed;

		continueButton.Disabled = !SaveManager.Instance.SlotExists(0);
	}

	private void OnStartPressed()
	{
		GetTree().ChangeSceneToFile(FirstScenePath);
	}

	private void OnContinuePressed()
	{
		if (SaveManager.Instance.Load(0))
		{
			GetTree().ChangeSceneToFile(FirstScenePath);
		}
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
