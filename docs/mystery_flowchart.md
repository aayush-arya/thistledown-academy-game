# Mystery Flowchart — The Hollow Bell

High-level structure of the main case. This is the outline `data/clues/*.json` and
`data/dialogue/*.json` should implement against. Keep it updated as content is written —
this is the map; the JSON is the territory.

## Act structure

**Act 1 — Something's Off (Days 1–3)**
- Teach systems: movement, interact, journal, corkboard, day/night loop.
- Location unlocked: Greenhouse (Junie's space, low-stakes).
- Clues: rumor-tier — student gossip about a disappearance, a missing-poster, Junie's
  half-joking theory. Corkboard connection here is intentionally easy/obvious to teach the
  mechanic.
- Beat: first dusk Omen Glass use, framed as the player's habitual ritual, not a big reveal.
- Ends when: player connects "disappearances cluster near the lake" on the corkboard →
  unlocks Lake Shore.

**Act 2 — The Pattern (Days 3–8)**
- Locations unlocked: Lake Shore, then the Archive.
- Relationship gates: Junie's trust unlocks her photography contact sheets (physical evidence
  of who was near the lake and when). Priya's stake surfaces around the midpoint once the
  player has 2+ lake-shore clues — she approaches the player, not the other way around.
- Clues: physical (footprints, a torn coat, a school ring), documentary (Archive newspaper
  clippings on a near-identical disappearance pattern from decades back, yearbook photos
  showing the bell tower before it was "condemned").
- Suspect introduced properly: Crane, framed at first as just background color, not a suspect
  — the player has to notice him.
- Ms. Halloway ambiguity established: she deflects one direct question convincingly, but a
  later clue (player's choice which) makes that deflection look suspicious in hindsight.
- Ends when: player connects "the pattern repeats on a cycle" + "the bell tower was sealed
  around the last cycle" → unlocks restricted access path to the Bell Tower grounds (still
  not inside).

**Act 3 — Who Rings It (Days 8–12)**
- Stealth/tension introduced: night investigation near the tower has a patrol risk (Crane, or
  an ambient threat) — getting caught costs a day, doesn't hard-fail.
- Interrogation-style scenes unlock for Crane and Halloway once enough evidence is pinned:
  present-evidence mechanic, wrong evidence gets a soft deflection, right evidence advances.
- Player must decide, based on gathered clues, whether Halloway is confronted as an ally or
  a suspect — this should be steerable by which clues got pinned/connected, not a hard
  dialogue-choice flag.
- Ends when: player has enough connected evidence to identify Crane's role specifically (not
  just "something's in the tower" but "Crane is maintaining it on purpose") → unlocks the Bell
  Tower interior.

**Act 4 — The Bell Tower (Days 12–14, climax)**
- High-tension, restricted, climax location. No combat — tension from avoidance/pacing and
  from the confrontation scene itself.
- Confrontation with Crane: player's accumulated understanding (which clues were connected,
  whether Halloway was treated as ally or suspect, whether Junie/Priya trust was built) should
  flavor his dialogue and the options available, not just gate a single ending checkpoint.
- **Endings (branch on: fate of the binding × fate of Crane):**
  1. **Sustain, quietly** — player helps Crane maintain the binding, tells no one. Status quo
     preserved, cost understood but hidden. Bleakest/most "understated gothic" ending.
  2. **Break it** — player forces the binding open. Removes the recurring cost but releases
     consequences the epilogue should gesture at, not fully resolve (keeps sequel-hook /
     New Game+ door open per stretch goals).
  3. **Replace the keeper** — player takes on/redistributes the binding differently (e.g.
     institutional exposure — tell the school board, force a real reckoning — changes who
     bears the cost going forward). Most "hopeful" ending, still bittersweet.
  4. **Walk away** — player has enough evidence to stop but chooses not to act on the tower
     itself; epilogue reflects consequences of inaction. Available only if player deliberately
     avoids the climax trigger rather than being locked out — don't make this feel like a
     failure state, make it a choice.

## Corkboard gating (design rule)

Every location unlock and major scene unlock should be gated by a **corkboard connection**,
not just clue count. This keeps the corkboard the spine of progression (see brief §5) rather
than a nice-to-have UI. Wrong/premature connections should always return a soft "that doesn't
feel right yet" nudge — never a hard fail state.

## Omen Glass question pool (ties to progress flags)

The dusk question list should be filtered by current act/flags so it never spoils content the
player hasn't reached and never goes stale once they have. Maintain the mapping in
`data/dialogue/omen_glass.json` (question id → required flag → cryptic response → clue/location
nudged).
