# Progress

## Done
- Phase 0 COMPLETE (confirmed in-game 2026-10-11): mod builds, loads in tModLoader 1.4.4.9, Placeholder Sword found and working in a Journey world.
- .NET 8 SDK installed; command-line build works (see Build notes).

## Next
- Phase 1 (Heavenly Teleporter + recipe). Waiting for owner's go-ahead.

## Build notes
- Project folder MUST be named MotherOfTerraria (tModLoader uses the folder name as the mod name). ModSources/MotherOfTerraria is a symlink to it.
- If `dotnet build` hangs at restore, run with `DOTNET_SYSTEM_NET_DISABLEIPV6=1` (this network drops IPv6).

## Known issues
- Heavenly Teleporter reference art is a mockup, not a game-ready sprite (see ASSETS.md).
