# Progress

## Done
- Phase 0 COMPLETE (confirmed in-game 2026-10-11): mod builds, loads in tModLoader 1.4.4.9, Placeholder Sword found and working in a Journey world.
- .NET 8 SDK installed; command-line build works (see Build notes).

- Phase 1 COMPLETE (confirmed in-game 2026-10-11): Heavenly Teleporter item + recipe (Celestial Sigil + Copper Shortsword, craftable only at an Ancient Manipulator), Debug Starter Kit (craft from 1 Dirt Block), localization file, sprite cut from the owner's design.

## Next
- Phase 2 (the Heavens). Waiting for owner's go-ahead.
- Open question: GitHub repo URL for the first push (Phase 0 tag).

## Build notes
- Project folder MUST be named MotherOfTerraria (tModLoader uses the folder name as the mod name). ModSources/MotherOfTerraria is a symlink to it.
- If `dotnet build` hangs at restore, run with `DOTNET_SYSTEM_NET_DISABLEIPV6=1` (this network drops IPv6).

## Known issues
- Celestial Sigil is crafted from 12 of each Lunar Fragment. Unverified whether fragments are still obtainable after Moon Lord is dead (check in a real playthrough).
- Owner reported the recipe working; the prefixed-sword and missing-ingredient cases were not individually confirmed.
- Close tModLoader before building from the command line (it locks the .tmod); in-game Build + Reload is the normal path.
- Heavenly Teleporter reference art is a mockup, not a game-ready sprite (see ASSETS.md).
