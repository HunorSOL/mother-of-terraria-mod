# Progress

## Done
- Phase 0 COMPLETE (confirmed in-game 2026-10-11): mod builds, loads in tModLoader 1.4.4.9, Placeholder Sword found and working in a Journey world.
- .NET 8 SDK installed; command-line build works (see Build notes).

- Phase 1 code written (NOT yet tested in-game): Heavenly Teleporter item + recipe (Celestial Sigil + Copper Shortsword at an Ancient Manipulator), Debug Starter Kit, localization file.

## Next
- Owner tests Phase 1 in-game, then Phase 2 (the Heavens).
- Open question: GitHub repo URL for the first push (Phase 0 tag).

## Build notes
- Project folder MUST be named MotherOfTerraria (tModLoader uses the folder name as the mod name). ModSources/MotherOfTerraria is a symlink to it.
- If `dotnet build` hangs at restore, run with `DOTNET_SYSTEM_NET_DISABLEIPV6=1` (this network drops IPv6).

## Known issues
- Celestial Sigil is crafted from 12 of each Lunar Fragment. Unverified whether fragments are still obtainable after Moon Lord is dead (check in a real playthrough).
- Unverified: a prefixed Copper Shortsword is accepted by the recipe (test in-game).
- Close tModLoader before building from the command line (it locks the .tmod); in-game Build + Reload is the normal path.
- Heavenly Teleporter reference art is a mockup, not a game-ready sprite (see ASSETS.md).
