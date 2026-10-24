# Progress

Last updated: 2026-10-24 (Phase 5 skeleton built).
Read ROADMAP.md (plan and rules), this file, and ASSETS.md (placeholder list) first. Work in the Documents folder, not the old Trash copy.

## Where we are
- Phase 0 COMPLETE (2026-10-11). Phase 1 COMPLETE (2026-10-11).
- Phase 2 (the Heavens subworld) COMPLETE for singleplayer: 2.1 to 2.5 confirmed in-game. 2.6 (multiplayer testing) is DEFERRED to Phase 8 by owner decision (see Decisions).
- Phase 2C (Heavens details and polish) COMPLETE by owner decision (2026-10-14: "phase 2c works"). Built and seen in-game: enemies (tuned), island look, atmosphere, island details, theme music. Skipped or deferred, not built: ambient creatures, grand arrival gateway, ambient sounds, floating canopy decorations, the performance check (do it before release), and the open decisions (dragon debuff, third enemy, bark close-up).
- Phase 3 (the Heavenly Basket) COMPLETE (owner, 2026-10-14). Phase 4 (summon sequence) CONFIRMED working by the owner (2026-10-24, along with everything else on the test list). Phase 5 (boss skeleton) BUILT 2026-10-24, awaiting the owner's test; the Basket-emptying rule is hooked up. Phase 6 onward not started.

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

### Phase 3: the Heavenly Basket (COMPLETE, confirmed by the owner)
- Owner decisions (2026-10-14): 30 crystals fill it (first 50, changed to 30 the same day; the constant is HeavenlyBasketPlayer.CrystalsNeeded); crystals stay in the Basket until it is full; they are only lost if the fight was started and lost; it stands in the centre of the top central island (the High island, x=330; I read "the centre top island" this way, so check it is the island the owner meant); it replaces the pylon; placeholder art for now (the owner is sending a design).
- Built: Tiles/HeavenlyBasket (now 8x9 tiles with the owner's design, see the art note below; in Tiles/HeavensObjectTile.cs), placed by the terrain pass in place of the High island's old centre statue (trees and saplings keep clear of it). Right-click puts in the crystals you carry (up to what is missing); hovering shows the crystal icon and "n/50"; it glows brighter as it fills; protected from breaking.
- Fill is stored per player in Common/HeavenlyBasketPlayer.cs (saved with the character). When it reaches 30 the PLACEHOLDER fight starts (chat message and sound; it fires once, because the Basket refuses crystals while a fight runs and once it is full). **Nothing empties the Basket for now** (owner, 2026-10-14): not leaving the Heavens, not quitting the game, not dying. The first version emptied it on a 60 s placeholder win and on leaving mid-fight; both were removed because the owner did not want that.
- **OWNER RULE FOR WHEN THE BOSS EXISTS (Phase 5; also written into ROADMAP.md Phases 3 and 5):** the Basket empties only (1) when the boss is KILLED, or (2) when the boss DISAPPEARS because the player died. Hook up HeavenlyBasketPlayer.EndFight(true) for a kill and EndFight(false) for the player-death case when the boss is alive and fightable. Still undecided: what leaving the Heavens or quitting mid-fight does once a boss is alive (ask the owner then).
- SINGLEPLAYER-ONLY: the fill and fight state live on the local player; multiplayer needs them moved to the server (Phase 8).
- Debug: the Debug Starter Kit now also gives 30 Heavens Crystals (exactly one Basket).
- Basket art (2026-10-14, owner design, NOT yet seen in-game): the owner's middle basket (DesignRefs/basket_design.jpg) is the tile, made BIG (8x9 tiles, 128x144 px) by tools/make_basket_sprite.py. As it fills, the light in the bowl gets brighter: the base art has the bowl light dimmed to 45%, a _Glow overlay sheet is drawn on top additively in proportion to the fill, plus a soft light orb above the bowl and rising sparkles from 10% fill. The player's fill drives it (per-player, so only the local player's fill is shown). The glow position (BowlCentre in HeavenlyBasket) and the glow ellipse in the script were set by eye: tune if they look off. The design's inventory-item and 'glowing/open' variants are unused for now.
- BUG FOUND AND FIXED (2026-10-14): the first build failed to load ("Asset could not be found: Tiles/HeavenlyBasket_Highlight"). A tile with TileID.Sets.HasOutlines needs a `<TileName>_Highlight.png` of the same size; now generated by tools/make_heavens_decor.py. Remember this for any future clickable tile (make_basket_sprite.py now makes it).

### Phase 4: the summon sequence (BUILT, NOT YET TESTED IN-GAME)
- Owner's script (2026-10-14), counted from the moment the Basket becomes full: 10 s chat "A heavenly miracle is upon us"; 15 s the screen shakes for 2 s (and again at 22 s and 28 s, same length and strength, added on request); 20 s chat "The mother of this world will protect us all"; 25 s chat "As she is.... The Mother of Terraria"; 30 s the boss spawns. The sky does NOT darken (not in the owner's script; my earlier suggestion was dropped).
- Code: Common/HeavenlyBasketPlayer.cs (StartSummon, RunSummon, SpawnBoss); the timings are constants at the top of that section (60 ticks = 1 s). The shake is a PunchCameraModifier. A sound plays at the start and at the shake.
- The sequence starts when the 30th crystal goes in, or when a full Basket is right-clicked (so an interrupted sequence can be run again). It cannot start twice at once. Leaving the Heavens or dying cancels it; the crystals stay in the Basket.
- At 30 s a PLACEHOLDER message says the boss would spawn here (and a roar sound), then the sequence ends and the crystals STAY (owner rule: they only empty when the boss is killed or vanishes because the player died; Phase 5 hooks that up). Phase 5 replaces SpawnBoss with the real spawn.
- SINGLEPLAYER-ONLY: it all runs on the local player; the boss spawn must move to the server for multiplayer (Phase 8).

### Process
- Everything above is committed locally (nothing pushed). ROADMAP.md status block updated.

## What we did on 2026-10-24: Phase 5, the boss skeleton (BUILT, NOT YET TESTED IN-GAME)
- Owner confirmed that everything on the previous "not yet tested" list works (Phase 4 sequence, trees, dragon, crystal icon, drop rate).
- New: NPCs/Bosses/MotherOfTerrariaBoss.cs (the class is not called MotherOfTerraria because that clashes with the mod's own name). PLACEHOLDER numbers: 300000 health, defense 100, contact 200, knockback-immune, lava-immune, flies through terrain, boss health bar, boss music (vanilla Boss2 stands in) at priority BossMedium so it beats the Heavens theme. AI: floats 260 px above the player swinging side to side; every 2.5 s gathers gold light for 0.5 s (the tell), then throws a fan of 3 Heavenly Spears (damage 150). If the player dies or leaves she flees and despawns. No bestiary entry yet.
- Spawn: HeavenlyBasketPlayer.SpawnBoss now really spawns her 300 px above the player at the end of the 30 s sequence. FightActive stays true while she lives.
- Basket rule hooked up: her OnKill calls EndFight(true); if the player dies while she lives, HeavenlyBasketPlayer.PostUpdate removes her and calls EndFight(false). Nothing else empties the Basket.
- DEFAULT I CHOSE (owner to confirm): leaving the Heavens (or quitting) mid-fight just ends the fight, she is gone with the subworld, and the crystals STAY (the owner's rule empties the Basket only on a kill or a player death). Right-click the full Basket to summon her again.
- The Heavens spawner pauses while she is alive (no knights or dragons during the boss fight).
- Loot: PLACEHOLDER 20-30 Lunar Bars. No treasure bag, trophy, relic or Expert-only drops yet (they need art and design: Phases 6 and 7).
- Debug: Items/Debug/DebugBossSummoner (craft from 1 Dirt + 1 Stone) summons her instantly inside the Heavens. Killing a boss summoned this way does not touch the Basket.
- Art: placeholder sprite made by tools/make_boss_placeholder.py (listed in ASSETS.md).
- Compiles (the command-line build only stops at the packaging error TML003 because tModLoader is open). Needs the in-game Build + Reload.
- SINGLEPLAYER-ONLY: the spawn and OnKill hook use the local player; multiplayer needs the server to do them (Phase 8).

## Not yet tested in-game
0a. PHASE 5 (2026-10-24): see the section above. Test steps are in the message that closed that session.

## Old test list (all confirmed by the owner on 2026-10-24)
Compiles, but the owner has not explicitly confirmed (test these first next session):
0. PHASE 4: the whole 30 s summon sequence (chat lines at 10/20/25 s, shakes at 15/22/28 s, the placeholder boss message at 30 s, restart by right-clicking the full Basket, cancel by leaving or dying) and the 30-crystal Basket with its growing glow. Also the Basket's size and glow position.
1. Trees standing still in the Heavens, and swaying again in the normal world after leaving (Systems/HeavensWindSystem.cs).
2. The dragon without its roar; the new crystal icon in the inventory and on the ground.
3. Crystal drop rate feel (1 crystal at 50%, Basket needs 30, so about 60 kills): tune if it feels too grindy or too easy.
Confirmed by the owner this session: enemy tuning, knockback, spear homing, fall death, the atmosphere and island details, the music.
Remember: tModLoader must be closed for command-line builds; the in-game Build + Reload is the normal path.

## Next: test Phase 5, then Phase 6 (boss design doc and attacks; Opus recommended for the design doc). The list below is the leftover 2C wish list, optional and can be picked up any time
1. Ambient creatures (glowing butterflies or birds), non-hostile; and a grand arrival gateway where the player lands.
2. Ambient sounds (wind, chimes): needs sounds from the owner, or a pick from vanilla ones.
3. Floating canopy puffs and leafy branches (DesignRefs: tree_puff.png, tree_branch.png) as extra floating decorations, if wanted.
4. Owner decisions still open: the dragon's debuff (fire does not match its golden-blue look; a holy or light effect may suit it), whether a third enemy type is wanted, and whether to send a bark close-up so the trunk art can match their style.
5. Performance check (frame rate, load time): do it before release.
Real art still wanted whenever the owner has it (generated placeholders): turf tile, stone tile, tree bark, bridge posts, flowers, vines. The crystal now uses the owner's design. See ASSETS.md.

## Decisions (keep)
- SINGLEPLAYER-FIRST (owner, 2026-10-12): the mod is solo-only for now. Multiplayer testing (old 2.6) moved to the "Multiplayer upgrade" step in Phase 8, after the boss works. Keep server-authoritative habits (logic on the server; only visuals, sound and UI on the client) and tag shortcuts with `// SINGLEPLAYER-ONLY:` comments. Before the upgrade: check that the dedicated server's hardware can run tModLoader (ARM support, spare RAM for SubworldLibrary's extra process) and keep SubworldLibrary versions matching.
- Boss summon (owner, 2026-10-14, FINAL): Heavens enemies drop crystals (1, at 50%); the Heavenly Basket (centre of the High island, replaces the pylon) takes 30 and starts a 30 s scripted sequence, then the boss spawns. The Basket empties ONLY when the boss is killed or vanishes because the player died; until then nothing empties it. Open for Phase 5: what leaving or quitting mid-fight does once a boss is alive.
- Enemy tuning (owner, 2026-10-14): knight 35000 HP, dragon 50000 HP, knight knockback-immune, dragon 0.01, weakly homing spear, 1 crystal at 50%, no dragon roar, fall death below row 330.
- Look and sound (owner, 2026-10-14): the owner's crystal, basket and music are in; stone spikes under islands removed; sky does not darken in the summon.
- Phases: 2C complete, 3 complete, 4 built and awaiting a test. The owner chose to move on from 2C before the deferred items.
- Enemy design decisions: knight is a ground enemy with a slow spear; dragon is a flying wyvern with swoops, not a dash; dragons are rarer and stronger than knights.
- Heavens look decisions: golden luminous turf; owner's heavenly trees as REAL tile trees; no ruins; no waterfalls; no shimmer; trees stand still.
- The ROADMAP.md file is git-ignored (kept local only), so it is not backed up on GitHub.

- Process note: ROADMAP.md holds the plan, rules and decisions (kept up to date as we go); PROGRESS.md holds what was done, what is untested, and the next steps; ASSETS.md holds every placeholder.

## Build notes
- Project folder MUST be named MotherOfTerraria (tModLoader uses the folder name as the mod name). ModSources/MotherOfTerraria is a symlink to it.
- SubworldLibrary is a required mod (build.txt modReferences). For compiling, its DLL is extracted from the Workshop .tmod into lib/SubworldLibrary.dll (git-ignored, not shipped). Re-extract if SubworldLibrary updates (Workshop ID 2785100219; version used 2.3.0.1).
- SubworldLibrary Enter/Exit must be called on the player's own client; in multiplayer they send a request to the server (nothing happens on the server; use MovePlayerToSubworld there).
- SubworldLibrary's NormalUpdates is false: the day/night clock and random world ticks do not run in the Heavens (we lock time to midday ourselves), but NPCs and projectiles update normally.
- A clickable tile (TileID.Sets.HasOutlines) needs a `<TileName>_Highlight.png` or the mod fails to load ("Asset could not be found"). For mod music, `ModSceneEffect.Priority` must beat the vanilla effects (use Event); BiomeHigh lost and the theme never played.
- If `dotnet build` hangs at restore, run with `DOTNET_SYSTEM_NET_DISABLEIPV6=1` (this network drops IPv6).
- The command-line build compiles the code but cannot package the mod while tModLoader is open (error TML003). Compile errors still show. The in-game Build + Reload is the normal path.
- tModLoader rewrites Localization/*.hjson on build and adds missing keys in a compact dotted form (e.g. `NPCs.HeavenlyKnight.DisplayName: ...`); that is normal and valid.
- Sprite sheets are regenerated by scripts in tools/ (ImageMagick and numpy needed). tools/ and DesignRefs/ are excluded from the mod build (build.txt buildIgnore).

## Known issues
- Celestial Sigil is crafted from 12 of each Lunar Fragment. Unverified whether fragments are still obtainable after Moon Lord is dead (check in a real playthrough).
- Owner reported the recipe working; the prefixed-sword and missing-ingredient cases were not individually confirmed.
- Heavenly Teleporter reference art is a mockup cut-out, not a hand-made sprite (see ASSETS.md).
- Enemy stats were tuned on 2026-10-14 with the owner; they assume Expert/Master (the owner never said which difficulty they play). The dragon's fire debuff may not suit its design.
- Generated placeholder art: turf, stone, bark, bridge posts, flowers, vines (see ASSETS.md). The crystal and the Basket use the owner's designs.
- Everything about the Basket and the summon is SINGLEPLAYER-ONLY (fill and sequence run on the local client).
- Unverified: the glow and orb position over the Basket's bowl were set by eye.

## Repo and GitHub
- GitHub repo (public): https://github.com/Unix-H/mother-of-terraria-mod (main + tags phase-0, phase-1, pushed 2026-10-11). Remote uses SSH (key ~/.ssh/id_ed25519_github). Pushed again on 2026-10-14 at the owner's request (main up to baef8fc: Phases 2C, 3 and 4 work). Never push without the owner saying so.
- Open decision for the owner: scrub ROADMAP.md from the GitHub history, or make the repo private (the repo is public and an early commit contained ROADMAP.md before it was git-ignored).
