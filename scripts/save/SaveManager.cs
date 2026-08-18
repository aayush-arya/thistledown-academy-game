using Godot;
using System.Collections.Generic;
using System.Text.Json;
using ThistledownHollowBell.Clues;
using ThistledownHollowBell.Managers;
using ThistledownHollowBell.Relationships;

namespace ThistledownHollowBell.Save;

// Reads/writes user://saves/slot_N.json. Gathers state from the other
// autoloads on save and pushes it back into them on load, so this is the
// only class that needs to know the on-disk format.
public partial class SaveManager : Node
{
	public static SaveManager Instance { get; private set; } = null!;

	private const string SaveDir = "user://saves";

	public override void _Ready()
	{
		Instance = this;
		if (!DirAccess.DirExistsAbsolute(SaveDir))
		{
			DirAccess.MakeDirRecursiveAbsolute(SaveDir);
		}
	}

	private static string SlotPath(int slot) => $"{SaveDir}/slot_{slot}.json";

	public bool SlotExists(int slot) => FileAccess.FileExists(SlotPath(slot));

	public void Save(int slot)
	{
		var data = new SaveData
		{
			Day = DayNightManager.Instance.CurrentDay,
			Slot = DayNightManager.Instance.CurrentSlot.ToString(),
			Act = GameManager.Instance.CurrentAct,
			Flags = new List<string>(GameManager.Instance.GetFlagsForSave()),
			UnlockedLocations = new List<string>(GameManager.Instance.GetUnlockedLocationsForSave()),
			DiscoveredClues = new List<string>(ClueDatabase.Instance.GetDiscoveredForSave()),
			CorkboardConnections = new List<string>(ClueDatabase.Instance.GetConnectionsForSave()),
			Relationships = new Dictionary<string, int>(RelationshipManager.Instance.GetValuesForSave()),
		};

		string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

		using var file = FileAccess.Open(SlotPath(slot), FileAccess.ModeFlags.Write);
		if (file == null)
		{
			GD.PushError($"SaveManager: could not open {SlotPath(slot)} for writing");
			return;
		}
		file.StoreString(json);
	}

	public bool Load(int slot)
	{
		string path = SlotPath(slot);
		if (!FileAccess.FileExists(path))
		{
			GD.PushWarning($"SaveManager: no save at {path}");
			return false;
		}

		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PushError($"SaveManager: could not open {path} for reading");
			return false;
		}

		string json = file.GetAsText();
		SaveData? data;
		try
		{
			data = JsonSerializer.Deserialize<SaveData>(json);
		}
		catch (JsonException e)
		{
			GD.PushError($"SaveManager: malformed save at {path}: {e.Message}");
			return false;
		}

		if (data == null) return false;

		var slotEnum = System.Enum.TryParse<Managers.DaySlot>(data.Slot, out var parsedSlot)
			? parsedSlot
			: Managers.DaySlot.Morning;

		DayNightManager.Instance.LoadFromSave(data.Day, slotEnum);
		GameManager.Instance.LoadFromSave(data.Act, data.Flags, data.UnlockedLocations);
		ClueDatabase.Instance.LoadFromSave(data.DiscoveredClues, data.CorkboardConnections);
		RelationshipManager.Instance.LoadFromSave(data.Relationships);

		return true;
	}

	public void DeleteSlot(int slot)
	{
		if (FileAccess.FileExists(SlotPath(slot)))
		{
			DirAccess.RemoveAbsolute(SlotPath(slot));
		}
	}
}
