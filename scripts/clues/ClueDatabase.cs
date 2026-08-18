using Godot;
using System.Collections.Generic;
using System.Text.Json;

namespace ThistledownHollowBell.Clues;

// Loads every data/clues/*.json file at startup and tracks, at runtime,
// which clues the player has discovered and which corkboard connections
// they've drawn. This is the backing store for the corkboard UI (Phase 2) —
// the UI itself just visualizes what's in here.
public partial class ClueDatabase : Node
{
	public static ClueDatabase Instance { get; private set; } = null!;

	[Signal]
	public delegate void ClueDiscoveredEventHandler(string clueId);

	[Signal]
	public delegate void ConnectionMadeEventHandler(string clueIdA, string clueIdB, bool wasCorrect);

	private const string ClueDataDir = "res://data/clues";

	private readonly Dictionary<string, Clue> _allClues = new();
	private readonly HashSet<string> _discovered = new();

	// Connections are stored as normalized "a|b" keys (alphabetical) so
	// order of connection doesn't matter.
	private readonly HashSet<string> _connections = new();

	// Where the player last dragged each pin on the corkboard. Absent
	// entries get a deterministic scatter position from the UI instead.
	private readonly Dictionary<string, Vector2> _pinPositions = new();

	public override void _Ready()
	{
		Instance = this;
		LoadAllClueFiles();
	}

	private void LoadAllClueFiles()
	{
		using var dir = DirAccess.Open(ClueDataDir);
		if (dir == null)
		{
			GD.PushWarning($"ClueDatabase: could not open {ClueDataDir}");
			return;
		}

		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (fileName != "")
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
			{
				LoadClueFile($"{ClueDataDir}/{fileName}");
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
	}

	private void LoadClueFile(string path)
	{
		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PushWarning($"ClueDatabase: failed to open {path}");
			return;
		}

		string json = file.GetAsText();
		ClueFile? parsed;
		try
		{
			parsed = JsonSerializer.Deserialize<ClueFile>(json);
		}
		catch (JsonException e)
		{
			GD.PushError($"ClueDatabase: malformed JSON in {path}: {e.Message}");
			return;
		}

		if (parsed == null) return;

		foreach (var clue in parsed.Clues)
		{
			if (_allClues.ContainsKey(clue.Id))
			{
				GD.PushWarning($"ClueDatabase: duplicate clue id '{clue.Id}' in {path}");
				continue;
			}
			_allClues[clue.Id] = clue;
		}
	}

	public Clue? GetClue(string id) => _allClues.TryGetValue(id, out var c) ? c : null;

	public bool IsDiscovered(string id) => _discovered.Contains(id);

	public IEnumerable<Clue> GetDiscoveredClues()
	{
		foreach (var id in _discovered)
		{
			if (_allClues.TryGetValue(id, out var c)) yield return c;
		}
	}

	public void DiscoverClue(string id)
	{
		if (!_allClues.ContainsKey(id))
		{
			GD.PushWarning($"ClueDatabase: tried to discover unknown clue '{id}'");
			return;
		}
		if (_discovered.Add(id))
		{
			EmitSignal(SignalName.ClueDiscovered, id);
		}
	}

	// Called by the corkboard UI when the player draws a string between two
	// pinned clues. Returns true if the connection is one the clue data
	// declares as intentional; false means "soft nudge, not a hard fail".
	public bool TryConnect(string clueIdA, string clueIdB)
	{
		bool correct = ClueDeclaresConnection(clueIdA, clueIdB) || ClueDeclaresConnection(clueIdB, clueIdA);

		if (correct)
		{
			_connections.Add(NormalizeKey(clueIdA, clueIdB));

			if (_allClues.TryGetValue(clueIdA, out var a) && a.UnlockFlag != null)
				Managers.GameManager.Instance?.SetFlag(a.UnlockFlag);
			if (_allClues.TryGetValue(clueIdB, out var b) && b.UnlockFlag != null)
				Managers.GameManager.Instance?.SetFlag(b.UnlockFlag);
		}

		EmitSignal(SignalName.ConnectionMade, clueIdA, clueIdB, correct);
		return correct;
	}

	private bool ClueDeclaresConnection(string fromId, string toId)
	{
		if (!_allClues.TryGetValue(fromId, out var clue)) return false;
		foreach (var c in clue.Connections)
		{
			if (c == toId) return true;
		}
		return false;
	}

	public bool IsConnected(string clueIdA, string clueIdB) => _connections.Contains(NormalizeKey(clueIdA, clueIdB));

	// All confirmed connections, as (idA, idB) pairs, for the corkboard to draw.
	public IEnumerable<(string A, string B)> GetAllConnections()
	{
		foreach (var key in _connections)
		{
			var parts = key.Split('|');
			if (parts.Length == 2) yield return (parts[0], parts[1]);
		}
	}

	private static string NormalizeKey(string a, string b) =>
		string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";

	public Vector2? GetPinPosition(string clueId) =>
		_pinPositions.TryGetValue(clueId, out var pos) ? pos : null;

	public void SetPinPosition(string clueId, Vector2 position) => _pinPositions[clueId] = position;

	// -- persistence helpers, called by SaveManager --

	public IEnumerable<string> GetDiscoveredForSave() => _discovered;

	public IEnumerable<string> GetConnectionsForSave() => _connections;

	public IReadOnlyDictionary<string, Vector2> GetPinPositionsForSave() => _pinPositions;

	public void LoadFromSave(IEnumerable<string> discovered, IEnumerable<string> connections)
	{
		_discovered.Clear();
		foreach (var id in discovered) _discovered.Add(id);
		_connections.Clear();
		foreach (var conn in connections) _connections.Add(conn);
	}

	public void LoadPinPositionsFromSave(Dictionary<string, Vector2> positions)
	{
		_pinPositions.Clear();
		foreach (var kvp in positions) _pinPositions[kvp.Key] = kvp.Value;
	}
}
