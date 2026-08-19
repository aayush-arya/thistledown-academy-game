using Godot;
using System.Collections.Generic;
using System.Text.Json;
using ThistledownHollowBell.Managers;

namespace ThistledownHollowBell.Omen;

// The dusk ritual: once per in-game day, at dusk, the player can ask the
// Omen Glass one question from a short list gated by story flags. This is
// the game's built-in hint system, so answers stay cryptic — see
// docs/lore_bible.md §3.
public partial class OmenGlassManager : Node
{
	public static OmenGlassManager Instance { get; private set; } = null!;

	private const string DataPath = "res://data/dialogue/omen_glass.json";

	private readonly List<OmenGlassQuestion> _all = new();
	private readonly HashSet<string> _answered = new();
	private int _lastAskedDay = -1;

	public override void _Ready()
	{
		Instance = this;
		LoadData();
	}

	private void LoadData()
	{
		using var file = FileAccess.Open(DataPath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PushWarning($"OmenGlassManager: could not open {DataPath}");
			return;
		}

		string json = file.GetAsText();
		OmenGlassFile? parsed;
		try
		{
			parsed = JsonSerializer.Deserialize<OmenGlassFile>(json);
		}
		catch (JsonException e)
		{
			GD.PushError($"OmenGlassManager: malformed JSON in {DataPath}: {e.Message}");
			return;
		}

		if (parsed == null) return;
		_all.AddRange(parsed.Questions);
	}

	// The Glass only speaks at dusk, and only once per day.
	public bool CanAsk() =>
		DayNightManager.Instance != null &&
		DayNightManager.Instance.CurrentSlot == DaySlot.Dusk &&
		DayNightManager.Instance.CurrentDay != _lastAskedDay;

	public List<OmenGlassQuestion> GetAvailableQuestions()
	{
		var list = new List<OmenGlassQuestion>();
		foreach (var q in _all)
		{
			if (_answered.Contains(q.Id)) continue;
			if (q.RequiresFlag != null && GameManager.Instance?.HasFlag(q.RequiresFlag) != true) continue;
			list.Add(q);
		}
		return list;
	}

	// Returns the cryptic response text and marks today as used.
	public string Ask(string questionId)
	{
		var question = _all.Find(q => q.Id == questionId);
		if (question == null)
		{
			GD.PushWarning($"OmenGlassManager: unknown question '{questionId}'");
			return "...";
		}

		_answered.Add(questionId);
		if (DayNightManager.Instance != null)
		{
			_lastAskedDay = DayNightManager.Instance.CurrentDay;
		}

		return question.ResponseText;
	}

	// -- persistence helpers, called by SaveManager --

	public IEnumerable<string> GetAnsweredForSave() => _answered;

	public int GetLastAskedDayForSave() => _lastAskedDay;

	public void LoadFromSave(IEnumerable<string> answered, int lastAskedDay)
	{
		_answered.Clear();
		foreach (var id in answered) _answered.Add(id);
		_lastAskedDay = lastAskedDay;
	}
}
