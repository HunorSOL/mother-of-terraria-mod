using Microsoft.Xna.Framework;
using MotherOfTerraria.Common;
using MotherOfTerraria.NPCs.Heavens;
using MotherOfTerraria.Systems;
using MotherOfTerraria.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Bosses
{
	// The Mother of Terraria: the mod's final boss. Phase 5 built the skeleton; this is the FIRST STAGE with a looping
	// attack cycle (more attacks and a second stage come later, and the final balance is Phase 6):
	//   1. SPEAR FAN:   a short gold glow, then three slightly homing Celestial Spears aimed at the player.
	//   2. SPEAR BURST: she gathers light, then throws rings of slightly homing spears in ALL directions (two waves,
	//                   the second ring offset so it fills the gaps of the first).
	//   3. DEATH BEAM:  a thin line follows the player (the tell), then locks and a huge golden beam sweeps across.
	//   4. FALLING STARS: seven gold stars hang high above, then drop one by one in a sweep; each leaves a burning patch
	//                  of light on the ground, and one last star follows you.
	//   5. FEATHER STORM: she beats her wings hard (the tell), then three spiral arms of golden feathers turn slowly one way
	//                  and then the other.
	//   6. WING BEAMS:  she spreads her wings wide (the tell), then a golden beam fires out of each wing tip, aimed to either side of you;
	//                  they sweep together and you follow the gap between them.
	//   ...then back to 1.
	//   STAGE CHANGE (owner design, 2026-10-25): the moment she falls to 50% health she drops whatever she is doing, stops, and forms a
	//   golden circle for 3 seconds (the tell), then releases FOUR ENCHANTED CRYSTALS (NPCs/Bosses/EnchantedCrystal.cs, 50000 health each).
	//   While any crystal lives she cannot be hurt. When the last one dies stage 2 begins at once (stage 2's attacks are NOT built yet:
	//   for now the same six attacks continue). Before each attack she moves to the next of five fixed spots around the player (see Stations): usually a slow drift, every third time a fast dash.
	// While she is alive no other enemy may be in the Heavens (the spawner stops, and any that are left are removed).
	// Texture: NPCs/Bosses/MotherOfTerrariaBoss.png (the owner's design, 8 frames of 420x332 made by tools/make_boss_sheet.py, Moon Lord sized: wings flap, hair and robe sway).
	// ALL NUMBERS BELOW ARE PLACEHOLDERS; the real balance is Phase 6.
	// SINGLEPLAYER-ONLY: OnKill talks to the local player's Basket (see HeavenlyBasketPlayer); multiplayer needs the server to do that.
	[AutoloadBossHead] // loads NPCs/Bosses/MotherOfTerrariaBoss_Head_Boss.png; without this the standard boss health bar never appears
	public class MotherOfTerrariaBoss : ModNPC
	{
		// HER HEALTH, exactly (placeholder; Phase 6 balances it): doubled to 600000, +30% to 780000, +30% again to 1014000 by the owner on
		// 2026-10-24. The game scales a boss's health by difficulty and (on Journey) by the enemy-difficulty slider, so the number on the
		// bar was about half of this; OnSpawn below sets it again after the scaling so the bar shows exactly this number (owner request).
		private const int BossLife = 1014000;

		private const int FrameCount = 8;
		private static Asset<Texture2D> stageTwoTexture; // the stage 2 sheet (same frame size as stage 1), loaded the first time it is drawn
		private const int WingsSpreadFrame = 2;   // the animation frame where the wings are raised highest
		private const int TicksPerFrame = 6;        // wing-beat speed: one full flap takes 8 x 6 = 48 ticks

		// MOVEMENT (like the big, calm final bosses of Calamity: she is huge, drifts slowly above the player, and only
		// now and then makes a fast dash). She works through a fixed list of "stations" around the player (offsets in
		// pixels from the player: x to the right, y down). Every third attack (StationEvery) she moves to the next station: most of the time
		// a slow drift, but one move in three (StationIndex divisible by 3) a fast dash with a golden after-image trail.
		private static readonly Vector2[] Stations =
		{
			new Vector2(0f, -235f),      // straight above (a DASH goes here first)
			new Vector2(-280f, -175f),   // high left
			new Vector2(280f, -175f),    // high right
			new Vector2(-150f, -250f),   // above, a little left
			new Vector2(150f, -250f),    // above, a little right
		};
		private const int StationEvery = 3;        // she moves to a new station only every this many attacks (owner: she moved too much)
		private const float StayWithin = 60f;      // while she is this close to her station she stays put, even if the player shifts a little
		private const float DriftSpeed = 7f;        // normal speed between attacks (pixels per tick)
		private const float DashSpeed = 24f;        // speed of the occasional dash
		private const float DashEase = 0.12f;       // slows her down as she arrives (bigger = later braking)
		private const float DashTrailSpeed = 14f;   // above this speed she leaves a golden after-image trail

		private const float SpearSpeed = 9f;
		private const int SpearDamage = 150;
		private const int SpearsInFan = 3;
		private const int StarlightDamage = 200;    // STAGE 2: the spear fan throws Starlight Lances that hit harder than the 150 spears (owner, 2026-10-25: "much more powerful"; 300 was extreme, so 200)
		private const float FanSpread = 0.28f;      // radians between neighbouring spears
		private const int RingSpears = 18;          // spears per ring in the burst
		private const float RingSpearSpeed = 7f;

		private const int BeamStart = 20;           // the beam attack waits this long first so she can reach her station
		private const int LaserDamage = 350;        // punishing but NOT a one-hit kill (was 700): it hurts hard, burns (Hellfire), and a beam you stay in hits again and again

		// Times in ticks (60 per second), counted from the start of each attack.
		private const int FanTell = 30, FanEnd = 55;
		private const int BurstTell = 45, BurstSecondWave = 80, BurstEnd = 100;
		// FALLING STARS: a row of stars hangs high above the arena (each with a line down to where it will land), then they
		// fall one after another in a sweep, and each leaves a burning patch of light on the ground. One extra star follows you.
		private const int StarDamage = 320;
		private const int StarColumns = 7;
		private const float StarSpacing = 190f;     // sideways gap between neighbouring stars
		private const float StarHeight = 520f;      // how far above the player the stars hang: on the screen when they start to fall (owner, 2026-10-24: the top-of-map version was too high)
		private const int StarSpawnAt = 30;         // she gathers light first
		private const int StarHoverTicks = 45;      // how long the first star hangs before it drops (time to read the pattern)
		private const int StarGap = 12;             // ticks between one star's drop and the next
		private const int StarRows = 1;            // 1 = a single row of stars (the original). 2 = a second row in the gaps (tried 2026-10-24, owner said too hard)
		private const int StarRowPause = 25;       // pause between the first row of stars and the second
		private const int StarTrackerExtra = 40;    // the following star drops this long after the last row star
		private const int StarsEnd = StarSpawnAt + StarHoverTicks + (StarRows - 1) * (StarColumns * StarGap + StarRowPause) + StarColumns * StarGap + StarTrackerExtra + 25; // the last star has just dropped; she does not wait for it to land
		// FEATHER STORM: she beats her wings hard (the flap speeds up: the tell), then throws three spiral arms of golden
		// feathers that turn slowly one way and then the other. You dodge by moving with the gaps between the arms.
		private const int FeatherDamage = 180;
		private const int FeatherTell = 40;
		private const int FeatherEmits = 35;        // how many times she throws (one feather per arm each time)
		private const int FeatherEvery = 8;         // ticks between throws
		private const int FeatherArms = 3;
		private const float FeatherTurn = 0.08f;    // radians the spiral turns per throw (about 0.6 radians a second)
		private const float FeatherStartSpeed = 5f;
		private const int FeatherEnd = FeatherTell + FeatherEmits * FeatherEvery + 25;  // she moves on while the last feathers are still flying
		// WING BEAMS: she spreads her wings wide and holds them there (the tell: thin lines from both wing tips follow you),
		// then a golden beam fires OUT OF EACH WING TIP. They are aimed to either side of you (you stand in the gap between
		// them) and sweep the same way, so the gap slides sideways and you have to move with it. Nothing but her own beams.
		private const int WingBeamDamage = 280;
		private const int WingBeamsStart = 45;      // she spreads her wings first; the beams' own tell (thin lines) runs after that
		private const int WingBeamsEnd = WingBeamsStart + GoldenDeathray.TellTicks + GoldenDeathray.BeamTicks + 15;
		private const int LaserEnd = BeamStart + GoldenDeathray.TellTicks + GoldenDeathray.BeamTicks + 15;

		// CROSS BEAMS (stage 2, owner's design 2026-10-25): she stops where she is and cannot be hurt. For 2 seconds she charges (gold light
		// streams into her, a glow grows; after 0.75 s eight thin lines show where the beams will go), then EIGHT beams leave her in a star (up, down, left, right and
		// the four diagonals; first built as a plus of four, which was too easy) and sweep slowly together. Double the damage of the stage 1 death beam. When they end she is
		// vulnerable again and moves on to the next attack.
		private const int CrossDamage = LaserDamage * 2;   // 700 base (the stage 1 death beam is 350)
		private const int CrossArms = 8;                   // owner, 2026-10-25: the plus (4) was too easy to dodge, so a star of 8 beams (the diagonals added)
		private const int CrossCharge = 120;               // 2 seconds from the start to the moment the beams fire
		private const int CrossSpawnAt = CrossCharge - GoldenDeathray.TellTicks;   // the beams (and their tell lines) appear this far in, so they fire at CrossCharge
		private const int CrossEnd = CrossCharge + GoldenDeathray.BeamTicks + 15;

		// MIRROR COURT (stage 2, owner's design 2026-10-26): she gets an invisible SHIELD of MirrorShield health. She vanishes, and four figures appear
		// around the player: the real her and three MirrorClone copies. All four close in slowly. While the shield holds, her real health cannot drop;
		// hits on the real one (clones cannot be hit) wear the shield down. Break it and the clones vanish and the fight goes on as normal; if the
		// clones arrive first they hurt the player. The real one has a faint halo the clones lack. A rare "stall" attack (see NextAttack).
		public const float MirrorSpeed = 1.1f;          // pixels per tick everyone closes in (500 px takes about 7.5 s)
		private const int MirrorShield = 5000;          // owner, 2026-10-26
		private const int MirrorAppear = 60;             // she gathers light this long, then the four figures appear
		private const float MirrorDistance = 500f;      // how far from the player the four start
		private const int MirrorApproach = 540;         // ticks from the appearing until they reach the player
		private const int MirrorLinger = 120;           // how long they stay on the player (hurting) before the attack gives up
		private const int MirrorEnd = MirrorAppear + MirrorApproach + MirrorLinger;
		private const int MirrorRepeatGap = 3;          // Mirror Court can only come up again after this many other attacks
		private const int MirrorMaxUses = 2;            // owner, 2026-10-26: it can happen only this many times in one fight
		private const int OtherAttackWeight = 4;        // owner, 2026-10-26: Mirror Court must be the LEAST likely attack (weight 1 against this for every other)

		// STAR HELL (stage 2, owner's design 2026-10-26): she stops for a second and gathers energy, then sends 20 very strongly homing stars (SeekerStar,
		// 1000 health each, 200 damage on touch, then they explode) in waves. The only real answer is to shoot them down. She waits until they are all gone.
		private const int HellCount = 20;
		private const int HellTell = 60;                // the second of gathering energy
		private const int HellWaves = 5;                // the stars leave in this many waves (4 each) ...
		private const int HellWaveGap = 10;             // ... this many ticks apart, so they do not all arrive in one frame
		private const int HellMin = HellTell + HellWaves * HellWaveGap + 20;  // she never moves on before this
		private const int HellMax = HellTell + HellWaves * HellWaveGap + SeekerStar.SeekerLife + 60; // ... and never waits longer than this

		private const int StageTwoPause = 90;      // ticks she roars and waits (1.5 s) after arriving, before her first stage 2 attack

		// GRAVITY WELL (stage 2, owner's design 2026-10-26): she gathers light for a second, then a dark well opens in the air about 450 px from the player
		// (above or to the side, never below, so the pull cannot drag anyone toward the Heavens' fall death), pulls the player in and drags Starlight
		// Lances in on spirals (see Projectiles/GravityWell.cs). She can be hurt during it. She moves on when the well has collapsed.
		private const int WellTell = 60;
		private const float WellDistance = 450f;
		private const int WellDamage = 400;             // the dark core's touch (the lances do 55% of it)
		private const int WellRepeatGap = 3;           // owner, 2026-10-26: it must be rare: weight 1 in the shuffle (against OtherAttackWeight) and only after this many other attacks
		private const int WellEnd = WellTell + GravityWell.TotalTicks + 15;

		private const int AttackCount = 6;          // the attacks in the loop (CrystalPhase below is not part of it)

		// THE CRYSTAL PHASE (stage change): she is invulnerable from the moment it starts until the last crystal is dead.
		public const float StageTwoAt = 0.5f;        // fraction of her health where it starts (owner: 60% first, changed to 50% on 2026-10-25)
		private const int CrystalTell = 180;         // 3 seconds forming the circle before the crystals appear
		private const float CircleRadius = 260f;     // the circle she draws around herself; the crystals appear on it
		private const int CrystalCount = 4;

		// STAGE 2's attack loop (owner, 2026-10-25): only the two spear attacks are carried over (as Starlight Lances); every other stage 2
		// attack will be completely new, so the old beam, stars, feathers and wing beams are NOT used in stage 2. New stage 2 attacks go here.
		// The order is SHUFFLED (owner, 2026-10-26): each time a random one that is not the current attack (see NextAttack).
		private static readonly Attack[] StageTwoLoop = { Attack.SpearFan, Attack.SpearBurst, Attack.CrossBeams, Attack.MirrorCourt, Attack.StarHell, Attack.GravityWell };

		private enum Attack
		{
			SpearFan = 0,
			SpearBurst = 1,
			DeathBeam = 2,
			FallingStars = 3,
			FeatherStorm = 4,
			WingBeams = 5,
			CrystalPhase = 6,                       // not in the loop: started by her health, ends in stage 2
			CrossBeams = 7,                         // stage 2 only (see StageTwoLoop)
			MirrorCourt = 8,                        // stage 2 only
			StarHell = 9,                           // stage 2 only
			GravityWell = 10,                       // stage 2 only
		}

		// Mirror Court's shield (health left) and her health when it began (restored every tick, so only the shield takes damage).
		// SINGLEPLAYER-ONLY: not synced to clients (the server runs the AI, so it works there; clients only need the picture).
		private int shieldLeft;
		private int lifeAtShield;
		private int attacksSinceMirror = MirrorRepeatGap;
		private int attacksSinceWell = WellRepeatGap;
		private int mirrorUses;

		private Attack CurrentAttack
		{
			get => (Attack)(int)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private ref float Timer => ref NPC.ai[1];
		private ref float StationIndex => ref NPC.ai[2];
		private ref float AttacksSinceMove => ref NPC.ai[3];

		// 1 = stage 1, 2 = stage 2. SINGLEPLAYER-ONLY: localAI is not synced; multiplayer needs this moved to a synced field (Phase 8).
		private int Stage
		{
			get => NPC.localAI[0] > 0f ? 2 : 1;
			set => NPC.localAI[0] = value == 2 ? 1f : 0f;
		}

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = FrameCount;
			NPCID.Sets.TrailCacheLength[Type] = 12; // remembers where she was, for the dash after-images
			NPCID.Sets.TrailingMode[Type] = 0;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Hide = true }; // no bestiary entry until the real design exists
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 130;         // her body (the wings are scenery, not a hitbox); the 420x332 picture is centred on it
			NPC.height = 210;
			NPC.damage = 200;        // contact damage (placeholder)
			NPC.defense = 100;       // placeholder. Owner, 2026-10-26: tried 200 and 120, back to 100
			NPC.lifeMax = BossLife;
			NPC.HitSound = SoundID.NPCHit5;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0f; // immune to knockback
			NPC.value = Item.buyPrice(gold: 50);
			NPC.aiStyle = -1;        // our own AI below
			NPC.boss = true;         // gives her the boss health bar
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.lavaImmune = true;
			NPC.npcSlots = 15f;      // fills the spawn budget like a boss
			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.OnFire] = true;
			NPC.buffImmune[BuffID.Poisoned] = true;

			// Boss music: a vanilla track stands in until the owner has one. A boss priority beats the Heavens
			// theme (priority Event) while she is alive, and the theme returns when she is gone.
			Music = MusicID.Boss2;
			SceneEffectPriority = SceneEffectPriority.BossMedium;
		}

		// While dashing she leaves a fading golden after-image trail, then she is drawn normally.
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			// While she cannot be hurt (the crystal phase) a golden shield glows around her, growing as the circle forms.
			bool hellShield = CurrentAttack == Attack.StarHell && Timer >= HellTell; // invulnerable from the moment she releases the stars
			bool wellShield = CurrentAttack == Attack.GravityWell && Timer >= WellTell; // invulnerable from the moment the well opens
			bool beamShield = BeamsShielded; // stage 1 Death Beam / Wing Beams: shielded while she holds still for the beam
			if ((CurrentAttack == Attack.CrystalPhase || CurrentAttack == Attack.CrossBeams || hellShield || wellShield || beamShield) && HeavensSkyTextures.Ready)
			{
				float grow = hellShield || wellShield || beamShield ? 1f : MathHelper.Clamp(Timer / (CurrentAttack == Attack.CrossBeams ? CrossCharge : CrystalTell), 0.15f, 1f);
				float pulse = 0.85f + 0.15f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
				Texture2D glow = HeavensSkyTextures.Soft;
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				spriteBatch.Draw(glow, NPC.Center - screenPos, null, new Color(255, 215, 100, 0) * (0.75f * grow * pulse), 0f, glow.Size() / 2f, 560f / glow.Width, SpriteEffects.None, 0f);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			// Mirror Court: only the real her has a faint halo (how to tell her from the clones).
			if (CurrentAttack == Attack.MirrorCourt && Timer >= MirrorAppear && HeavensSkyTextures.Ready)
			{
				float pulse = 0.8f + 0.2f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
				Texture2D glow = HeavensSkyTextures.Soft;
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				spriteBatch.Draw(glow, NPC.Center - screenPos, null, new Color(255, 215, 100, 0) * (0.3f * pulse), 0f, glow.Size() / 2f, 340f / glow.Width, SpriteEffects.None, 0f);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			if (NPC.velocity.Length() > DashTrailSpeed)
			{
				Texture2D texture = BodyTexture;
				Vector2 origin = NPC.frame.Size() / 2f;
				for (int i = NPC.oldPos.Length - 1; i >= 1; i--)
				{
					if (NPC.oldPos[i] == Vector2.Zero)
					{
						continue;
					}
					float fade = 1f - i / (float)NPC.oldPos.Length;
					Vector2 position = NPC.oldPos[i] + NPC.Size / 2f - screenPos + new Vector2(0f, NPC.gfxOffY);
					spriteBatch.Draw(texture, position, NPC.frame, new Color(255, 215, 110, 0) * (0.45f * fade), 0f, origin, 1f, SpriteEffects.None, 0f);
				}
			}
			if (Stage == 2)
			{
				// The stage 2 picture (the owner's enraged design) replaces the normal one.
				spriteBatch.Draw(BodyTexture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);
				return false;
			}
			return true;
		}

		private Texture2D BodyTexture
		{
			get
			{
				if (Stage != 2)
				{
					return TextureAssets.Npc[Type].Value;
				}
				stageTwoTexture ??= ModContent.Request<Texture2D>("MotherOfTerraria/NPCs/Bosses/MotherOfTerrariaBoss_Stage2", AssetRequestMode.ImmediateLoad);
				return stageTwoTexture.Value;
			}
		}

		// Runs once when she appears, after the game has scaled her health for the difficulty: put it back to exactly BossLife.
		public override void OnSpawn(IEntitySource source)
		{
			NPC.lifeMax = BossLife;
			NPC.life = BossLife;
		}

		// Plays the wing-beat animation on a loop.
		public override void FindFrame(int frameHeight)
		{
			bool beatingHard = CurrentAttack == Attack.FeatherStorm && Timer < FeatherTell + 30; // the wings beat twice as fast before the storm
			NPC.frameCounter += beatingHard ? 2 : 1;
			NPC.frame.Y = (int)(NPC.frameCounter / TicksPerFrame % FrameCount) * frameHeight;
			if (CurrentAttack == Attack.CrystalPhase || CurrentAttack == Attack.CrossBeams)
			{
				NPC.frame.Y = WingsSpreadFrame * frameHeight; // wings spread while she forms the circle and while the crystals live
			}
			else if (CurrentAttack == Attack.WingBeams && Timer > 8 && Timer < WingBeamsEnd - 15)
			{
				NPC.frame.Y = WingsSpreadFrame * frameHeight; // wings held wide for the Wing Wall
			}
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];

			// The player died or left: she vanishes (the Basket rule for this is handled by HeavenlyBasketPlayer).
			if (!target.active || target.dead)
			{
				NPC.velocity.Y -= 0.4f;
				NPC.EncourageDespawn(30);
				return;
			}
			NPC.timeLeft = 600; // a boss never despawns just for being far away

			Lighting.AddLight(NPC.Center, 1f, 0.85f, 0.4f);
			if (Main.GameUpdateCount % 10 == 0)
			{
				HeavensEnemyCleaner.Clear(); // the fight is the boss alone
			}

			// Dash to the current station (relative to the player, so she follows when the player moves). During the
			// beam she holds still once she is in place. She faces the front, so the picture is never flipped.
			bool holding = CurrentAttack == Attack.CrystalPhase || CurrentAttack == Attack.CrossBeams || CurrentAttack == Attack.MirrorCourt || CurrentAttack == Attack.StarHell || CurrentAttack == Attack.GravityWell || (CurrentAttack == Attack.DeathBeam && Timer >= BeamStart - 5) || (CurrentAttack == Attack.WingBeams && Timer >= WingBeamsStart - 5);
			Vector2 station = Stations[(int)StationIndex % Stations.Length];
			Vector2 goal = target.Center + station + new Vector2(0f, 10f * (float)System.Math.Sin(Main.GameUpdateCount / 40f)); // a slow, heavy bob
			Vector2 toGoal = goal - NPC.Center;
			float topSpeed = (int)StationIndex % 3 == 0 ? DashSpeed : DriftSpeed;
			float speed = holding ? 0f : System.Math.Min(topSpeed, System.Math.Max(0f, toGoal.Length() - StayWithin) * DashEase);
			NPC.velocity = Vector2.Lerp(NPC.velocity, toGoal.SafeNormalize(Vector2.Zero) * speed, holding ? 0.15f : 0.25f);
			NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

			// At 50% health (StageTwoAt) the stage change begins, whatever she was doing (her own beams stop; spears and stars already in flight finish).
			if (Stage == 1 && CurrentAttack != Attack.CrystalPhase && NPC.life <= NPC.lifeMax * StageTwoAt && Main.netMode != NetmodeID.MultiplayerClient)
			{
				StartCrystalPhase();
			}
			NPC.dontTakeDamage = CurrentAttack == Attack.CrystalPhase || CurrentAttack == Attack.CrossBeams || (CurrentAttack == Attack.StarHell && Timer >= HellTell) || (CurrentAttack == Attack.GravityWell && Timer >= WellTell) || BeamsShielded; // untouchable while the crystals live, during the cross beams, and from the moment the Star Hell stars are released (owner, 2026-10-26)

			Timer++;
			if (Timer < 0)
			{
				GatherLight(1.5f); // the stage 2 roar pause: no attack yet
				return;
			}
			switch (CurrentAttack)
			{
				case Attack.SpearFan:
					DoSpearFan(target);
					break;
				case Attack.SpearBurst:
					DoSpearBurst(target);
					break;
				case Attack.DeathBeam:
					DoDeathBeam(target);
					break;
				case Attack.FallingStars:
					DoFallingStars(target);
					break;
				case Attack.FeatherStorm:
					DoFeatherStorm();
					break;
				case Attack.WingBeams:
					DoWingBeams(target);
					break;
				case Attack.CrystalPhase:
					DoCrystalPhase();
					break;
				case Attack.CrossBeams:
					DoCrossBeams();
					break;
				case Attack.MirrorCourt:
					DoMirrorCourt(target);
					break;
				case Attack.StarHell:
					DoStarHell();
					break;
				case Attack.GravityWell:
					DoGravityWell(target);
					break;
			}
		}

		// Moves on to the next attack in the cycle (fan -> burst -> beam -> fan ...).
		// Owner, 2026-10-26: she cannot be hurt while she holds still for a stage 1 beam attack (the Death Beam and the Wing Beams), like the Cross Beams in stage 2.
		// From the moment she stops to aim until the attack ends (the same moment `holding` in AI starts).
		private bool BeamsShielded =>
			(CurrentAttack == Attack.DeathBeam && Timer >= BeamStart - 5) || (CurrentAttack == Attack.WingBeams && Timer >= WingBeamsStart - 5);

		private void NextAttack()
		{
			Timer = 0;
			if (Stage == 2)
			{
				// Shuffled: any stage 2 attack except the one that just ran. Mirror Court is the rarest: weight 1 against OtherAttackWeight,
				// only after MirrorRepeatGap other attacks, and at most MirrorMaxUses times in the whole fight.
				attacksSinceMirror++;
				attacksSinceWell++;
				System.Collections.Generic.List<Attack> choices = new();
				foreach (Attack candidate in StageTwoLoop)
				{
					if (candidate == CurrentAttack)
					{
						continue;
					}
					if (candidate == Attack.GravityWell)
					{
						if (attacksSinceWell >= WellRepeatGap)
						{
							choices.Add(candidate); // weight 1: the rarest, with Mirror Court
						}
					}
					else if (candidate != Attack.MirrorCourt)
					{
						for (int w = 0; w < OtherAttackWeight; w++)
						{
							choices.Add(candidate);
						}
					}
					else if (attacksSinceMirror >= MirrorRepeatGap && mirrorUses < MirrorMaxUses)
					{
						choices.Add(candidate);
					}
				}
				CurrentAttack = choices[Main.rand.Next(choices.Count)];
				if (CurrentAttack == Attack.GravityWell)
				{
					attacksSinceWell = 0;
				}
				if (CurrentAttack == Attack.MirrorCourt)
				{
					attacksSinceMirror = 0;
					mirrorUses++;
				}
			}
			else
			{
				CurrentAttack = (Attack)(((int)CurrentAttack + 1) % AttackCount);
			}
			// She only changes position every StationEvery attacks; the rest of the time she hangs in place.
			AttacksSinceMove++;
			if (AttacksSinceMove >= StationEvery)
			{
				AttacksSinceMove = 0;
				StationIndex = (StationIndex + 1) % Stations.Length;
			}
			NPC.netUpdate = true;
		}

		// Gold dust gathering around her: the visible tell for the spear attacks.
		private void GatherLight(float intensity)
		{
			Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldCoin, 0f, -2f, 0, default, 1.4f);
			dust.noGravity = true;
			Lighting.AddLight(NPC.Center, 1.4f * intensity, 1.2f * intensity, 0.5f * intensity);
		}

		private void DoSpearFan(Player target)
		{
			if (Timer > 0 && Timer < FanTell)
			{
				GatherLight(1f);
			}
			if (Timer == FanTell)
			{
				SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
					for (int i = 0; i < SpearsInFan; i++)
					{
						float angle = (i - (SpearsInFan - 1) / 2f) * FanSpread;
						if (Stage == 2)
						{
							ThrowSpear(aim.RotatedBy(angle) * SpearSpeed, ModContent.ProjectileType<StarlightLance>(), StarlightDamage); // stage 2: Starlight Lances
						}
						else
						{
							ThrowSpear(aim.RotatedBy(angle) * SpearSpeed);
						}
					}
				}
			}
			if (Timer >= FanEnd)
			{
				NextAttack();
			}
		}

		// Rings of spears in every direction. The second ring is turned by half a gap so it covers the holes in the first.
		private void DoSpearBurst(Player target)
		{
			if (Timer > 0 && Timer < BurstTell)
			{
				GatherLight(1.5f + Timer / BurstTell);
			}
			if (Timer == BurstTell || Timer == BurstSecondWave)
			{
				SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					float step = MathHelper.TwoPi / RingSpears;
					float start = Timer == BurstTell ? (target.Center - NPC.Center).ToRotation() : (target.Center - NPC.Center).ToRotation() + step / 2f;
					for (int i = 0; i < RingSpears; i++)
					{
						if (Stage == 2)
						{
							ThrowSpear((start + i * step).ToRotationVector2() * RingSpearSpeed, ModContent.ProjectileType<StarlightLance>(), StarlightDamage); // stage 2: the swarm throws lances at 200 each (owner, 2026-10-25)
						}
						else
						{
							ThrowSpear((start + i * step).ToRotationVector2() * RingSpearSpeed);
						}
					}
				}
			}
			if (Timer >= BurstEnd)
			{
				NextAttack();
			}
		}

		// A thin line that follows the player, then a huge sweeping golden beam (see Projectiles/GoldenDeathray.cs).
		private void DoDeathBeam(Player target)
		{
			if (Timer == BeamStart)
			{
				SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					float angle = (target.Center - NPC.Center).ToRotation();
					float sweepDirection = Main.rand.NextBool() ? 1f : -1f;
					int damage = NPC.GetAttackDamage_ForProjectiles(LaserDamage, LaserDamage * 0.8f);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GoldenDeathray>(),
						damage, 0f, Main.myPlayer, NPC.whoAmI, angle, sweepDirection);
				}
			}
			if (Timer >= BeamStart && Timer < BeamStart + GoldenDeathray.TellTicks)
			{
				GatherLight(0.8f);
			}
			if (Timer >= LaserEnd)
			{
				NextAttack();
			}
		}

		// Gold stars appear in a row high above the player, then drop one by one, sweeping left to right or right to left.
		// The last star follows the player's sideways position and locks just before it drops.
		private void DoFallingStars(Player target)
		{
			if (Timer > 0 && Timer < StarSpawnAt)
			{
				GatherLight(1.4f);
			}
			if (Timer == StarSpawnAt)
			{
				SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int damage = NPC.GetAttackDamage_ForProjectiles(StarDamage, StarDamage * 0.8f);
					float y = System.Math.Max(target.Center.Y - StarHeight, 200f);
					// TWO rows of stars (doubled, owner request 2026-10-24): the first row, then a second row half a gap to the side
					// that fills in the spaces and sweeps the opposite way, then the one star that follows the player.
					bool leftToRight = Main.rand.NextBool();
					float secondRowShift = StarSpacing / 2f * (Main.rand.NextBool() ? 1f : -1f);
					int secondRowDelay = StarColumns * StarGap + StarRowPause;
					for (int row = 0; row < StarRows; row++)
					{
						for (int i = 0; i < StarColumns; i++)
						{
							bool forward = (row == 0) == leftToRight;
							int order = forward ? i : StarColumns - 1 - i; // which star of the row drops first
							float x = target.Center.X + (i - (StarColumns - 1) / 2f) * StarSpacing + (row == 1 ? secondRowShift : 0f);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(x, y), Vector2.Zero, ModContent.ProjectileType<GoldCrystalStar>(),
								damage, 0f, Main.myPlayer, StarHoverTicks + row * secondRowDelay + order * StarGap, 0f);
						}
					}
					Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(target.Center.X, y), Vector2.Zero, ModContent.ProjectileType<GoldCrystalStar>(),
						damage, 0f, Main.myPlayer, StarHoverTicks + (StarRows - 1) * secondRowDelay + StarColumns * StarGap + StarTrackerExtra, 1f);
				}
			}
			if (Timer >= StarsEnd)
			{
				NextAttack();
			}
		}

		// The spiral: every FeatherEvery ticks one feather per arm leaves her, and the whole pattern turns by FeatherTurn.
		// It turns one way for the first half and the other way for the second half.
		private void DoFeatherStorm()
		{
			if (Timer > 0 && Timer < FeatherTell)
			{
				GatherLight(1.2f);
				// Dust bursts off the wing tips: the wings are beating hard.
				for (int side = -1; side <= 1; side += 2)
				{
					Dust dust = Dust.NewDustPerfect(NPC.Center + new Vector2(side * Main.rand.NextFloat(120f, 200f), Main.rand.NextFloat(-90f, 40f)),
						DustID.GoldCoin, new Vector2(side * 2f, 1f), 0, default, 1.3f);
					dust.noGravity = true;
				}
			}

			int throwIndex = ((int)Timer - FeatherTell) / FeatherEvery;
			if (Timer >= FeatherTell && (Timer - FeatherTell) % FeatherEvery == 0 && throwIndex < FeatherEmits)
			{
				if (throwIndex == 0)
				{
					SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
				}
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					// Where the pattern points now: the sum of all turns so far (forward in the first half, then back).
					int half = FeatherEmits / 2;
					float angle = throwIndex < half ? throwIndex * FeatherTurn : (half - (throwIndex - half)) * FeatherTurn;
					angle += MathHelper.PiOver2 * 0.5f; // the first throw does not start at a round angle
					int damage = NPC.GetAttackDamage_ForProjectiles(FeatherDamage, FeatherDamage * 0.8f);
					for (int arm = 0; arm < FeatherArms; arm++)
					{
						float a = angle + arm * MathHelper.TwoPi / FeatherArms;
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, a.ToRotationVector2() * FeatherStartSpeed,
							ModContent.ProjectileType<GoldenFeather>(), damage, 0f, Main.myPlayer);
					}
				}
			}
			if (Timer >= FeatherEnd)
			{
				NextAttack();
			}
		}

		// She holds her wings spread (see FindFrame), gold dust streams off the wing tips, then a beam fires out of each wing tip.
		private void DoWingBeams(Player target)
		{
			if (Timer > 0 && Timer < WingBeamsStart)
			{
				GatherLight(1.2f);
				for (int side = -1; side <= 1; side += 2)
				{
					Dust dust = Dust.NewDustPerfect(NPC.Center + new Vector2(side * 170f, -70f) + Main.rand.NextVector2Circular(30f, 30f),
						DustID.GoldCoin, new Vector2(side * 1.5f, -1f), 0, default, 1.4f);
					dust.noGravity = true;
				}
			}
			if (Timer == WingBeamsStart)
			{
				SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int damage = NPC.GetAttackDamage_ForProjectiles(WingBeamDamage, WingBeamDamage * 0.8f);
					float sweepDirection = Main.rand.NextBool() ? 1f : -1f; // both beams sweep the same way
					Vector2 left = NPC.Center + new Vector2(-170f, -70f);
					Vector2 right = NPC.Center + new Vector2(170f, -70f);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), left, Vector2.Zero, ModContent.ProjectileType<WingBeamLeft>(), damage, 0f, Main.myPlayer,
						NPC.whoAmI, (target.Center - left).ToRotation() + 0.4f, sweepDirection);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), right, Vector2.Zero, ModContent.ProjectileType<WingBeamRight>(), damage, 0f, Main.myPlayer,
						NPC.whoAmI, (target.Center - right).ToRotation() - 0.4f, sweepDirection);
				}
			}
			if (Timer >= WingBeamsEnd)
			{
				NextAttack();
			}
		}

		// She stops, glowing gold (the glow around her means she cannot be hurt), light streams into her for 2 seconds, then four beams
		// leave her in a plus shape. The beams' own thin tell lines are visible for the last 75 ticks of the charge.
		private void DoCrossBeams()
		{
			if (Timer == 1)
			{
				SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.4f }, NPC.Center); // a low chime as she starts to charge
			}
			if (Timer < CrossCharge)
			{
				float charge = Timer / (float)CrossCharge;
				for (int i = 0; i < 3; i++)
				{
					// light streams in from all round and gets faster and denser as the charge builds
					Vector2 direction = Main.rand.NextVector2Unit();
					float distance = Main.rand.NextFloat(180f, 320f) * (1.1f - 0.4f * charge);
					Dust dust = Dust.NewDustPerfect(NPC.Center + direction * distance, DustID.GoldCoin, -direction * (2f + 6f * charge), 0, default, 1.3f);
					dust.noGravity = true;
				}
				GatherLight(1f + charge * 1.5f);
			}
			if (Timer == CrossSpawnAt)
			{
				SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int damage = NPC.GetAttackDamage_ForProjectiles(CrossDamage, CrossDamage * 0.8f);
					float sweepDirection = Main.rand.NextBool() ? 1f : -1f; // all of them sweep the same way, so the star keeps its shape
					for (int arm = 0; arm < CrossArms; arm++)
					{
						float angle = -MathHelper.PiOver2 + arm * MathHelper.TwoPi / CrossArms; // up first, then every 45 degrees round: an eight-pointed star
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CrossBeam>(),
							damage, 0f, Main.myPlayer, NPC.whoAmI, angle, sweepDirection);
					}
				}
			}
			if (Timer >= CrossEnd)
			{
				NextAttack();
			}
		}

		// GRAVITY WELL: she holds still for a second gathering light, then the well opens (see the constants above and Projectiles/GravityWell.cs).
		private void DoGravityWell(Player target)
		{
			if (Timer < WellTell)
			{
				GatherLight(1f + Timer / (float)WellTell * 1.5f);
				if (Timer == 1)
				{
					SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.5f }, NPC.Center);
				}
			}
			if (Timer == WellTell && Main.netMode != NetmodeID.MultiplayerClient)
			{
				// somewhere on the upper half of a ring around the player (never below), and not on top of her
				float angle = -MathHelper.Pi + Main.rand.NextFloat(0.2f, MathHelper.Pi - 0.2f); // between left and right, through straight up
				Vector2 place = target.Center + angle.ToRotationVector2() * WellDistance;
				if (Vector2.Distance(place, NPC.Center) < 280f)
				{
					place.X = target.Center.X + (place.X > target.Center.X ? -1f : 1f) * System.Math.Abs(place.X - target.Center.X); // the other side
				}
				int damage = NPC.GetAttackDamage_ForProjectiles(WellDamage, WellDamage * 0.8f);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), place, Vector2.Zero, ModContent.ProjectileType<GravityWell>(), damage, 0f, Main.myPlayer, NPC.whoAmI);
			}
			if (Timer >= WellEnd)
			{
				NextAttack();
			}
		}

		// STAR HELL: she holds still for a second gathering energy (the tell), then sends the stars out in waves; she moves on when they are all gone.
		private void DoStarHell()
		{
			if (Timer < HellTell)
			{
				float charge = Timer / (float)HellTell;
				for (int i = 0; i < 3; i++)
				{
					Vector2 direction = Main.rand.NextVector2Unit();
					Dust dust = Dust.NewDustPerfect(NPC.Center + direction * Main.rand.NextFloat(150f, 260f), DustID.GoldCoin, -direction * (3f + 5f * charge), 0, default, 1.3f);
					dust.noGravity = true;
				}
				GatherLight(1f + charge * 1.5f);
				if (Timer == 1)
				{
					SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f }, NPC.Center);
				}
			}
			int sinceStart = (int)Timer - HellTell;
			if (sinceStart >= 0 && sinceStart % HellWaveGap == 0 && sinceStart / HellWaveGap < HellWaves)
			{
				int wave = sinceStart / HellWaveGap;
				SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.7f, Pitch = 0.2f }, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int perWave = HellCount / HellWaves;
					float offset = Main.rand.NextFloat(MathHelper.TwoPi);
					for (int i = 0; i < perWave; i++)
					{
						float angle = offset + (i + wave * 0.5f) * MathHelper.TwoPi / perWave;
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<SeekerStar>(), 0, NPC.whoAmI, angle);
					}
				}
			}
			if (Timer >= HellMin && (Timer >= HellMax || !StarsAlive()))
			{
				NextAttack();
			}
		}

		private bool StarsAlive()
		{
			int star = ModContent.NPCType<SeekerStar>();
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == star && (int)npc.ai[0] == NPC.whoAmI)
				{
					return true;
				}
			}
			return false;
		}

		// MIRROR COURT. She gathers light for a second (the tell), then the four figures appear on a ring around the player (she takes a random
		// one of the four places, the clones the rest) and everyone walks in. Her health is put back every tick and the difference is taken off the shield.
		private void DoMirrorCourt(Player target)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}
			if (Timer == 1)
			{
				shieldLeft = MirrorShield;
				lifeAtShield = NPC.life;
				SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.2f }, NPC.Center);
			}
			int dealt = lifeAtShield - NPC.life;
			if (dealt > 0)
			{
				shieldLeft -= dealt;
			}
			NPC.life = lifeAtShield; // her real health cannot drop while the shield holds

			if (Timer < MirrorAppear)
			{
				GatherLight(1f + Timer / (float)MirrorAppear);
			}
			else if (Timer == MirrorAppear)
			{
				SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
				float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);
				int real = Main.rand.Next(4);
				for (int i = 0; i < 4; i++)
				{
					Vector2 place = target.Center + (startAngle + i * MathHelper.PiOver2).ToRotationVector2() * MirrorDistance;
					for (int d = 0; d < 30; d++)
					{
						Dust dust = Dust.NewDustPerfect(place + Main.rand.NextVector2Circular(60f, 100f), DustID.GoldCoin, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.6f);
						dust.noGravity = true;
					}
					if (i == real)
					{
						NPC.Center = place;
						NPC.velocity = Vector2.Zero;
						System.Array.Clear(NPC.oldPos);
					}
					else
					{
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)place.X, (int)place.Y, ModContent.NPCType<MirrorClone>(), 0, NPC.whoAmI);
					}
				}
			}
			else
			{
				// everyone walks in (she overrides her usual movement, which holds her still during this attack)
				Vector2 toPlayer = target.Center - NPC.Center;
				NPC.velocity = toPlayer.Length() > 50f ? toPlayer.SafeNormalize(Vector2.Zero) * MirrorSpeed : Vector2.Zero;
			}

			if (Timer > MirrorAppear && shieldLeft <= 0)
			{
				SoundEngine.PlaySound(SoundID.Item107, NPC.Center); // the shield shatters
				for (int i = 0; i < 40; i++)
				{
					Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.GoldCoin, Main.rand.NextVector2Circular(12f, 12f), 0, default, 1.8f);
					dust.noGravity = true;
				}
				EndMirrorCourt();
			}
			else if (Timer >= MirrorEnd)
			{
				EndMirrorCourt(); // the clones reached the player and had their chance; the shield drops
			}
		}

		private void EndMirrorCourt()
		{
			int clone = ModContent.NPCType<MirrorClone>();
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == clone && (int)npc.ai[0] == NPC.whoAmI)
				{
					MirrorClone.Vanish(npc);
				}
			}
			NextAttack();
		}

		// Starts the stage change. A big hit can carry her past the threshold, so her health is put back to exactly StageTwoAt.
		private void StartCrystalPhase()
		{
			NPC.life = System.Math.Max(NPC.life, (int)System.Math.Ceiling(NPC.lifeMax * StageTwoAt));
			foreach (Projectile projectile in Main.ActiveProjectiles)
			{
				if (projectile.ModProjectile is GoldenDeathray)
				{
					projectile.Kill(); // her death beam and wing beams stop at once (they are tied to her)
				}
			}
			CurrentAttack = Attack.CrystalPhase;
			Timer = 0;
			NPC.netUpdate = true;
		}

		// She stops and draws a golden circle around herself for CrystalTell ticks (dust fills in the circle a little more every
		// tick: the tell), then four Enchanted Crystals appear on the circle. She stays untouchable until all four are dead,
		// and then stage 2 starts at once.
		private void DoCrystalPhase()
		{
			if (Timer < CrystalTell)
			{
				float filled = Timer / CrystalTell;
				for (int i = 0; i < 5; i++)
				{
					float angle = -MathHelper.PiOver2 + Main.rand.NextFloat() * filled * MathHelper.TwoPi; // the circle grows clockwise from the top
					Dust dust = Dust.NewDustPerfect(NPC.Center + angle.ToRotationVector2() * CircleRadius, DustID.GoldCoin, Vector2.Zero, 0, default, 1.5f);
					dust.noGravity = true;
					dust.velocity = angle.ToRotationVector2() * 0.4f;
				}
				GatherLight(1f + filled);
				if (Timer == 1 || Timer % 60 == 0)
				{
					SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.5f + filled }, NPC.Center); // a chime that rises as the circle closes
				}
			}
			else if (Timer == CrystalTell)
			{
				SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int i = 0; i < CrystalCount; i++)
					{
						Vector2 position = NPC.Center + (-MathHelper.PiOver4 + i * MathHelper.PiOver2).ToRotationVector2() * CircleRadius;
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)position.X, (int)position.Y, ModContent.NPCType<EnchantedCrystal>(), 0, NPC.whoAmI, 0f, i);
					}
				}
			}
			else if (Timer > CrystalTell + 10 && !CrystalsAlive())
			{
				BeginStageTwo(); // she is vulnerable again (dontTakeDamage follows the attack)
			}
		}

		// TESTING ONLY (the Debug Stage Two Skipper item calls this; remove with it): jump straight to stage 2, skipping the crystals.
		public void DebugSkipToStageTwo()
		{
			if (Stage == 2 || Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}
			int crystal = ModContent.NPCType<EnchantedCrystal>();
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == crystal && (int)npc.ai[0] == NPC.whoAmI)
				{
					npc.active = false;
				}
			}
			foreach (Projectile projectile in Main.ActiveProjectiles)
			{
				if (projectile.ModProjectile is GoldenDeathray)
				{
					projectile.Kill();
				}
			}
			NPC.life = System.Math.Min(NPC.life, (int)System.Math.Ceiling(NPC.lifeMax * StageTwoAt));
			BeginStageTwo();
		}

		// The switch to stage 2 (owner, 2026-10-26): she ROARS (a long roar, the screen shakes, a burst of light), TELEPORTS to her place above the player
		// (a flash where she was and where she lands), and holds still for StageTwoPause ticks before the first stage 2 attack. The attack code does not run
		// while Timer is below zero (see AI).
		private void BeginStageTwo()
		{
			Player target = Main.player[NPC.target];
			SoundEngine.PlaySound(SoundID.Roar with { Volume = 1f }, NPC.Center);
			Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, Vector2.UnitY, 9f, 16f, 70, -1f, "MotherOfTerraria:StageTwoRoar"));
			for (int i = 0; i < 60; i++)
			{
				Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.GoldCoin, Main.rand.NextVector2Circular(14f, 14f), 0, default, 1.8f);
				dust.noGravity = true;
			}
			StationIndex = 0;                               // straight above the player (see Stations)
			AttacksSinceMove = 0;
			Vector2 landing = target.Center + Stations[0];
			NPC.Center = landing;
			NPC.velocity = Vector2.Zero;
			System.Array.Clear(NPC.oldPos);
			for (int i = 0; i < 60; i++)
			{
				Dust dust = Dust.NewDustPerfect(landing, DustID.GoldCoin, Main.rand.NextVector2Circular(14f, 14f), 0, default, 1.8f);
				dust.noGravity = true;
			}
			Stage = 2;
			CurrentAttack = Attack.SpearFan;
			Timer = -StageTwoPause;
			NPC.netUpdate = true;
		}

		private bool CrystalsAlive()
		{
			int crystal = ModContent.NPCType<EnchantedCrystal>();
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == crystal && (int)npc.ai[0] == NPC.whoAmI)
				{
					return true;
				}
			}
			return false;
		}

		// One Celestial Spear: her own spear, which uses the boss's stronger homing (see Projectiles/HeavenlySpear.cs).
		private void ThrowSpear(Vector2 velocity, int type = -1, int baseDamage = SpearDamage)
		{
			int damage = NPC.GetAttackDamage_ForProjectiles(baseDamage, baseDamage * 0.8f);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
				type < 0 ? ModContent.ProjectileType<CelestialSpear>() : type, damage, 2f, Main.myPlayer);
		}

		// Owner rule (2026-10-14): the Basket empties when the boss is KILLED (and when the player dies, which
		// HeavenlyBasketPlayer handles). SINGLEPLAYER-ONLY: this reaches the local player's Basket.
		public override void OnKill()
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Main.LocalPlayer.GetModPlayer<HeavenlyBasketPlayer>().EndFight(true);
			}
		}

		// PLACEHOLDER loot: lunar bars. Expert/Master gets nothing extra yet; the treasure bag, trophy, relic and
		// the real drops need art and design (Phases 6 and 7, see ASSETS.md).
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemID.LunarBar, 1, 20, 30));
		}
	}
}
