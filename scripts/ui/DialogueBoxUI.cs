using Godot;
using System;
using System.Collections.Generic;
using ThistledownHollowBell.Dialogue;

namespace ThistledownHollowBell.UI;

// Bottom-anchored VN-style dialogue box: speaker name, line text, and a
// stack of option buttons rebuilt each time the conversation advances.
// Owns no dialogue logic itself — DialogueController tells it what to show
// and listens for which option got picked.
public partial class DialogueBoxUI : Control
{
	private Label _speakerLabel = null!;
	private Label _textLabel = null!;
	private VBoxContainer _optionsContainer = null!;

	public event Action<DialogueOption>? OptionChosen;
	public event Action? CloseRequested;

	public override void _Ready()
	{
		_speakerLabel = GetNode<Label>("Panel/Margin/VBox/SpeakerLabel");
		_textLabel = GetNode<Label>("Panel/Margin/VBox/TextLabel");
		_optionsContainer = GetNode<VBoxContainer>("Panel/Margin/VBox/OptionsContainer");
	}

	public void ShowNode(DialogueNode node, List<DialogueOption> visibleOptions)
	{
		_speakerLabel.Text = node.Speaker;
		_textLabel.Text = node.Text;

		foreach (Node child in _optionsContainer.GetChildren())
		{
			child.QueueFree();
		}

		if (visibleOptions.Count == 0)
		{
			var endButton = new Button { Text = "[End conversation]" };
			endButton.Pressed += () => CloseRequested?.Invoke();
			_optionsContainer.AddChild(endButton);
			return;
		}

		foreach (var option in visibleOptions)
		{
			var button = new Button { Text = option.Text };
			button.Pressed += () => OptionChosen?.Invoke(option);
			_optionsContainer.AddChild(button);
		}
	}
}
