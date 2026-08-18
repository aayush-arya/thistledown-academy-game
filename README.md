# Thistledown Academy: The Hollow Bell

A gothic-mystery boarding-school game. Students and townsfolk have started vanishing near the
old lake, always preceded by a sound the locals call the Hollow Bell. The administration
insists it's nothing. You, an outcast with a cracked heirloom mirror and no particular
enthusiasm for any of this, are going to find out anyway.

See [`docs/lore_bible.md`](docs/lore_bible.md) for setting/character reference and
[`docs/mystery_flowchart.md`](docs/mystery_flowchart.md) for the case structure.

## Status

This is an in-progress solo/AI-assisted build. Current state:

**Built:**
- Project scaffold (Godot 4.3, C#/.NET 8) with autoload wiring
- Day/night cycle manager (morning/afternoon/dusk/night slots, day counter)
- Story-flag + location-unlock system (`GameManager`)
- JSON-driven clue database with corkboard-connection validation (`ClueDatabase`)
- JSON-driven branching dialogue data model + gating logic (`DialogueManager`)
- Relationship meters for Junie/Priya (`RelationshipManager`)
- JSON save/load covering day, flags, unlocked locations, discovered clues, corkboard
  connections, and relationship values (`SaveManager`)
- Third-person walk/look/interact player controller
- A minimal playable test scene (greenhouse) with two pickable clues, and a main menu
- Seed content for Act 1 (greenhouse clues + Junie's opening dialogue tree)

**Not yet built** (see `docs/mystery_flowchart.md` for the design):
- Corkboard UI (drag-and-connect string interface) — currently the connection logic exists
  in `ClueDatabase.TryConnect`, but there's no visual board to drive it
- Dialogue box UI with portraits — `DialogueManager` has no on-screen presentation yet
- Interrogation/present-evidence UI for suspects
- Remaining locations (Lake Shore, Archive, Bell Tower) — only the Greenhouse has a built scene
- Stealth/patrol encounters
- Omen Glass ritual UI and its question-pool data file
- Full art pass (currently placeholder boxes/capsules, no CC0 asset packs imported yet)
- Audio
- Windows export configuration

## Requirements to open/run this project

- **Godot 4.3+ (.NET/C# version)** — the standard (non-.NET) Godot build cannot open this
  project, since it relies on C# scripts.
- **.NET 8 SDK** — needed to build the C# scripts. Neither Godot nor the .NET SDK is installed
  in the environment this project was scaffolded in, so none of this has been opened in the
  editor or run yet. Please install both, open `project.godot`, let it build, and press Play
  on `scenes/hub/greenhouse.tscn` (or the main scene) to sanity-check the current state —
  then report back anything that doesn't work so it can get fixed.

## Controls (current build)

| Action | Key |
|---|---|
| Move | WASD |
| Look | Mouse |
| Run | Shift |
| Interact | E |
| Toggle mouse capture | Esc |

## Project structure

```
scenes/       hub, locations, ui, player scenes
scripts/      player, dialogue, clues, relationships, save, managers
data/         dialogue/*.json, clues/*.json — content lives here, not in code
docs/         lore_bible.md, mystery_flowchart.md
```
