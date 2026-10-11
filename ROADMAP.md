# Mod Roadmap: Heavenly Teleporter + Mother of Terraria (tModLoader)

> Drop this file into the mod's root folder and tell Claude Code: "Read ROADMAP.md and start Phase 0."
> Work ONE phase at a time. Do not start the next phase until the current phase's "Done when" checks pass in-game and I confirm.

## Project context (for Claude Code)

- **Owner:** a beginner at C# who wants to learn by building. Explain what each new file/class does in 1-2 sentences when you create it. Prefer small, testable steps over big rewrites.
- **Platform:** tModLoader (C#). Before writing code, check the current stable tModLoader version and follow its API (do not rely on old 1.3 / early 1.4 examples). Use the official tModLoader wiki guides and the ExampleMod (github.com/tModLoader/tModLoader, `ExampleMod` folder) as the reference for patterns.
- **Multiplayer:** the owner hosts a dedicated server (Raspberry Pi / Synology). Every feature must work in multiplayer, or be clearly marked as singleplayer-only. Never use client-only state for gameplay logic.
- **Assets:** use a filler/placeholder design for EVERY new item, tile, NPC, boss, projectile, and background. Do not try to design or generate final art. The owner will send the real designs later. Keep placeholders easy to swap (one texture file per asset, named after the class, e.g. `HeavenlyTeleporter.png`, so a real image can simply replace the filler with no code changes). Keep a `ASSETS.md` checklist listing every asset, its filename, size/frame requirements, and whether it is still a placeholder.
- **Style:** one class per file, grouped in folders (Items/, Tiles/, NPCs/Bosses/, Projectiles/, Systems/, Common/).

## The idea

1. After defeating **Moon Lord**, the player can craft the **Celestial Sigil** (vanilla item; verify exact vanilla crafting/availability rules and design around them).
2. Combine **Celestial Sigil + Copper Shortsword** (any prefix) to craft the **Heavenly Teleporter** (name can change).
3. Using the Heavenly Teleporter opens a portal to **the Heavens**: a new, beautiful dimension with trees, golden statues, etc.
4. In the Heavens there is a **pylon-looking structure**. Right-clicking it starts the boss sequence. (A lore video/cutscene is **optional**, see Phase 4.)
5. After the sequence, the **Mother of Terraria** spawns: a **super hard** endgame boss, tougher than Moon Lord, fully custom with a unique design and special attacks. It is meant to be the hardest fight in the mod, a true post-Moon Lord challenge.

## Feasibility summary

| Piece | Verdict | Notes |
|---|---|---|
| Items + recipes | Easy | Standard ModItem / ModRecipe work. |
| Pylon-like interactive object | Easy-Moderate | ModTile with `RightClick` hook. |
| Mother of Terraria boss (phases, attacks, boss bar, music, drops) | Moderate-Hard | The biggest chunk of work. Doable. Making it super hard without feeling cheap takes lots of playtesting. |
| New dimension | Moderate-Hard | Use the **SubworldLibrary** mod (dependency). Fallback: build the Heavens as a structure high in the existing sky and teleport the player there. |
| Lore video | **Optional / risky** | Not required. Terraria runs on FNA; video playback is not a standard tModLoader feature. Skipped by default; the boss spawns right after the pylon intro. Can be revisited as a stretch goal. |
| Custom art/animation | Time-consuming, not technically hard | Boss sprites/animation are the real cost. |

## Which model to use (Claude Code)

- **Default to Sonnet 5.5.** It is cheaper and handles well-scoped tasks (items, recipes, tiles, single attacks, bug fixes) well. Claude Code defaults to Opus 5.5, so switch yourself: `/model sonnet`.
- **Use Opus 5.5 for the hard, open-ended parts:** the real dimension (Phase 2b), boss design and AI structure (Phase 6), and any bug that Sonnet can't solve after a couple of tries. Switch with `/model opus`, then switch back when the hard part is done.
- **Hybrid option:** `/model opusplan` uses Opus in plan mode and Sonnet to carry out the plan. Good for Phases 2b and 6.
- **Before switching to Opus, try raising effort first:** `/effort high` on Sonnet.
- **Fable is not needed** for this project. Only consider `/model fable` if Opus is stuck on something.
- Run `claude update` first; Sonnet 5.5 needs a recent Claude Code version. Check usage with `/usage`.
- Claude Code: when a phase starts, remind me which model the phase recommends, and tell me if I should switch.

## Phase 0: Dev environment and "hello mod"

> **Model: Sonnet 5.5**

- Install the .NET SDK required by the current tModLoader, set up the mod project (tModLoader's "Develop Mods" menu -> create new mod), open it in VS Code or Visual Studio.
- Add one trivial item (e.g. a placeholder sword) that can be spawned and used.
- Add git: `git init`, `.gitignore` (must include `.env` and `.env.*` before the first commit), first commit. Local only; do not push (see hard safety rules).
- **Done when:** the mod builds, loads in tModLoader, and the test item works in a singleplayer world.

## Phase 1: Items and the crafting chain

> **Model: Sonnet 5.5**

- Create the **Heavenly Teleporter** item (placeholder sprite, tooltip, rarity).
- Recipe: Celestial Sigil + Copper Shortsword (any prefix) -> Heavenly Teleporter.
  - Verify how the vanilla Celestial Sigil is obtained and make sure the chain works post-Moon Lord.
- **Done when:** in a test world, the recipe only works with the correct ingredients, and the Teleporter appears in inventory.

## Phase 2: The Heavens (location)

> **Model: Sonnet 5.5 for Step 2a (sky structure). Switch to Opus 5.5 for Step 2b (real subworld via SubworldLibrary), then back to Sonnet.**

- Step 2a (cheap version first): generate a "Heavens" area (platforms/cloud islands, golden statues, trees) as a structure in the existing world's sky. Using the Teleporter teleports the player there.
- Step 2b (real dimension): add SubworldLibrary as a dependency; move the area into a true subworld with its own worldgen, entry via the Teleporter, and a way back.
- Visual identity: custom background/sky tint, ambient music slot, no random vanilla enemy spawns inside.
- **Done when:** the player can enter, explore, and leave safely; works on a dedicated server (test with at least two clients if possible).

## Phase 3: The pylon-like structure

> **Model: Sonnet 5.5**

- Create a custom ModTile (multi-tile object) placed during Heavens worldgen.
- `RightClick` interaction triggers a server-authoritative "start sequence" event (placeholder: chat message).
- **Done when:** right-clicking triggers the placeholder event, and only once per boss attempt (no double-trigger spam).

## Phase 4: Boss summon sequence (video is optional)

> **Model: Sonnet 5.5**

- Default: when the pylon is right-clicked, run a short in-engine intro (e.g. screen shake, sky darkens, a few lines of text, music change), then spawn the boss. Keep it simple and server-authoritative.
- **Stretch goal only:** a lore video or fuller cutscene. If wanted later, run a quick spike first to see whether video playback works inside tModLoader, and report findings before building. Do not block the rest of the roadmap on this.
- **Done when:** interacting with the pylon triggers the intro and fires the boss-spawn event once.

## Phase 5: Mother of Terraria, boss skeleton

> **Model: Sonnet 5.5**

- Create the ModNPC with placeholder sprite: health, defense, boss bar, music slot, immunity to knockback/lava, despawn rules, spawn from the Phase 4 trigger (a debug spawn item is fine for early testing).
- Basic AI: moves toward/around the player, one simple attack.
- Loot: placeholder drop + trophy/relic slots, expert/master loot hooks.
- **Done when:** it spawns after the sequence, can be fought and killed, drops loot, and works in multiplayer.

## Phase 6: Boss design and attacks

> **Model: Opus 5.5 for the boss design doc, phase structure, and AI state machine. Sonnet 5.5 for implementing each individual attack, then Opus 5.5 again for difficulty balancing.**

- Write a short design doc first (`BOSS_DESIGN.md`): theme, phases (suggest 3), each attack with a telegraph, damage, and counterplay.
- Implement attacks one at a time, each as its own method/state in a state machine; test each in isolation before combining.
- Add phase transitions, enrage rules, difficulty scaling for expert/master, and multiplayer scaling.
- Difficulty target: **super hard**, a boss that takes many attempts, well above Moon Lord. Use high health and defense, fast and layered attacks, multi-phase escalation, and attack combinations in later phases.
- Hard does not mean unreadable: keep a visible tell on each attack so deaths come from mistakes. Avoid unavoidable damage and instant-kill gimmicks.
- Add an optional difficulty config (ModConfig) with a "normal super hard" default, so the owner can tune health, damage, and attack speed without editing code.
- **Done when:** the full fight is playable start to finish, and 3+ playtest runs with endgame gear show it is genuinely difficult but beatable.

## Phase 7: Real art, audio, polish

> **Model: Sonnet 5.5**

- The owner supplies the final designs. Replace each placeholder using the `ASSETS.md` checklist, one asset at a time, and tell the owner exactly what to send (dimensions, frame counts for animations, transparent PNG).
- If a real design has different dimensions than the filler, adjust hitboxes, animation frames, and tile sizes in code.
- Add localization strings, tooltips, boss bar icon, music, and a short in-game explanation of the progression chain.
- **Done when:** every row in `ASSETS.md` is marked final, and nothing in the mod still uses a filler.

## Phase 8: Multiplayer test and release

> **Model: Sonnet 5.5, switch to Opus 5.5 only if a multiplayer bug is hard to track down.**

- Test on the actual dedicated server: join/leave, death mid-fight, boss despawn, subworld transitions.
- Package the mod; optionally publish to the Steam Workshop via tModLoader.

## How to test (applies to every phase)

- **Build and load:** in tModLoader, go to Workshop -> Develop Mods -> Build + Reload. Then enable the mod under Manage Mods and restart if prompted.
- **Use a throwaway world:** create a new Journey-mode world for testing so progress and items can't be lost.
- **Skip the grind:** Claude Code should add a debug item to the mod that gives you the Celestial Sigil, Copper Shortsword, or Heavenly Teleporter instantly, so Moon Lord does not need to be beaten every time. Remove it before release.
- **Check the logs:** `client.log` in the tModLoader logs folder shows errors when something fails to load. Paste errors back to Claude Code.
- **Per-phase checks:** each phase's "Done when" is the acceptance test; Claude Code must tell you the exact in-game steps.
- **Real playtest:** do at least one full run through the real progression chain (Moon Lord -> Sigil -> Teleporter -> Heavens -> boss) before release.
- **Multiplayer:** test on a local tModLoader host with a second client first, then on the dedicated server. Players need the same mod version, and the server must have the mod files enabled.

## Ground rules for Claude Code

### Hard safety rules (never break these)

- **NEVER push to GitHub unless I explicitly tell you to.** Local commits are fine. Do not run `git push`, create remotes, open pull requests, or publish releases on your own, and do not ask me to approve a push as a side effect of finishing a phase. Wait for me to say so in plain words.
- **NEVER publish, commit, or upload the `.env` file.** Add `.env` (and any `.env.*`) to `.gitignore` in Phase 0, before the first commit. Never print its contents, paste it into logs, or include it in any package, Workshop upload, or build output. If a secret ever ends up in a tracked file, stop and tell me immediately.
- When in doubt about publishing anything (GitHub, Steam Workshop, releases), ask first.

- Never block progress on art. Use filler designs and move on; list what still needs real art in `ASSETS.md`.
- Before each phase, restate the plan in a few lines and wait for confirmation if the plan changes from this file.
- **At the end of every phase, STOP and tell me clearly:** "Phase X is done, please test it now," followed by the exact in-game steps to test it (what to build/load, which debug items to use, what I should see). Then wait for me to reply with the test results before starting the next phase. Never continue to the next phase on your own.
- If something in this roadmap turns out to be impossible or risky, say so plainly and propose options. Do not quietly cut features.
- Keep `PROGRESS.md` updated with what's done, what's next, and known issues.
