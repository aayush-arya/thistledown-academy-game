using Godot;
using System;
using ThistledownHollowBell.Managers;
using ThistledownHollowBell.Omen;

namespace ThistledownHollowBell.UI;

// The dusk ritual screen: either tells the player the Glass has nothing to
// say right now, offers the current question list, or shows the cryptic
// response after they ask one. Deliberately plain/text-only for now —
// Phase 7 gives this proper parchment/ink styling.
public partial class OmenGlassUI : Control
{
	private Label _bodyLabel = null!;
	private VBoxContainer _optionsContainer = null!;
	private Button _closeButton = null!;

	public event Action? CloseRequested;

	public override void _Ready()
	{
		_bodyLabel = GetNode<Label>("Panel/Margin/VBox/BodyLabel");
		_optionsContainer = GetNode<VBoxContainer>("Panel/Margin/VBox/OptionsContainer");
		_closeButton = GetNode<Button>("Panel/Margin/VBox/CloseButton");
		_closeButton.Pressed += () => CloseRequested?.Invoke();

		Refresh();
	}

	private void Refresh()
	{
		ClearOptions();

		var mgr = OmenGlassManager.Instance;
		if (mgr == null)
		{
			_bodyLabel.Text = "The glass is silent.";
			return;
		}

		if (!mgr.CanAsk())
		{
			_bodyLabel.Text = DayNightManager.Instance?.CurrentSlot == DaySlot.Dusk
				? "The glass has already shown you what it will, today."
				: "The glass stays dark until dusk.";
			return;
		}

		var questions = mgr.GetAvailableQuestions();
		if (questions.Count == 0)
		{
			_bodyLabel.Text = "You have no questions left to ask it. Not tonight, anyway.";
			return;
		}

		_bodyLabel.Text = "You turn the cracked glass over in your hand and ask —";
		foreach (var q in questions)
		{
			var button = new Button { Text = q.PromptText };
			string id = q.Id;
			button.Pressed += () => OnQuestionChosen(id);
			_optionsContainer.AddChild(button);
		}
	}

	private void OnQuestionChosen(string questionId)
	{
		string response = OmenGlassManager.Instance?.Ask(questionId) ?? "...";
		ClearOptions();
		_bodyLabel.Text = response;
	}

	private void ClearOptions()
	{
		foreach (Node child in _optionsContainer.GetChildren())
		{
			child.QueueFree();
		}
	}
}
