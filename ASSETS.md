# Assets checklist

All art is placeholder until the owner supplies finals. Replace by dropping a PNG with the same filename over the filler (no code changes).

| Asset | File | Size / frames | Status |
|---|---|---|---|
| Placeholder Sword (Phase 0 test item) | Items/PlaceholderSword.png | 40x40, single frame | Placeholder (delete with the item before release) |
| Heavenly Teleporter (item) | Items/HeavenlyTeleporter.png | 64x64, single frame, transparent PNG | Cut out and downscaled from DesignRefs/teleporter.jpg (owner design). Replace with a hand-made 64x64 version later if wanted. |
| Debug Starter Kit (test item) | Items/Debug/DebugStarterKit.png | 32x32, single frame | Placeholder (delete with the item before release) |
| Heavens background (empty, so no mountains show) | Backgrounds/HeavensEmpty.png | 256x256, fully transparent | Placeholder. Real scenery art optional in Phase 2C (e.g. distant floating islands). |
| Golden Statue (Heavens decoration tile) | Tiles/GoldenStatue.png | 52x106: 3x6 tiles of 16px with 2px padding between tiles, transparent PNG, single frame | Placeholder (generated: robed figure with halo and spear). Owner design to replace it. |

## Vanilla stand-ins (borrowed Terraria visuals that should become custom)

These are not files in the mod; the code uses the game's own graphics for now. Each needs a custom replacement (new tile/texture + a small code change).

| Stand-in | Where | Planned fix |
|---|---|---|
| Hallowed Grass (island tops) | Subworlds/HeavensTerrainPass.cs | Owner dislikes the look. Custom heavenly surface tile, Phase 2C. |
| Hallowed-style trees | Subworlds/HeavensTerrainPass.cs (WorldGen.GrowTree) | Custom trees or tree-like landmarks, Phase 2C. |
| Cloud blocks (island bodies, bridges) | Subworlds/HeavensTerrainPass.cs | Custom cloud/marble-like body block, Phase 2C. |
| Cloud bridges between islands | Subworlds/HeavensTerrainPass.cs (BuildBridge) | Golden heavenly bridges, and move islands closer together, Phase 2C. |
| Sky: now a custom gradient (Systems/HeavensSky.cs). Vanilla sun and clouds still show | Systems/HeavensSky.cs | Custom background art (layered parallax sky, halo), Phase 2C. |
| Vanilla loading screen text | Subworlds/HeavensTerrainPass.cs | Optional custom loading screen, Phase 2C. |
| Gold dust when hitting the statue | Tiles/GoldenStatue.cs | Custom dust, optional. |

## Not built yet (will need owner designs)

| Asset | Phase |
|---|---|
| Heavens music track (vanilla Space music plays as placeholder, see Systems/HeavensSceneEffect.cs) | 2C |
| Heavens ambient sounds | 2C |
| Heavens enemies (sprites, animations) and their heavenly spears (projectile art) | 2C |
| Harmless ambient creatures (butterflies, birds) | 2C |
| Pylon-like structure (tile) | 3 |
| Mother of Terraria boss (sprite, animation, projectiles), boss bar icon, trophy, relic, drops, music | 5, 6, 7 |

## Design references (not used by the game directly)

| Reference | File | Notes |
|---|---|---|
| Heavenly Teleporter design | DesignRefs/teleporter.jpg | 2400x1792 mockup: golden winged sword, blue gem in the guard, blue core in the blade, purple grip, star pommel. Has a fake checkerboard background, so it is a reference only. A real item sprite (transparent PNG, roughly 40-64 px) must be made from it. Excluded from the mod build. |
