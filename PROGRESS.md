# Progress

Last updated: end of day 2026-10-12. Next session: 2026-10-13.
Read ROADMAP.md (plan and rules), this file, and ASSETS.md (placeholder list) first. Work in the Documents folder, not the old Trash copy.

## Where we are
- Phase 0 COMPLETE (2026-10-11). Phase 1 COMPLETE (2026-10-11).
- Phase 2 (the Heavens subworld) COMPLETE for singleplayer: 2.1 to 2.5 confirmed in-game. 2.6 (multiplayer testing) is DEFERRED to Phase 8 by owner decision (see Decisions).
- Phase 2C (Heavens details and polish) IN PROGRESS: enemies, the island look, trees and statues are built; atmosphere, detail, audio and tuning remain.
- Phase 3 onward (pylon/Heavenly Basket, boss summon, boss) not started. Boss summon design is deferred, to be discussed.

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

## Enemy tuning (2026-10-14, owner approved)
- Knight: health 35000, defense 70, spear damage 140 (contact 150 unchanged). Dragon: health 50000, defense 80 (contact 190, swoop x1.3 unchanged). Aim: 8-15 s to kill at endgame damage; 3 hits kill an unbuffed player. Damage numbers assume Expert/Master (base is scaled up there); owner has not yet said which difficulty they play, so re-check in a playtest. Spawn caps and crystal drops unchanged.

- Fall death (2026-10-14, untested): Common/HeavensFallDeath.cs kills the player who falls below Main.worldSurface (row 330, where the cave background starts) in the Heavens.

- Knockback: knight FULLY immune (knockBackResist 0); dragon takes a little (0.05) (owner, 2026-10-14).

- Crystal drops (2026-10-14, owner): both enemies drop exactly 1 crystal, 50% of the time (was knight 1 in 3 for 1-2, dragon 1 in 2 for 1-3).

## Not yet tested in-game (test these FIRST tomorrow)
Everything below compiles but the owner has not confirmed it after the last build:
1. The trees standing still (Systems/HeavensWindSystem.cs): crowns and branches must not move in the Heavens, and trees in the normal world must still sway after leaving.
2. The real trees, the golden turf and the bridges as a whole after the last rebuild: no crash on load, trees present on every island, crowns line up with trunks. If trees are missing, check client.log for "Heavens trees: X grown of Y tried".
3. Knight health +50% (13500) and the dragon spawn weight change.
Already confirmed by the owner: the statues, the saplings and stumps, the dragon ("absolutely great"), the knight facing and animation, the island look and the golden trees after the rebuild ("good").
Remember: tModLoader must be closed for command-line builds; the in-game Build + Reload is the normal path.

## To finish tomorrow (suggested order)
1. Rebuild and test the list above; fix anything broken.
2. Tune the enemies together: health, damage, spawn rates and caps, crystal drop chances (the owner wants to decide these now that both enemies exist).
3. Atmosphere pass (needs nothing from the owner): drifting clouds, light beams, falling petals or sparkles, a layered parallax sky background, a sun or halo.
4. More island detail: floating rocks, flowers, vines, variety. The floating canopy puff and leafy branch from the owner's tree sheet are saved in DesignRefs (tree_puff.png, tree_branch.png) and could become floating decorations.
5. Ambient creatures (glowing butterflies or birds), non-hostile; and a grand arrival gateway where the player lands.
6. Audio: Heavens music and ambient sounds (needs tracks from the owner; vanilla Space music plays until then).
7. Owner decisions still open: the dragon's debuff (fire does not match its golden-blue look; a holy or light effect may suit it), whether a third enemy type is wanted, the crystal's final design, and whether to send a bark close-up so the trunk art can match their style.
8. Performance check (frame rate, load time), then mark Phase 2C complete and plan Phase 3 (the Heavenly Basket: how many crystals fill it, whether it resets, what happens on defeat, where it stands, its look).
Real art still wanted whenever the owner has it (all currently generated placeholders): turf tile, tree bark, bridge posts, crystal sprite. See ASSETS.md.

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
