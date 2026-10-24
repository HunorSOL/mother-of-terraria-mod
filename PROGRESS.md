# Progress

Last updated: 2026-10-24 (Phase 5 skeleton plus first-stage attack cycle).
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

## Not yet tested in-game
0q. The feather art in the storm.
0p. Feather look; Wing Beams (gap readable, follow it fair); star landing glow.
0o. The new star art; the shorter rests (does it feel fast but fair?).
0n. Wing Wall: readable gaps, enough time to change height, fair damage.
0m. The smaller boss; the island above the Basket is gone (try both a NEW world and the already-saved one); Feather Storm: readable tell, fair gaps, spiral turns back.
0l. Blocks placed in the Heavens survive leaving and returning.
0k. Her size after the second shrink.
0j. She moves much less.
0i. Her size on the screen and the spawn above the Basket.
0h. The new size and hitbox, whether she fits the screen, and the calmer movement with occasional dashes.
0g. Falling Stars: readable tell, fair gaps between the patches, star art, the tracking star.
0f. The beam is thin and straight now.
0e. The retuned beam (thin, slow, 350 damage).
0d. Celestial Spear look and hit line, enemies vanishing at the start of the summon, the dashes (speed, readability, after-images).
0c. The boss's real sprite and its animation, and whether the hitbox (110x170) feels right against her size.
0b. The boss's first-stage attacks and the doubled health (the section above).
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
