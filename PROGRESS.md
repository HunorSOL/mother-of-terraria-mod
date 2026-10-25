# Progress

Last updated: end of day 2026-10-24 (session 3: Phase 5 boss built with a first stage of seven attacks, six after 2026-10-25; pushed to GitHub at the owner's request).
Read ROADMAP.md (plan and rules), this file, and ASSETS.md (placeholder list) first. Work in the Documents folder, not the old Trash copy.

## Where we are
- Phase 0 COMPLETE (2026-10-11). Phase 1 COMPLETE (2026-10-11).
- Phase 2 (the Heavens subworld) COMPLETE for singleplayer: 2.1 to 2.5 confirmed in-game. 2.6 (multiplayer testing) is DEFERRED to Phase 8 by owner decision (see Decisions).
- Phase 2C (Heavens details and polish) COMPLETE by owner decision (2026-10-14: "phase 2c works"). Built and seen in-game: enemies (tuned), island look, atmosphere, island details, theme music. Skipped or deferred, not built: ambient creatures, grand arrival gateway, ambient sounds, floating canopy decorations, the performance check (do it before release), and the open decisions (dragon debuff, third enemy, bark close-up).
- Phase 3 (the Heavenly Basket) COMPLETE (owner, 2026-10-14). Phase 4 (summon sequence) CONFIRMED working (owner, 2026-10-24).
- Phase 5 (boss skeleton) and the FIRST STAGE of Phase 6 are BUILT and being tested by the owner: the Mother of Terraria with six attacks in a loop, her real sprite, a health bar, loot placeholder, and the Basket-emptying rule hooked up. The owner has tested most of it and tuned it (see "Current state of the boss" just below, then the dated sections further down for the history and the reasons). NEXT: stage 2 (starts at 50% health, owner decision 2026-10-25, was 60%); the design is waiting for the owner's answers to five questions (see "Next").
- The Heavens is now SAVED with the world (the owner can build there); the floater above the Basket was removed.

## Current state of the boss (end of 2026-10-24; the dated sections below explain each change and why)
- **Where it lives:** NPCs/Bosses/MotherOfTerrariaBoss.cs (class name is not MotherOfTerraria because that is the mod's name). Marked [AutoloadBossHead] (needed for the boss bar), head icon NPCs/Bosses/MotherOfTerrariaBoss_Head_Boss.png.
- **Body:** health EXACTLY 1,014,000 (constant BossLife, set again in OnSpawn after the game's difficulty scaling; the owner tests on Journey), defense 100, contact damage 200, knockback-immune, flies through terrain. Sprite: the owner's design, 8 frames of 420x332 (tools/make_boss_sheet.py: WIDTH 240, UPSCALE 2), hitbox 130x210 (her body, not the wings). Music: vanilla Boss2 stands in, priority BossMedium. Standard boss bar works. Loot: placeholder 20-30 Lunar Bars only (no bag, trophy, relic yet).
- **Spawn:** the Basket sequence spawns her 250 px above the Basket. While the sequence runs and while she lives, knights and dragons are removed and do not spawn (Common/HeavensEnemyCleaner.cs). The Basket empties on her death, or when the player dies; leaving the Heavens mid-fight keeps the crystals (my default, owner has not objected).
- **Movement:** five stations around the player (straight above, high left, high right, above-left, above-right). She moves to the next station only every 3rd attack; one move in three is a fast dash (24 px/tick) with a golden after-image trail, the others a slow drift (7). She stays put when within 60 px of her station.
- **The attack loop (fixed order, then repeats):**
  1. Spear fan: 3 Celestial Spears (the owner's spear art), slightly homing, aimed at the player; tell 30 ticks, ends at 55; damage 150; speed 9.
  2. Spear burst: two rings of 18 Celestial Spears in all directions (second ring offset by half a gap), speed 7; tell 45; ends at 100.
  3. Death beam: a thin line follows the player for 75 ticks and freezes for the last 12, then a thin golden beam (28 px hit width, 56 drawn) fires for 150 ticks and sweeps slowly (about 14 deg/s); damage 350 and Hellfire 6 s. Comes out of her chest.
  4. Falling stars: one row of 7 Gold Crystal Stars (the owner's art) hang 520 px above the player, each with a thick golden line and a ground glow marking where it lands, then drop one after another in a sweep, plus one star that follows the player and locks 25 ticks before its drop. Star damage 320; each landing leaves a Starfire patch (104x44, 5 s, 40% damage). Constant StarRows = 1 (2 is the doubled version the owner found too hard).
  5. Feather storm: wings beat fast (tell 40), then 3 spiral arms of the owner's feather projectile that turn one way and back; one throw every 4 ticks for 70 throws; damage 180; feathers speed up from 5 to 11.
  6. Wing beams: she spreads her wings; a beam fires out of each wing tip, each aimed to one side of the player (gap between them), both sweep the same way; damage 280. (Replaced the first "Wing Wall" idea: the owner's rule is that beams only come out of her.)
  7. (Halo rings: REMOVED 2026-10-25, see the section below.)
- **Owner's rules for her:** make her harder with NEW attacks and a second stage, never by speeding things up (the speed-up was rolled back, "just hard and annoying"); every attack needs a clear tell; beams only come out of her (the star guide lines are the one exception, the owner likes them); size is fine as it is; she must not stay still but also not move too much.
- **Stage 2:** the TRANSITION is built (2026-10-25, see "Stage change: the Enchanted Crystals" below); the stage 2 attacks and look are NOT built. Owner decision: it begins at 50% health (changed from 60% on 2026-10-25). Open questions for the owner: (1) the transition (invulnerable with 3 breakable crystals, or a simple pause), (2) a stage 2 look (a second sprite from the owner, or a tinted placeholder), (3) which new attacks (Star Constellation, Judgment Mark, Healing Orbs, Guardians; I suggested starting with Judgment Mark and Star Constellation), (4) whether the old attacks stay the same or get extra layers (6 rings, a fourth feather arm), (5) a fixed or random order.
- **Debug items for testing:** Debug Starter Kit (Sigil, shortsword, Teleporter, 30 crystals), Debug Boss Summoner (craft from 1 Dirt + 1 Stone; summons her at once inside the Heavens), Debug Stage Tester (craft from 1 Dirt + 1 Stone; a sword that drops her to 50% health). All to be removed before release.

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

### Boss first stage: attacks and health (2026-10-24, later the same day; BUILT, NOT YET TESTED)
Owner feedback: "good start", but the boss is far from final. Asked for: double health, no other enemies while she lives, spears slightly homing and fired in all directions, a big golden Moon-Lord-style laser, all in a cycle; new attacks and a second stage come later.
- Health doubled: 300000 -> 600000 (placeholder; Phase 6 balances).
- Cycle (NPC.ai[0] = attack, ai[1] = timer): SPEAR FAN (3 aimed spears) -> SPEAR BURST (two rings of 18 spears in all directions at 0.75 s and 1.3 s, the second ring offset by half a gap; the gold glow is the tell) -> DEATH BEAM -> repeat. Constants at the top of Boss class.
- Spears: the boss throws them with ai[0] = 1, which gives them stronger homing than the knight's (pi/130 rad = 1.4 deg per tick for 2.5 s, versus 1.8 deg per tick for 1.5 s; constants BossTurnPerTick and BossHomingTicks in HeavenlySpear.cs) and a longer life (5.5 s). The knight's spears behave exactly as before.
- Death beam: Projectiles/GoldenDeathray.cs, drawn in code (no art file): 1.25 s thin line that follows the player (no damage), then locks and a huge gold beam (130 px wide glow, 70 px hit width, white-hot core) sweeps 2.5 s at about 29 deg/s in a random direction. Damage 700 base (about 3 hits kill an unbuffed player) plus Hellfire for 6 s. She holds still while it fires. Constants at the top of the file.
- No other enemies: the spawner already paused while she lives; now she also removes any knight or dragon that is still around (they vanish without drops).
- SINGLEPLAYER-ONLY note unchanged: the beam angle is synced every tick by the owner machine, which should work in multiplayer, but that is untested.
- To tune: beam sweep speed and length, beam damage, ring size, homing, cycle order and rests.

### Boss sprite (2026-10-24, owner's design; BUILT, NOT YET SEEN IN-GAME)
- The owner sent the boss sheet (DesignRefs/boss_design.jpg). tools/make_boss_sheet.py cuts the boss out, shrinks her to 296 px wide with a small 56-colour palette (Terraria look), and animates it: 8 frames, wings beat, hair and robe sway, body breathes. Frame 296x220 (the placeholder was 160x160, so she is much bigger). 6 ticks per frame.
- Hitbox is her body only, 110x170; the picture is centred on it, wings are scenery. She faces the front, so the picture is never flipped. Hover height raised to 300 px to suit the size. The beam and spears still start at her centre (chest).
- The same script also cuts out the Trophy, Treasure Bag, Celestial Spear and Gold Crystal Star into DesignRefs/ (not used yet). The owner says the stars are for the NEXT attack. Owner answered: yes, it replaces the spear art (done, see below).
- Removed tools/make_boss_placeholder.py (the old placeholder sprite).
- To tune if it looks off: WIDTH, COLOURS, flap strength in make_frame (tools/make_boss_sheet.py); hitbox and TicksPerFrame in the boss class.

### Celestial Spear, enemy clearing and Empress-style movement (2026-10-24; BUILT, NOT YET TESTED)
Owner: the Celestial Spear replaces the boss's spear art; the Heavens enemies must disappear when the summon sequence starts; she should move fast but in an organized way, like the Empress of Light.
- Projectiles/CelestialSpear.cs (art Projectiles/CelestialSpear.png from the owner's design, 42x104, made by tools/make_celestial_spear.py). It inherits HeavenlySpear (same flight and hit, the boss's stronger homing, longer hit line). All three boss spear attacks now throw it. The knight keeps the Heavenly Spear.
- Enemies: Common/HeavensEnemyCleaner.cs removes every knight and dragon (gold dust, no drops). It runs when the Basket becomes full (summon start), every 10 ticks during the 30 s sequence, and every 10 ticks while the boss lives. The spawner is paused for the whole sequence and fight (HeavensEnemyCleaner.FightRunning). If the sequence is cancelled, spawning resumes.
- Movement (NPCs/Bosses/MotherOfTerrariaBoss.cs, Stations, MaxSpeed, DashEase): the old slow sway is gone. She keeps a list of five stations around the player (high left, high right, far above, low right, low left; offsets from the player, so she follows him) and dashes to the next one at the start of every attack (top speed 26 px/tick, eases in), with a gentle bob and a fading golden after-image trail while dashing. During the burst she also flips to the mirror-image side between the two rings. The beam waits 0.5 s (BeamStart) so she is in place, and she holds still while it fires. Five stations versus three attacks means every attack is thrown from every station over time.
- To tune: station offsets, MaxSpeed, DashEase, trail strength, the BeamStart wait.

### Beam retune (2026-10-24, owner: thinner, slower, not one hit but punishing; NOT YET TESTED)
- Hit width 70 -> 28 px, drawn glow 130 -> 56 px, sweep 29 -> 14 degrees a second, damage 700 -> 350 base (no longer a near one-hit). It still burns (Hellfire 6 s) and a beam you stay in keeps hitting, so it stays punishing. Constants: GoldenDeathray.cs (SweepPerTick, BeamHitWidth, BeamDrawWidth) and LaserDamage in the boss class.

### Beam fix (2026-10-24, owner: dashes good; beam too wide and not straight; NOT YET TESTED)
- Cause: the beam was drawn by stretching the game's "magic pixel" texture as if it were 1x1, but that texture is bigger, so the beam was far wider than its numbers and its rotation origin was off (crooked). Now only the 1x1 corner is drawn (GoldenDeathray.Draw), so the drawn width is real (56 px glow, 16 px core) and it is a straight line from her centre. Sparks stay on the line. The hit width (28 px) was already a straight line.

### New attack: Falling Stars (2026-10-24; BUILT, NOT YET TESTED)
Owner picked it from the idea list (next: Feather Storm, Halo Rings, then stage 2 ideas: Star Constellation, Judgment Mark, Protective Barrier; the owner said "i like those"). The Empress comparison: the owner wants attacks that do not feel like the Empress of Light's.
- Fourth attack in the cycle (fan, burst, beam, STARS, repeat; five stations vs four attacks still never lock an attack to one station). In the boss class: StarDamage 320, StarColumns 7, StarSpacing 190, StarHeight 520, StarHoverTicks 45, StarGap 12.
- She gathers light for 0.5 s, then seven Gold Crystal Stars appear in a row 520 px above the player, spaced 190 px apart. Each pulses and shows a thin golden line down to where it will land (the tell). After 0.75 s they fall one after another, 0.2 s apart, in a sweep (randomly left to right or right to left); then an eighth star that FOLLOWED the player sideways locks 25 ticks before it drops.
- A landing star explodes and leaves a Starfire patch (104x44 px, drawn in code, burns 5 s, hurts 40% of the star's damage, keeps hitting). The patches shrink the safe ground over the attack. A star that hits the player does not leave a patch.
- Code: Projectiles/GoldCrystalStar.cs (art Projectiles/GoldCrystalStar.png, owner's design, made by tools/make_gold_star.py), Projectiles/StarfirePatch.cs (no art file).
- A star spawned inside an island falls through it and only starts colliding in the open. The warning line stops at the first solid tile or platform it finds (about 70 tiles max).
- SINGLEPLAYER-ONLY: the patch is spawned by the owner machine; the tracking star's movement and the timers use local counters.
- To tune: counts, spacing, hover and gap timing, fall speed (GoldCrystalStar.cs), patch size, life and damage (StarfirePatch.cs).

### Moon-Lord size and calm movement (2026-10-24, owner: as big as Moon Lord, not so much movement, like Calamity's final boss, occasional dashes; BUILT, NOT YET TESTED)
- Size: the sheet is now enlarged 4x with crisp pixels (tools/make_boss_sheet.py: WIDTH 320, UPSCALE 4): frames are 1120x840 (about 70 x 52 tiles), the sheet 1120x6720. Hitbox 340x560 (her body; the wings are scenery). If it is not big enough or too big, change UPSCALE and the hitbox in SetDefaults.
- Movement: she is now a heavy hovering presence. Five stations, farther out (straight above at -560, high left/right at +-700,-430, above-left/right at +-380,-600). Between attacks she DRIFTS at 8 px/tick, and only every third attack (StationIndex divisible by 3) makes a fast DASH (24 px/tick) with the golden after-image trail. The slow bob is bigger. Removed the burst's mid-attack mirror dash. Constants: Stations, DriftSpeed, DashSpeed, DashEase, DashTrailSpeed.
- Not changed: the attacks themselves. The stars still hang 520 px above the player so they stay on screen; the beam and spears still start at her centre.
- Texture note: the sheet is large (about 30 MB in memory); fine on a normal PC, but remember it for the performance check.

### Smaller (fits the screen) and appears above the Basket (2026-10-24; BUILT, NOT YET TESTED)
- Owner: 4x was too big, did not fit the screen. Now UPSCALE 3: frames 840x630 (about 52 x 39 tiles), sheet 840x5040; hitbox 260x420; stations pulled in about 20% (straight above -460, high left/right +-560,-350, above-left/right +-300,-500). Tuning: UPSCALE in tools/make_boss_sheet.py, hitbox in SetDefaults, Stations.
- She now spawns 450 px above the Heavenly Basket (found by scanning the tiles around the player: HeavenlyBasketPlayer.FindBasketTop; falls back to the player) and flies to her stations from there.

### Less movement (2026-10-24, owner: size is fine, she moves too much; NOT YET TESTED)
- She now changes station only every 3rd attack (StationEvery), so she hangs in place for a whole fan-burst-beam run; one station change in three is the fast dash, the others a drift (7 px/tick). While she is within 110 px of her station she stays still even if the player moves a little (StayWithin), and the bob is smaller and slower. If it is still too much, raise StationEvery or StayWithin; if too still, lower them.

### Smaller again (2026-10-24, owner: still out of the screen; NOT YET TESTED)
- UPSCALE 3 -> 2: frames 560x420 (about 35 x 26 tiles), sheet 560x3360, hitbox 175x280 (two thirds of before). Stations pulled in to two thirds (straight above -310, high left/right +-375,-235, above-left/right +-200,-335), dead zone 80 px, spawn 330 px above the Basket. If it is still too big or now too small, change UPSCALE in tools/make_boss_sheet.py (and the hitbox, stations); for a size between 2 and 3, change WIDTH too (240 with UPSCALE 3 is about 630 wide).

### Heavens now saves (2026-10-24, owner: things built in the Heavens were gone after leaving and returning; NOT YET TESTED)
- Cause: HeavensSubworld.ShouldSave was false by my earlier design (regenerate on every visit). Now true: the Heavens is generated on the first visit and saved with the world (SubworldLibrary writes its own file next to the world file).
- CONSEQUENCE FOR TESTING: a world that already has a saved Heavens will not pick up later terrain-code changes. Use a new throwaway world, or delete that world's saved Heavens file (it sits beside the main .wld file, named after the subworld), to see a new layout. A world that visited the Heavens BEFORE this change has no saved copy, so its first visit after the change generates the current layout and saves it.
- Not done on purpose: saving mid-fight (the boss and the Basket's fight state are not saved), and no reset button for the Heavens. Ask the owner if one is wanted.
- Things to confirm: build in the Heavens, leave, return: it is still there; the Basket still holds its crystals; the enemies still spawn as normal.

### 2026-10-24 (later): smaller boss, land removed, Feather Storm (BUILT, NOT YET TESTED)
Owner: saving the Heavens works ("good job"); make the boss smaller; continue with attacks; remove the piece of land above the Basket.
- Smaller boss: WIDTH 320 -> 240 in tools/make_boss_sheet.py (UPSCALE still 2): frames 420x332 (about 26 x 21 tiles), hitbox 130x210, stations pulled in to 75% (straight above -235, high left/right +-280,-175, above-left/right +-150,-250), dead zone 60 px, spawn 250 px above the Basket.
- Land above the Basket: I took it to be the small floating island at x 330, y 150 (the "Floaters" entry new(330,150,22,8,2)); removed from HeavensTerrainPass (a comment marks the spot). If the owner meant a different piece (the floating rock at x 375, y 190, or the one at x 300, y 88), say so. Because the Heavens is saved now, an already-saved Heavens would keep the island, so Systems/HeavensLegacyCleanup.cs clears its rectangle (tiles x 300-360, y 105-175) ONCE per saved Heavens (a flag saved with it); new Heavens worlds never have it.
- NEW ATTACK 5: FEATHER STORM (boss class: FeatherDamage 180, FeatherTell 40, FeatherEmits 70, FeatherEvery 4, FeatherArms 3, FeatherTurn 0.04). The wings beat twice as fast for the tell, with gold dust at the wing tips; then every 4 ticks she throws one feather per arm (3 arms, 120 degrees apart) and the whole spiral turns slowly one way for the first half and back the other way for the second half. Feathers (Projectiles/GoldenFeather.cs, placeholder art Projectiles/GoldenFeather.png made by tools/make_feather.py) start slow (5) and speed up to 11, pass through walls, last 7 s. The cycle is now fan, burst, beam, stars, feathers.
- Next planned (owner liked them): Halo Rings, then stage 2 (Star Constellation, Judgment Mark, Protective Barrier; Healing Orbs and Guardians optional). The stage 2 trigger (suggested: 50% health) still needs the owner's OK.

### New attack: Wing Wall (2026-10-24; BUILT, NOT YET TESTED)
Owner: "lets do the wing one" (idea 5 of the list: wings spread, a golden wall of light sweeps in with a single gap).
- Sixth attack in the cycle: fan, burst, beam, stars, feathers, WALL. Boss class: WallDamage 260, WallTell 45, WallFirstDelay 50, WallSecondDelay 150, WallStartDistance 1000, WallGapRange 250, WallGapShift 240.
- Tell: she holds her wings spread (animation frame 2, WingsSpreadFrame) with gold dust at the wing tips for 0.75 s. Then two walls appear at the edge of the screen (1000 px to the side of the player, same side for both). Each wall is faint, with the gap marked by bright caps, for 50 ticks (first) or 150 ticks (second), then turns solid and sweeps across at 12 px/tick (about 3.2 s to cross 2300 px). The first gap is near the player's height (within +-250 px), the second is 240-330 px higher or lower, so the player has to change height between the walls. The gap is 230 px tall.
- Why same side: walls from opposite sides would cross in the middle with gaps at different heights, which can be impossible to survive.
- Code: Projectiles/LightWall.cs (drawn in code, no art file). Parameters in ai[]: direction, gap height, delay.
- To tune: gap size (HalfGap in LightWall.cs), speed, delays, damage.

### Star art and a faster boss (2026-10-24, owner: the stars she shoots look ugly and are not my designs; make her faster, she is vulnerable after her attacks; NOT YET TESTED)
- Stars: the earlier sprite shrank the owner's star WITH its blue trail to 51x72 px and 32 colours, so it looked muddy. Now tools/make_gold_star.py crops to the star only (the trail removed, including leftover blue bits), 81x83 px, 64 colours, so the gold shading and blue crystal survive; the comet trail is sparks drawn in code. Hitbox 52 px (was 34). I read "the stars thing" as the Falling Stars (made from the owner's Gold Crystal Star). The FEATHERS are my own placeholder drawing (the owner's sheet has no feather), so they may be what looked ugly: ask the owner; if so, a feather cut from the wing art or a real design is the fix.
- Faster: the rests after attacks are cut. Fan ends 55 (was 90), burst 100 (was 130), beam 15 after (was 50) and it starts after 20 ticks (was 30), stars end 25 after the last drop (was 120), feathers 25 after the last throw (was 110), wall 30 after the second wall starts moving (was 40 after it finished crossing). She moves on while the last projectiles are still flying. Tells are unchanged, so every attack stays readable. The whole cycle is roughly 40% shorter. If it is still too slow, shorten the tells and rests (constants at the top of the boss class); if too hectic, add a few ticks back.

### Feather redraw, Wing Wall replaced by Wing Beams, no beams except her own (2026-10-24; BUILT, NOT YET TESTED)
Owner: "the stars werent the problem, those shards she shoots out were" (the FEATHERS, my placeholder, 16x44 flat gold leaves) and "what are these free beams, she only shoots beams out of herself not any other way".
- Feathers: redrawn by tools/make_feather.py, 22x70, in the colours of the owner's wings (blue tip, gold base, dark indigo outline, pale quill, sparkle). STILL a placeholder (the design sheet has no loose feather); the owner may send a real design. Hit line lengthened to match.
- RULE (owner, 2026-10-24): beams come only OUT OF HER (her body or wing tips), never from elsewhere on the screen. The things that broke it, now changed:
  - Wing Wall (walls of light sweeping in from the screen edges) is REMOVED (Projectiles/LightWall.cs deleted). Replaced by WING BEAMS (sixth attack): she spreads her wings, gold dust streams off the wing tips, then a beam fires out of EACH WING TIP (Projectiles/GoldenDeathray.cs: WingBeamLeft and WingBeamRight, the same beam with another start point and aim). Each is aimed 23 degrees to one side of the player, so the player stands in the gap between them; both sweep the same way (random), so the gap slides sideways and the player follows it. Same thin beam and tell as the Death Beam, 280 base damage each. She holds still while they fire.
  - Falling Stars: the thin golden line from each star down to the ground is REMOVED; the landing spot is now marked by a soft golden glow on the ground (brightening as the drop nears). The Starfire patches (flames on the ground) are unchanged: they are fire, not beams. Say so if those should go too.
- Wing Wall constants and code are gone from the boss class. Keep in mind: all the owner-approved effects are listed above in this file.

### Feather: the owner's design (2026-10-24; BUILT, NOT YET TESTED)
- The owner sent a feather sheet (DesignRefs/feather_design.jpg: a big "2D main projectile" feather, an individual feather with a trail, a three-feather "spread shot / barrage", and an "impact effect"). The main feather is now the Feather Storm projectile: tools/make_feather.py cuts it out by colour (no glow, no sparkles), turns it to point up, shrinks it to 25x80 with 40 colours (Terraria look). The earlier placeholder is gone. Hit line lengthened to match (+-30 px).
- NOT used yet: the spread-shot picture and the impact burst (a possible hit effect when a feather ends; for now it just fades). Ask the owner if they want them.

### Star landing marks restored (2026-10-24, owner: "the marks for the landing stars were good, those can stay")
- The thin golden line from each hovering star down to where it will land is BACK exactly as it was (I had replaced it with a ground glow, which is removed again). OWNER DECISION: keep these marks. The "no beams except out of her" rule therefore does not apply to these guide lines; it still applies to the attacks themselves (the Wing Wall stays removed).

### Falling Stars from the top of the map (2026-10-24, owner: so they cannot be dodged by going above them; NOT YET TESTED)
- The stars now spawn at the very top of the map (y = 100 px, StarTopY in the boss class) instead of 520 px above the player, so a player who flies high is still in their path. The thin tell lines now run from the top of the screen down to the landing spot. Fall acceleration 0.9 -> 1.5 and top speed 22 -> 34 (GoldCrystalStar.cs) so a fall of about 3700 px takes about 2 s instead of dragging on. Things to watch: stars hover offscreen so only the lines are visible; an island above a player still shelters them (a star lands on the first solid thing, as before); the tracking star now has a long fall, so the player can still step aside after it locks (lock is 25 ticks before the drop; shorten the fall or raise LockBeforeFall if it feels too easy).

### Boss health bar (2026-10-24, owner: there is no boss health bar like the other bosses; NOT YET TESTED)
- Likely cause: tModLoader's standard boss bar needs the boss to have a boss head icon, and she had none (NPC.boss = true alone is not enough). Added NPCs/Bosses/MotherOfTerrariaBoss_Head_Boss.png (48x48, her face, crown and halo, cut from the first sprite frame by tools/make_boss_sheet.py; tModLoader loads files named <NPC>_Head_Boss.png on its own). That gives her the standard bar with her icon, and also shows her icon on the map. If the bar is STILL missing in-game, the next step is a custom ModBossBar class; check the client.log for errors.
- The icon is a placeholder crop; the owner may want a hand-made icon.

### Falling Stars doubled (2026-10-24, owner: "have more stars, double them"; NOT YET TESTED)
- Now TWO rows of 7 stars (14, plus the tracking star): the first row as before, then after a 25 tick pause a second row shifted half a gap (95 px, to a random side) so it lands in the spaces, sweeping the OPPOSITE way. Constants: StarRowPause 25 in the boss class (StarColumns 7, StarSpacing 190, StarGap 12 unchanged). The tracking star drops last. The attack lasts about 2 s longer.
- To keep the ground from burning all attack, the patches are a little narrower (104 -> 96 px) and burn 4 s instead of 5 (StarfirePatch.cs). With two rows the second row's patches land between the first row's, so the whole floor is on fire for a moment: players can fly above the flames (the stars still come from the top of the map). If it is too much, raise StarSpacing, drop the second row to fewer stars, or shorten the patch life further.

### Star marks made visible (2026-10-24, owner: "i dont see them make them more visible"; NOT YET TESTED)
- BUG FOUND: the line scan looked only 70 tiles down from the star, but the stars now hang at the top of the map and the islands are 200+ tiles lower, so the line ended far above the screen. It now scans to the bottom of the world (every 6 ticks), and only the part on the screen is drawn.
- More visible: drawn with ordinary blending (the bright Heavens sky hid the old additive glow) as a dark indigo edge (12 px), a gold body (8 px) and a white core (3 px), pulsing and brightening as the drop nears; plus a big glowing gold mark on the ground at the landing spot (it was removed earlier, now back as an addition, the owner liked "the marks"). Everything is in GoldCrystalStar.PreDraw. If it is too much or still too faint, change the widths and alphas there.

### Faster cycle (2026-10-24, owner: she is too easy, make her faster, her attacks should cycle faster; NOT YET TESTED)
Every attack is shorter and the rests are almost gone (tell = the visible warning, kept readable but quicker):
- Spear fan: tell 30 -> 20, ends 55 -> 38. Burst: tell 45 -> 32, second ring 80 -> 56, ends 100 -> 70. Spears 9 -> 11 px/tick, ring spears 7 -> 8.5.
- Beam (and the Wing Beams): tell 75 -> 55 ticks, firing 150 -> 105, starts after 12 ticks, ends 6 after; the tell line follows faster (0.045), the sweep is a little faster (about 19 deg/s) so the angle covered stays similar.
- Falling Stars: gathers 30 -> 20, first star hangs 45 -> 36 ticks, 12 -> 9 ticks between drops, 25 -> 15 between rows, tracker 40 -> 30 after the rows.
- Feather Storm: tell 40 -> 25, one throw every 3 ticks (was 4), the spiral still turns at the same speed, feathers start at 6.5 and top out at 13 (was 5 and 11); ends 12 after the last throw.
- Wing Beams: spreads 45 -> 32 ticks before firing.
- Movement: she changes station every 2nd attack (was 3), drifts at 9 (was 7), dashes at 28 (was 24).
- Overall (attack durations added up, not counting her moving between stations), a full cycle of the six attacks went from about 23 s to about 16 s (29% shorter). If it is still too easy, the next levers are damage, more simultaneous attacks (two at once), a random attack order, or the second stage.

### Falling Stars back to the old version (2026-10-24, owner: "go back to the old falling star thing, this is too hard"; NOT YET TESTED)
- Back to ONE row of 7 stars plus the tracking star (the boss class constant StarRows = 1; set it to 2 to get the doubled version again, the code for it is still there). The star timings are back to the original (gather 30, hang 45, 12 ticks between drops, tracker 40 after the row, ends 25 after) and the burning patches are back to 104 px wide and 5 s.
- KEPT because the owner asked for them after the original: the stars fall from the very top of the map, and the line and ground marks are the thick visible ones. The faster fall speed also stays (it is needed for the long fall).
- I read "the old falling star thing" as the single row. If the owner meant going back further (stars 520 px above the player, the thin faint marks, the slow fall), say so.

### Falling stars slower (2026-10-24, owner: keep falling from the top of the map, reduce the falling speed; NOT YET TESTED)
- Top-of-map spawn kept. Fall acceleration 1.5 -> 1.0 and top speed 34 -> 24 px/tick (GoldCrystalStar.cs: FallAcceleration, MaxFallSpeed), so the fall from the top to the islands takes about 2.5 s (was about 2 s). The first version was 0.9 and 22. Tune those two numbers if needed.

### Beams easier to dodge again (2026-10-24, owner: the beams became impossible to dodge, there used to be a mark that followed you; NOT YET TESTED)
- Cause: in the "faster cycle" change I shortened the beam tell (75 -> 55 ticks), made the tell line follow faster (0.035 -> 0.045 rad/tick), and made the sweep faster, so the line stayed glued to the player right until it fired. Put back: tell 75, firing 150, follow speed 0.035, sweep 0.004 rad/tick (14 deg/s); Death Beam starts after 20 ticks, Wing Beams after 45, rests 15 (all as they were before the speed-up). This applies to the Wing Beams too (same projectile).
- NEW: the tell line now FREEZES for the last 12 ticks before the beam fires (AimFreezeTicks in GoldenDeathray.cs), so the player can step off the marked line. The follow-then-freeze is the readable version of "the mark that follows you". If it is still too hard, raise AimFreezeTicks or lower TrackTurnPerTick; if too easy, the reverse.
- The faster cycle is therefore a bit slower again in the beam attacks (a cycle is about 18.5 s of attack time). The other attacks keep their faster timings.

### Stars back on the screen (2026-10-24, owner: "the stars are still too high up, they used to be in your screen when they started falling, roll back to that"; NOT YET TESTED)
- ROLLED BACK: the stars hang 520 px above the player again (StarHeight in the boss class, never above row 200 px), so they are on the screen when they start to fall, and the fall speed is the original (acceleration 0.9, top speed 22). This undoes the "from the top of the map" change; flying above the stars dodges them again (the owner's decision). Kept: single row, the thick visible marks (the line scan to the bottom of the world is harmless).
- If flying above them has to be stopped again later, an option that keeps them on screen: make the star keep falling from its hover point but ALSO have it hit anything it crosses on the way (it already does), or make a second set come from below. Ask before building.

### Rolled back the "too easy" speed-up (2026-10-24, owner: everything became much faster, the spears are impossible, it is just hard and annoying; the owner will make her harder with NEW attacks instead; NOT YET TESTED)
- Every number from the "Faster cycle" change is back to what it was before it: spear fan tell 30 / ends 55, spears 9 px/tick, burst tell 45 / second ring 80 / ends 100, ring spears 7, feather storm tell 40 / one throw every 4 ticks / turn 0.04 / start speed 5 / max 11 / ends 25 after, movement every 3rd attack at 7 (dash 24). The beam timings and the star timings had already been put back (see the entries above). The earlier rest cuts from the "she is vulnerable after her attacks" request (the shorter rests that the owner asked for first) STAY, since the owner asked for those and not the later speed-up.
- The tell line freeze (12 ticks) on the beams stays; the stars still start on the screen, single row.
- DIFFICULTY PLAN (owner): do not make her harder by speeding things up. New attacks and a second stage next. Keep this in mind: more speed made the attacks unreadable, not harder in a good way.

### Health +30%, boss bar fix, Halo Rings (2026-10-24; BUILT, NOT YET TESTED)
- Health: 600000 -> 780000 (+30%, owner request; placeholder, Phase 6 balances).
- BOSS BAR (the head icon alone did not work): tModLoader only loads a boss head icon for a class marked [AutoloadBossHead]; I had not put the marker on the class. Added it to MotherOfTerrariaBoss (it loads NPCs/Bosses/MotherOfTerrariaBoss_Head_Boss.png). tModLoader's own notes say any NPC with a boss head icon shows the standard boss bar automatically. If the bar is STILL missing after this, the next step is a custom ModBossBar class.
- NEW ATTACK 7: HALO RINGS (owner approved the design, gap 45 and damage 280, 2026-10-24): her halo flares gold for 0.7 s (a glow above her head), then 4 rings spread out from her, one every 0.7 s, at 6 px/tick (slow). Each ring is thin (28 px band) and has one 45-degree gap whose edges are marked by brighter bars; the gap is turned 30-90 degrees away from the direction of the player, alternating sides, so the player has to move along the ring. A player half in the gap is not hit. Damage 280. Code: Projectiles/HaloRing.cs (drawn in code, no art file), DoHaloRings in the boss class (HaloDamage, HaloTell, HaloRingsCount 4, HaloEvery 42). It ends 60 ticks after the last ring. The cycle is now: fan, burst, beam, stars, feathers, wing beams, halo rings.
- To tune: ring speed and thickness (HaloRing.cs), gap width (GapWidth), count and spacing (boss class).

### Decision: stage 2 starts at 60% health (owner, 2026-10-24)
- The owner chose 60% (not the 50% I suggested) as the point where stage 2 begins. Nothing is built yet; the design is being discussed (the open questions are listed in the conversation: transition, look, which attacks, combos). Record the owner's answers here and in ROADMAP.md (Phase 6) before building.

### Health +30% again (2026-10-24, owner; the boss bar now shows, confirmed by the owner)
- 780000 -> 1014000 (placeholder). The boss health bar works (the [AutoloadBossHead] fix). Stage 2 at 60% health is now at about 608400 health lost, i.e. when 405600 remain.

### Her health is exactly what the code says (2026-10-24, owner: Journey mode while testing, "just have her actually health"; NOT YET TESTED)
- The bar showed about half of the code's number (the game scales boss health by difficulty, and on Journey by the enemy-difficulty slider). Now MotherOfTerrariaBoss.OnSpawn sets lifeMax and life to the constant BossLife (1,014,000) after the scaling, so the bar shows exactly that, whatever the difficulty or the slider. CHANGE BossLife to change her health. Side effect: the Journey slider no longer changes her health. For multiplayer later (Phase 8), scaling by player count may be wanted back (ApplyDifficultyAndPlayerScaling).
- NOTE: this DOUBLES what the owner saw (about 507k -> 1,014,000). The owner asked for the real number, but if the fight now feels too long, change BossLife (for example 507000 keeps what the owner had been testing against).

### Halo Rings use the owner's ring design (2026-10-25; BUILT, NOT YET TESTED)
- The owner sent the ring sheet (DesignRefs/Gemini_Generated_Image_sm4f30sm4f30sm4f.jpg: Expanding Ring projectile, Ring Burst / Multi-Ring, Magic Spark). The rings were drawn in code from the game's white pixel; now each ring is drawn as short pieces of the owner's ring BAND (dark outline, gold, blue, gold, outline), a 16x28 strip cut from the design by tools/make_halo_ring.py into Projectiles/HaloRing.png. A strip is used instead of one big picture because the ring grows to 1700 px and has a 45 degree gap. The bars next to the gap are drawn 25% thicker with a pale gold bar behind them as the cap marker. Hit width and gap are unchanged (the band is 28 px, the hit band 28 px).
- NOT used from the sheet: the spiked outer ornaments and glow of the big ring, the multi-ring burst, the magic spark (a possible source for the tell glow or a stage 2 attack). Ask the owner.
- To tune: THICKNESS in tools/make_halo_ring.py (and HitHalfThickness in HaloRing.cs); cap look in HaloRing.PreDraw.
- Also 2026-10-25: the owner confirmed the boss bar shows exactly 1,014,000.

### Halo Rings REMOVED (2026-10-25, owner: "super buggy and tacky, I don't like them")
- The attack, Projectiles/HaloRing.cs, its strip texture and tools/make_halo_ring.py are deleted (also the localization entry). The loop is now SIX attacks: fan, burst, beam, stars, feathers, wing beams (AttackCount 6). The owner's ring sheet stays in DesignRefs/ (it was not used elsewhere; the Multi-Ring and Magic Spark pictures are still free for another idea). A NEW attack to take its slot is to be discussed with the owner. The two entries just above about Halo Rings (first build 2026-10-24, owner's art 2026-10-25) are history only.

### Stage change: the Enchanted Crystals (2026-10-25; BUILT, NOT YET TESTED)
Owner's design: the moment she falls to 60% health (CHANGED TO 50% the same day, see the next entry) she stops what she is doing and forms a circle for 3 seconds (the tell); then she releases 4 enchanted crystals (50,000 health each) that follow the player and shoot short beams; the player must destroy them; while any lives she is INVULNERABLE; when the last dies stage 2 begins AT ONCE (the owner first said "then one more Wing Beams attack", then changed it to immediately, so there is no extra attack).
- Boss class (NPCs/Bosses/MotherOfTerrariaBoss.cs): new state Attack.CrystalPhase (6; not part of the loop, AttackCount stays 6). Trigger in AI(): stage 1 and life <= 60% of lifeMax, from any attack; her health is put back to exactly 60% if a big hit overshot, and her own beams (GoldenDeathray and the wing beams) are killed; spears, stars and feathers already in flight finish. She holds still with her wings spread. CrystalTell 180 ticks: gold dust fills in a circle of 260 px radius around her a little more every tick (clockwise from the top), a chime rises, a golden shield glow grows. At tick 180 the four crystals appear on the circle (the diagonals). NPC.dontTakeDamage is true for the whole phase. 10 ticks after the last crystal is gone: a roar, a burst of light, Stage = 2 and the loop restarts at the Spear Fan. Stage is kept in NPC.localAI[0] (SINGLEPLAYER-ONLY: not synced). STAGE 2 ITSELF HAS NO NEW ATTACKS YET: until it is designed the same six attacks continue.
- Crystals (NPCs/Bosses/EnchantedCrystal.cs): 50,000 health (set again in OnSpawn after the difficulty scaling, like the boss), defense 60, no contact damage, knockback-immune, no drops, no bestiary entry. The four sit at the corners of a square (330 px from the player) that turns slowly around the player (0.012 rad/tick), moving at up to 7 px/tick. Each fires a beam 90 ticks after appearing, then every 200 ticks, staggered 50 ticks apart so they never all fire together. They vanish if she does.
- Crystal beam (Projectiles/CrystalBeam.cs): short (480 px), thin (22 px hit), damage 200; tell: a thin line follows the player for 30 ticks and holds still for the last 15 (45-tick tell), then the beam is on for 24 ticks and does not sweep. Drawn from the white pixel like the boss's beam. Note the owner's rule "beams only come out of her": the crystals are her extension, the owner designed this, so it is an intended exception.
- PLACEHOLDER art: the crystal uses the Gold Crystal Star picture (ASSETS.md), with a soft glow. The owner may want a real crystal design.
- Also: there is no health display for the crystals (the boss bar shows her bar at the threshold and she cannot be hurt). If it is hard to tell how many are left or how hurt they are, add a small bar or a count.
- HOW TO TEST QUICKLY: craft the Debug Stage Tester and hit her once (see the entry above).
- To tune: CrystalTell, CircleRadius, StageTwoAt (boss class); crystal life, orbit radius and spin, fire timing, damage (EnchantedCrystal constants); beam tell, length and width (CrystalBeam constants).

### Stage 2 threshold now 50%, and a debug stage tester (2026-10-25; BUILT, NOT YET TESTED)
- OWNER DECISION: the stage change starts at 50% health (it was 60%; the owner agreed "you were right", my earlier suggestion had been 50%). Constant MotherOfTerrariaBoss.StageTwoAt = 0.5 (now public). She now needs to lose 507,000 health (of 1,014,000) before the circle starts. Older entries below that say 60% are history.
- DEBUG ITEM (TESTING ONLY, REMOVE BEFORE RELEASE, together with the other debug items): Items/Debug/DebugStageTester ("Debug Stage Tester", craft from 1 Dirt + 1 Stone; the icon is a copy of the other debug icons). A 1-damage sword: when it hits her it sets her health to exactly the threshold (507,000), so her own AI starts the stage change on its next tick. It does nothing once the stage change has begun. To be removed when stage 2 has been tested (owner's instruction).

### Crystal layout: 2 beam stars and 2 dashing stars (2026-10-25; BUILT, NOT YET TESTED)
Owner tested the crystals ("great") and set the layout: 2 beam stars, and 2 dashing stars with sharp spikes that spin up when they start to dash. (I read "make some stages" as this layout of the four crystals.)
- Beam stars (crystals 0 and 2): unchanged except the beam is LONGER: 480 -> 900 px (CrystalBeam.BeamLength).
- Dashing stars (crystals 1 and 3, in EnchantedCrystal.cs): a long sharp spike at each of the star's 8 points (4 long, 4 short diagonal), drawn in code (DrawDasherExtras): a golden shaft with a pale highlight and a dark outline, the outer 45% BLOODY dark red. Nothing is drawn for the beam stars. Behaviour: they circle like the others until 130 ticks after appearing (the second dasher is half a cycle, 115 ticks, later), then SPIN UP for 55 ticks (the spin ramps from slow to 0.55 rad/tick, blood dust drips, a thin red line shows the dash direction; the aim follows the player and LOCKS for the last 20 ticks), then DASH 34 ticks at 25 px/tick (about 850 px, through the player's spot and out the other side), then return to circling (fast, 2.3x) and wind down the spin; one dash every 230 ticks each. Contact damage 220 (base) only while dashing, 0 while circling; their hitbox grows from 56 to 110 px during the dash to cover the spikes. They do not fire beams.
- To tune: DashFirst, DashEvery, DashTell, DashAimFreeze, DashTicks, DashSpeed, DashDamage, MaxSpin (EnchantedCrystal.cs); spike size and colours in DrawDasherExtras.
- Art note: still all in code on top of the Gold Crystal Star stand-in picture (see ASSETS.md). A real design for the spiked star can replace it.

### Crystals: no blood, slower dashes, slower minions (2026-10-25, owner; BUILT, NOT YET TESTED)
- Blood removed: the spike tips are now pale gold (no red, and no blood dust: gold dust drips while spinning up; the aim line is pale gold, not red). The earlier "blood-red tips" and "red line" in the entry above are history.
- Dash slower: DashSpeed 25 -> 16 px/tick, DashTicks 34 -> 46 (about 735 px, so it still passes through the player's spot).
- All crystals slower: circling speed 7 -> 5.5, the circle turns 0.012 -> 0.009 rad/tick, the hurry-back after a dash 2.3x -> 2x. Spin-up, timing and damage unchanged.

### Crystals slower again, 30,000 health (2026-10-25, owner: "still pretty hectic"; BUILT, NOT YET TESTED)
- Health 50,000 -> 30,000 each (EnchantedCrystal.CrystalLife; 120,000 for the four). Earlier entries that say 50,000 are history.
- Slower: circling speed 5.5 -> 4, the circle turns 0.009 -> 0.006 rad/tick, dash speed 16 -> 11 px/tick for 60 ticks (about 660 px), and fewer attacks: a beam star fires every 260 ticks (was 200), a dasher dashes every 300 ticks (was 230). Spin-up tell (55 ticks), aim lock and damage unchanged.

### Crystals a bit harder again, and the disappearing beams fixed (2026-10-25, owner: "now too easy"; BUILT, NOT YET TESTED)
- Crystal health back to 50,000 each. Between the two last settings: circling speed 4 -> 5, the circle turns 0.0075 rad/tick, dash speed 11 -> 14 px/tick for 50 ticks (about 700 px), a beam star fires every 230 ticks, a dasher dashes every 265 ticks.
- BUG: the boss's beams (and the wing beams) sometimes vanished from the screen (but may still have hurt). LIKELY CAUSE (not seen directly, found by reasoning): Terraria only draws a projectile when its position is within about 360 px of the screen (ProjectileID.Sets.DrawScreenCheckFluff, default 360); a beam's position is at the boss or a wing tip, but the beam is 2600 px long, so whenever its start was off the screen the beam was not drawn. FIX: GoldenDeathray (and so WingBeamLeft/Right) sets DrawScreenCheckFluff to 3000, CrystalBeam to 1500. If beams still vanish, ask the owner WHEN (during the tell, while firing, after the boss dashes, at the screen edge, when she moves), then look at the beam's end rules (owner dead, Strength fade).

## Not yet tested in-game (consolidated 2026-10-24; the per-change test notes are in the dated sections above)
The owner tested and approved: the summon sequence and everything older, the dashes, her size (the second shrink was good, a later 25% shrink needs a look), the Celestial Spear, the feather, the wing and halo ideas as designs, the star marks, the boss bar (works after [AutoloadBossHead]).
STILL TO CHECK IN-GAME:
- The dashing stars (spikes, spin-up tell, dash, damage) and the longer crystal beams.
- The stage change at 50% (use the Debug Stage Tester sword; circle tell, 4 crystals, her invulnerability, the crystal beams, the switch to stage 2) and the removal of the Halo Rings.
- Her health: CONFIRMED by the owner (2026-10-25): the bar shows exactly 1,014,000. If the fight is too long, change BossLife.
- Wing Beams (the gap between the two beams) and the beam aim freeze (12 ticks).
- Falling Stars as they are now (stars 520 px above, single row, original speed, thick marks).
- The pace after the roll-back of the speed-up (spears 9, burst, feathers, movement every 3rd attack).
- Her smaller size (frame 420x332) against the screen and her hitbox (130x210); the Heavens save (blocks stay after leaving; the Basket keeps its crystals); the one-time removal of the floater above the Basket in an already saved Heavens.
- Anything else the owner mentioned in passing; ask at the start of the next session.

## Old test list (all confirmed by the owner on 2026-10-24)
Compiles, but the owner has not explicitly confirmed (test these first next session):
0. PHASE 4: the whole 30 s summon sequence (chat lines at 10/20/25 s, shakes at 15/22/28 s, the placeholder boss message at 30 s, restart by right-clicking the full Basket, cancel by leaving or dying) and the 30-crystal Basket with its growing glow. Also the Basket's size and glow position.
1. Trees standing still in the Heavens, and swaying again in the normal world after leaving (Systems/HeavensWindSystem.cs).
2. The dragon without its roar; the new crystal icon in the inventory and on the ground.
3. Crystal drop rate feel (1 crystal at 50%, Basket needs 30, so about 60 kills): tune if it feels too grindy or too easy.
Confirmed by the owner this session: enemy tuning, knockback, spear homing, fall death, the atmosphere and island details, the music.
Remember: tModLoader must be closed for command-line builds; the in-game Build + Reload is the normal path.

## Next
0. WAITING ON THE OWNER (2026-10-24, end of day): they will send (a) the stage 2 design (the second look) and (b) the boss music; the remaining stage 2 questions (transition, which new attacks, whether old attacks change, order) are to be discussed on the next session. When the music arrives: convert it like the Heavens theme (ffmpeg to Assets/Music/MotherOfTerraria.ogg, check loudness against Heavens.ogg at about -16 LUFS), point MotherOfTerrariaBoss.Music at it with MusicLoader.GetMusicSlot(Mod, "Assets/Music/..."), keep SceneEffectPriority.BossMedium, and ask whether stage 2 gets its own track. When the design arrives: make a second 8-frame sheet with a copy of tools/make_boss_sheet.py (same frame size, 420x332, so the hitbox and the attack origins stay valid), and add the swap at the stage change.
1. Stage 2 of the boss (starts at 50% health): get the owner's answers to the five questions in "Current state of the boss", write the design into ROADMAP.md Phase 6 (and BOSS_DESIGN.md if the owner wants one; Opus is recommended for the design doc), then build the transition, the new attacks and the stage 2 look.
2. Boss extras that need art or design: treasure bag, trophy, relic, real drops, her own music, a stage 2 sprite, a hand-made head icon. The owner's sheet has a Trophy and Treasure Bag design (DesignRefs/boss_trophy.png, boss_bag.png).
3. Later: balance pass for the whole fight (Phase 6), real art swaps (Phase 7), the multiplayer upgrade, the performance check (her sheet is large) and removal of the debug items (Phase 8).
The list below is the leftover 2C wish list, optional and can be picked up any time:
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
- Boss decisions (owner, 2026-10-24): her health is exactly what the code says (BossLife; Journey scaling is overridden); stage 2 starts at 50% health (was 60%); make her harder only with new attacks and a stage, not with speed; beams come only out of her (the star guide lines stay); the Celestial Spear replaces the spear art; size 420x332 frames; she moves only every 3rd attack; the Heavens is saved with the world; the floater above the Basket is gone.
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
- Boss: music is vanilla Boss2 (placeholder); loot is placeholder Lunar Bars; her damage and health numbers are placeholders for Phase 6; the boss spawn, the Basket hook-up, the clean-up system and the star/ring timers are SINGLEPLAYER-ONLY (see `SINGLEPLAYER-ONLY` comments).
- Boss: the Heavens is saved now, so a world that already has a saved Heavens does not pick up later terrain-code changes (use a new world or delete the saved Heavens file).
- Boss: the sprite is one picture warped into 8 frames (not hand-drawn); the head icon, feather, star, spear are the owner's designs processed by scripts in tools/; the beams, rings, patches and sparks are drawn in code.

## Repo and GitHub
- GitHub repo (public): https://github.com/Unix-H/mother-of-terraria-mod (main + tags phase-0, phase-1, pushed 2026-10-11). Remote uses SSH (key ~/.ssh/id_ed25519_github). Pushed again on 2026-10-14 at the owner's request (main up to baef8fc: Phases 2C, 3 and 4 work). Pushed again on 2026-10-24 at the owner's request (everything up to the end of session 3: the boss and its first stage). Never push without the owner saying so.
- Open decision for the owner: scrub ROADMAP.md from the GitHub history, or make the repo private (the repo is public and an early commit contained ROADMAP.md before it was git-ignored).
