# Progress

## Done
- Phase 0 COMPLETE (confirmed in-game 2026-10-11): mod builds, loads in tModLoader 1.4.4.9, Placeholder Sword found and working in a Journey world.
- .NET 8 SDK installed; command-line build works (see Build notes).

- Phase 1 COMPLETE (confirmed in-game 2026-10-11): Heavenly Teleporter item + recipe (Celestial Sigil + Copper Shortsword, craftable only at an Ancient Manipulator), Debug Starter Kit (craft from 1 Dirt Block), localization file, sprite cut from the owner's design.

- Phase 2.1 COMPLETE (confirmed in-game 2026-10-12): SubworldLibrary 2.3.0.1 dependency, empty Heavens subworld (800x400, flat cloud floor, regenerated every visit). Temporary /heavens debug command used for the test, removed in 2.2.
- Phase 2.3 COMPLETE (confirmed in-game 2026-10-12): escape items blocked inside the Heavens; Heavens terrain (HeavensSubworld.ProtectedTiles) can't be broken, blown up or replaced. Add each new terrain block type to that set.
- Phase 2.2 COMPLETE (confirmed in-game 2026-10-12): the Heavenly Teleporter enters the Heavens; using it inside returns to the main world (no separate return item). Inventory and character intact across trips.

## In progress
- Phase 2.4 (owner reviewed 2026-10-12, looks good apart from the hallowed look): Heavens terrain is now 4 long islands (Arrival, High, Low, flat Arena) + 4 small floaters joined by cloud bridges, hallowed grass tops, trees, 6 golden statues (placeholder art). Fixed layout and seed. Protected block types: Cloud, HallowedGrass, Trees, Saplings, GoldenStatue.
- Phase 2.5 (awaiting owner test): Heavens sky (azure-to-gold gradient, warm tint, fewer clouds), placeholder music (vanilla Space), permanent midday, only this mod's NPCs may spawn there (none exist yet, so no spawns). Note for 2C: SubworldLibrary's NormalUpdates is false by default; check that custom enemies spawn, and set NormalUpdates => true if they don't.

## Next
- (2.3 decisions) owner decisions 2026-10-12: flight, mounts, hooks, jumps and Rod of Discord/Harmony all ALLOWED. Placing blocks allowed; breaking the Heavens' own terrain blocked. Escape items (Magic Mirror, Recall, Cell Phone, Shellphone, Potion of Return, conches) blocked; the Teleporter is the only exit.
- 2.4 REVISED (owner, 2026-10-12): long, wide, majestic floating islands with trees and golden statues. NEW Phase 2C (Heavens details and polish) added after Phase 2; Phases 3-8 keep their numbers. Scope of 2C is a draft to refine with the owner. 2C now includes custom hostile Heavens enemies (owner supplies designs; heavenly-spear attacks, very high damage). Boss summon design is deferred, to be discussed later.
- Phase 2 is a real subworld (SubworldLibrary), not a sky structure. Steps 2.1-2.6 are in ROADMAP.md (local only).
- Open decision: scrub ROADMAP.md from GitHub history, or make the repo private (repo is public).

- Pushed to GitHub (2026-10-11): https://github.com/Unix-H/mother-of-terraria-mod (main + tags phase-0, phase-1). Remote uses SSH (key ~/.ssh/id_ed25519_github).

- Owner feedback on 2.4 (2026-10-12): the Hallowed Grass look is not good. Logged in Phase 2C to be replaced with a custom surface. Also: replace cloud bridges with golden heavenly bridges and move the islands closer together (Phase 2C). See ASSETS.md for the full list of placeholders and stand-ins.

## Build notes
- Project folder MUST be named MotherOfTerraria (tModLoader uses the folder name as the mod name). ModSources/MotherOfTerraria is a symlink to it.
- SubworldLibrary is a required mod (build.txt modReferences). For compiling, its DLL is extracted from the Workshop .tmod into lib/SubworldLibrary.dll (git-ignored, not shipped). Re-extract if SubworldLibrary updates. Workshop ID 2785100219.
- SubworldLibrary Enter/Exit must be called on the player's own client; in multiplayer they send a request to the server. They do nothing on the server (use MovePlayerToSubworld there).
- If `dotnet build` hangs at restore, run with `DOTNET_SYSTEM_NET_DISABLEIPV6=1` (this network drops IPv6).

## Known issues
- Celestial Sigil is crafted from 12 of each Lunar Fragment. Unverified whether fragments are still obtainable after Moon Lord is dead (check in a real playthrough).
- Owner reported the recipe working; the prefixed-sword and missing-ingredient cases were not individually confirmed.
- Close tModLoader before building from the command line (it locks the .tmod); in-game Build + Reload is the normal path.
- Heavenly Teleporter reference art is a mockup, not a game-ready sprite (see ASSETS.md).
