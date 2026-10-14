# Progress

Last updated: 2026-10-14 (session 2).
Read ROADMAP.md (plan and rules), this file, and ASSETS.md (placeholder list) first. Work in the Documents folder, not the old Trash copy.

## Where we are
- Phase 0 COMPLETE (2026-10-11). Phase 1 COMPLETE (2026-10-11).
- Phase 2 (the Heavens subworld) COMPLETE for singleplayer: 2.1 to 2.5 confirmed in-game. 2.6 (multiplayer testing) is DEFERRED to Phase 8 by owner decision (see Decisions).
- Phase 2C (Heavens details and polish) COMPLETE by owner decision (2026-10-14: "phase 2c works"). Items skipped or deferred, not built: ambient creatures, grand arrival gateway, ambient sounds, floating canopy decorations, the performance check (do it before release), and the open decisions (dragon debuff, third enemy, bark close-up). Previous status: nearly done: enemies (tuned), island look, atmosphere, island details and the theme music are built and the owner has seen them in-game. Ambient creatures, the arrival gateway, ambient sounds, performance check and the open owner decisions remain.
- Phase 3 (the Heavenly Basket) BUILT 2026-10-14, awaiting the owner's in-game test (see "Phase 3" below). Phase 4 onward (summon sequence, boss) not started.

## What we did on 2026-10-12

### Phase 2 (subworld), all confirmed in-game by the owner
- 2.1: SubworldLibrary 2.3.0.1 added as a dependency, empty Heavens subworld.
- 2.2: the Heavenly Teleporter enters the Heavens; using it inside returns home (it is the only way out). Inventory intact.
- 2.3: rules inside: everything allowed in the normal world stays allowed (wings, mounts, hooks, jumps, Rod of Discord); escape items blocked (Magic Mirror, Ice Mirror, Recall, Potion of Return, Cell Phone, Shellphones, Conches); the Heavens' own terrain cannot be broken, blown up or replaced (HeavensSubworld.IsProtectedTile); ordinary placed blocks are free.
- 2.4/2.5: fixed-layout island world, custom sky (azure to gold gradient, warm tint, fewer clouds), no vanilla mountains (empty background style), placeholder music (vanilla Space), permanent midday, no vanilla enemy spawns (only this mod's NPCs).
- Crash fixed on the way: ManageSpecialBiomeVisuals needs a screen filter; the scene effect now drives the sky directly.

### Phase 2C: enemies (owner's real sprites for all)
- Heavenly Knight (ground): walks toward the player, stops at island edges, 0.75 s glow tell then throws a SLOW heavenly spear, contact damage, faces the player (art faces right, spriteDirection reversed), 7-frame animation (stand + 6 walk) made by tools/make_knight_sheet.py.
- Heavens Dragon (flying wyvern): owner's long golden dragon, 8-frame animation (body ripple + wing flap) by tools/make_dragon_sheet.py. Attack: approach, 0.5 s roar-and-gather warning, homing swoop (turn-limited), retreat to one side, repeat. NO dash. On Fire! for 5 s on any hit, fire-immune, flies through terrain, only the head is a hitbox.
- Heavens Crystal: placeholder item (Items/HeavensCrystal), dropped by both enemies.
- Custom spawner (Subworlds/HeavensSpawner.cs): spawns by hand (not Terraria's spawn system). Knights on the ground 60-85 tiles away; dragons in the air. Knights are picked 2 times in 3, dragons 1 in 3.
- PLACEHOLDER numbers (to tune): knight 13500 health (raised 50% on 2026-10-12), defense 60, contact 150, spear damage 120, spear speed 10, max 3 at once, drops crystals 1 in 3 for 1-2. Dragon 12000 health, defense 70, contact 190, swoop damage x1.3, max 1 at once, drops crystals 1 in 2 for 1-3.

### Phase 2C: the look of the Heavens
- World is 700x400. Four long islands (Arrival 140, High 330, Low 335, flat Arena 538) plus four small floaters, about 15-20 tile gaps. Fixed layout, same every visit.
- Custom golden luminous Heavens Turf tile (Tiles/HeavensTurf, standard block sheet from tools/make_turf_sheet.py). PLACEHOLDER look: speckled gold with amber edges.
- Golden bridges: vanilla Golden Platform tiles (protected by style) with glowing Golden Bridge Posts (placeholder art).
- Real Terraria trees: custom ModTree "HeavenlyTree" (trunk, branches, crown) growing on the turf, grown with WorldGen.GrowTree, fixed seed. Crowns are the owner's canopies; bark is generated (tools/make_tree_textures.py, shape templates in tools/templates from tModLoader's ExampleMod, MIT).
- Trees and plants STAND STILL in the Heavens (Systems/HeavensWindSystem.cs hooks On_TileDrawing.GetWindCycle). The normal world is unaffected.
- Owner's golden statue (8x7 tiles, 6 placed), sapling and stump decorations (owner's art, 2x3 and 3x2 tiles).
- Tried and then REMOVED at the owner's request: hallowed grass look, vanilla trees painted gold, temple ruins, waterfalls (cancelled), shimmer puddles (two versions).

### Infrastructure and process
- Singleplayer-first decision recorded; the Teleporter refuses to work in multiplayer with a message (tagged SINGLEPLAYER-ONLY).
- CLAUDE.md added: tells a new session to read ROADMAP.md, PROGRESS.md and ASSETS.md first.
- ASSETS.md lists every placeholder and stand-in. DesignRefs/ holds the owner's design images and cut-outs. tools/ holds the scripts that regenerate sprite sheets (need ImageMagick and numpy).
- All work is committed locally. NOTHING has been pushed since 2026-10-11 (the branch is 36 commits ahead of origin). Never push without the owner saying so.

## What we did on 2026-10-14

### Enemies (owner approved; "now it is a challenge")
- Knight: health 35000, defense 70, spear damage 140 (contact 150 unchanged). Dragon: health 50000, defense 80 (contact 190, swoop x1.3 unchanged). Aim: 8-15 s to kill at endgame damage; 3 hits kill an unbuffed player. The damage numbers assume Expert/Master (base is scaled up there); the owner has not said which difficulty they play, so re-check if it ever feels soft.
- Knockback: knight FULLY immune (knockBackResist 0); dragon takes a tiny bit (0.01). (Went 0.15/0.1 to 0.04/0.02 to 0/0, then the owner wanted the dragon to take a little again.)
- Crystal drops: both enemies drop exactly 1 crystal, 50% of the time (was knight 1 in 3 for 1-2, dragon 1 in 2 for 1-3). Spawn caps unchanged.
- Knight spear weak homing (owner said "dragon spears" but only the knight throws spears): turns 108 deg/s (first 72, raised at owner request) for the first 1.5 s, stops steering within 140 px, so jumping does not dodge it but a late sidestep does. Constants at the top of Projectiles/HeavenlySpear.cs. Owner: "good job".
- Dragon: removed the vanilla roar sound from the swoop warning (owner found it annoying). The visual tell (light gathering at the head) is unchanged. NOT yet re-tested.

### Heavens rules and items
- Fall death: Common/HeavensFallDeath.cs kills the player who falls below Main.worldSurface (row 330, where the cave background starts) in the Heavens. Owner: "good job".
- Heavens Crystal icon: now the owner's big crystal (DesignRefs/crystal_design.jpg), shrunk to 22x32 and given a Terraria look (no glow, 1 px outline, small palette) by tools/make_crystal_icon.py; item hitbox updated to 22x32. NOT yet seen in-game. Owner may still replace it with hand-made art.

### Atmosphere and island details (owner: "the heaven looks great")
- Sky (Systems/HeavensSky.cs, HeavensSkyTextures.cs): sun with glow and two rotating halo rings, 7 slow light beams, 3 parallax cloud layers, 70 twinkling golden motes. All procedural, no art files.
- Foreground (Systems/HeavensAmbientSystem.cs): gold sparkle and pink petal dust drifting across the screen.
- Islands (Subworlds/HeavensTerrainPass.cs): 12 floating rocks of the new Heavens Stone tile, flowers (3 kinds) on island tops, hanging vines (2 lengths) under islands. New tiles are protected (HeavensSubworld.IsProtectedTile). Art is placeholder (see ASSETS.md; scripts tools/make_stone_sheet.py, make_heavens_plants.py).
- To tune: cloud/beam/mote numbers in HeavensSky.cs, the `Rocks` list and the spawn chances in ScatterFlowers/HangVines.

- Removed (owner request, 2026-10-14): the stone spikes under the main islands (they looked like odd T-shaped blocks). Floating rocks, flowers and vines stay. NOT yet re-checked in-game.

### Music (confirmed working in-game)
- The owner's track "Beneath the Spires" is the Heavens theme. Source mp3 kept in DesignRefs/music/; the game uses Assets/Music/Heavens.ogg, made with: ffmpeg -i DesignRefs/music/Beneath_the_Spires.mp3 -af "volume=-3dB" -c:a libvorbis -q:a 6 Assets/Music/Heavens.ogg. The original was -13 LUFS; now turned down 3 dB (to about -16 LUFS; it was 5 dB, the owner asked for a bit louder). Change the dB value and re-run to adjust.
- BUG FOUND AND FIXED: at first the vanilla cave-like music played instead, because HeavensSceneEffect.Priority (BiomeHigh) lost to a higher-priority vanilla effect. It is now SceneEffectPriority.Event (above all vanilla biomes and environments, below the bosses). When the boss fight is built, its music must use a boss priority to take over. Ambient sounds are still open.

### Phase 3: the Heavenly Basket (BUILT, NOT YET TESTED IN-GAME)
- Owner decisions (2026-10-14): 50 crystals fill it; crystals stay in the Basket until it is full; they are only lost if the fight was started and lost; it stands in the centre of the top central island (the High island, x=330; I read "the centre top island" this way, so check it is the island the owner meant); it replaces the pylon; placeholder art for now (the owner is sending a design).
- Built: Tiles/HeavenlyBasket (4x3 tiles, in Tiles/HeavensObjectTile.cs), placed by the terrain pass in place of the High island's old centre statue (trees and saplings keep clear of it). Right-click puts in the crystals you carry (up to what is missing); hovering shows the crystal icon and "n/50"; it glows brighter as it fills; protected from breaking.
- Fill is stored per player in Common/HeavenlyBasketPlayer.cs (saved with the character). When it reaches 50 the PLACEHOLDER fight starts (chat message and sound); the Basket refuses crystals while a fight is running, so it fires once per attempt. The fight ends as a loss if the player dies, leaves the Heavens, or quits the game mid-fight (crystals reset to 0). With no boss yet, the placeholder fight ends as a WIN after 60 s (also resets to 0). The boss code (Phase 5) must call HeavenlyBasketPlayer.EndFight(won) instead of the timer; Phase 4 replaces StartFight with the real summon sequence.
- ASSUMPTION to confirm with the owner: a WIN also empties the Basket (otherwise the boss could be re-summoned forever), and leaving the Heavens mid-fight counts as a loss (otherwise leaving would dodge the loss).
- SINGLEPLAYER-ONLY: the fill and fight state live on the local player; multiplayer needs them moved to the server (Phase 8).
- Debug: the Debug Starter Kit now also gives 50 Heavens Crystals.
- BUG FOUND AND FIXED (2026-10-14): the first build failed to load ("Asset could not be found: Tiles/HeavenlyBasket_Highlight"). A tile with TileID.Sets.HasOutlines needs a `<TileName>_Highlight.png` of the same size; now generated by tools/make_heavens_decor.py. Remember this for any future clickable tile.

### Process
- Everything above is committed locally (nothing pushed). ROADMAP.md status block updated.

## Not yet tested in-game
Compiles and was loaded, but the owner has not explicitly confirmed:
1. Trees standing still in the Heavens, and swaying again in the normal world after leaving (Systems/HeavensWindSystem.cs).
2. The dragon without its roar; the new crystal icon in the inventory and on the ground.
3. Crystal drop rate feel (1 crystal, 50%): decide together with the Basket design.
Confirmed by the owner this session: enemy tuning, knockback, spear homing, fall death, the atmosphere and island details, the music.
Remember: tModLoader must be closed for command-line builds; the in-game Build + Reload is the normal path.

## Next: test Phase 3, then Phase 4 (summon sequence); the list below is the leftover 2C wish list, optional and can be picked up any time
1. Ambient creatures (glowing butterflies or birds), non-hostile; and a grand arrival gateway where the player lands.
2. Ambient sounds (wind, chimes): needs sounds from the owner, or a pick from vanilla ones.
3. Floating canopy puffs and leafy branches (DesignRefs: tree_puff.png, tree_branch.png) as extra floating decorations, if wanted.
4. Owner decisions still open: the dragon's debuff (fire does not match its golden-blue look; a holy or light effect may suit it), whether a third enemy type is wanted, and whether to send a bark close-up so the trunk art can match their style.
5. Performance check (frame rate, load time), then mark Phase 2C complete and plan Phase 3 (the Heavenly Basket: how many crystals fill it, whether it resets, what happens on defeat, where it stands, its look).
Real art still wanted whenever the owner has it (generated placeholders): turf tile, stone tile, tree bark, bridge posts, flowers, vines. The crystal now uses the owner's design. See ASSETS.md.

## Decisions (keep)
- SINGLEPLAYER-FIRST (owner, 2026-10-12): the mod is solo-only for now. Multiplayer testing (old 2.6) moved to the "Multiplayer upgrade" step in Phase 8, after the boss works. Keep server-authoritative habits (logic on the server; only visuals, sound and UI on the client) and tag shortcuts with `// SINGLEPLAYER-ONLY:` comments. Before the upgrade: check that the dedicated server's hardware can run tModLoader (ARM support, spare RAM for SubworldLibrary's extra process) and keep SubworldLibrary versions matching.
- Boss summon idea (owner, planning stage, specifics TBC): Heavens enemies drop crystals; a Heavenly Basket in the middle of the Heavens summons the boss when full. Logged in ROADMAP.md (Phases 2C and 3).
- Enemy design decisions: knight is a ground enemy with a slow spear; dragon is a flying wyvern with swoops, not a dash; dragons are rarer and stronger than knights.
- Heavens look decisions: golden luminous turf; owner's heavenly trees as REAL tile trees; no ruins; no waterfalls; no shimmer; trees stand still.
- The ROADMAP.md file is git-ignored (kept local only), so it is not backed up on GitHub.

- Process note: ROADMAP.md holds the plan, rules and decisions (kept up to date as we go); PROGRESS.md holds what was done, what is untested, and the next steps; ASSETS.md holds every placeholder.

## Build notes
- Project folder MUST be named MotherOfTerraria (tModLoader uses the folder name as the mod name). ModSources/MotherOfTerraria is a symlink to it.
- SubworldLibrary is a required mod (build.txt modReferences). For compiling, its DLL is extracted from the Workshop .tmod into lib/SubworldLibrary.dll (git-ignored, not shipped). Re-extract if SubworldLibrary updates (Workshop ID 2785100219; version used 2.3.0.1).
- SubworldLibrary Enter/Exit must be called on the player's own client; in multiplayer they send a request to the server (nothing happens on the server; use MovePlayerToSubworld there).
- SubworldLibrary's NormalUpdates is false: the day/night clock and random world ticks do not run in the Heavens (we lock time to midday ourselves), but NPCs and projectiles update normally.
- If `dotnet build` hangs at restore, run with `DOTNET_SYSTEM_NET_DISABLEIPV6=1` (this network drops IPv6).
- The command-line build compiles the code but cannot package the mod while tModLoader is open (error TML003). Compile errors still show. The in-game Build + Reload is the normal path.
- tModLoader rewrites Localization/*.hjson on build and adds missing keys in a compact dotted form (e.g. `NPCs.HeavenlyKnight.DisplayName: ...`); that is normal and valid.
- Sprite sheets are regenerated by scripts in tools/ (ImageMagick and numpy needed). tools/ and DesignRefs/ are excluded from the mod build (build.txt buildIgnore).

## Known issues
- Celestial Sigil is crafted from 12 of each Lunar Fragment. Unverified whether fragments are still obtainable after Moon Lord is dead (check in a real playthrough).
- Owner reported the recipe working; the prefixed-sword and missing-ingredient cases were not individually confirmed.
- Heavenly Teleporter reference art is a mockup cut-out, not a hand-made sprite (see ASSETS.md).
- All enemy stats are placeholders; the dragon's fire debuff may not suit its design.
- Generated placeholder art: turf, bark, bridge posts, crystal (see ASSETS.md).

## Repo and GitHub
- GitHub repo (public): https://github.com/Unix-H/mother-of-terraria-mod (main + tags phase-0, phase-1, pushed 2026-10-11). Remote uses SSH (key ~/.ssh/id_ed25519_github). Nothing pushed since; the branch is 36 commits ahead.
- Open decision for the owner: scrub ROADMAP.md from the GitHub history, or make the repo private (the repo is public and an early commit contained ROADMAP.md before it was git-ignored).
