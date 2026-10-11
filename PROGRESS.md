# Progress

## Done
- Phase 0 scaffold: mod project, placeholder sword, .gitignore, ASSETS.md, local git repo.
- .NET 8 SDK installed; command-line build works (see Build notes).

## Next
- Confirm Phase 0 in-game (Build + Reload, enable mod, craft sword in a Journey world).
- Then Phase 1 (Heavenly Teleporter + recipe), once Phase 0 is confirmed.

## Build notes
- Project folder MUST be named MotherOfTerraria (tModLoader uses the folder name as the mod name). ModSources/MotherOfTerraria is a symlink to it.
- If `dotnet build` hangs at restore, run with `DOTNET_SYSTEM_NET_DISABLEIPV6=1` (this network drops IPv6).

## Known issues
- Heavenly Teleporter reference art is a mockup, not a game-ready sprite (see ASSETS.md).
