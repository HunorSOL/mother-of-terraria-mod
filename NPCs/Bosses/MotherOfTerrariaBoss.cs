using Microsoft.Xna.Framework;
using MotherOfTerraria.Common;
using MotherOfTerraria.NPCs.Heavens;
using MotherOfTerraria.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
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
	//   ...then back to 1. Before each attack she dashes to the next of five fixed spots around the player (see Stations).
	// While she is alive no other enemy may be in the Heavens (the spawner stops, and any that are left are removed).
	// Texture: NPCs/Bosses/MotherOfTerrariaBoss.png (the owner's design, 8 frames of 296x220 made by tools/make_boss_sheet.py: wings flap, hair and robe sway).
	// ALL NUMBERS BELOW ARE PLACEHOLDERS; the real balance is Phase 6.
	// SINGLEPLAYER-ONLY: OnKill talks to the local player's Basket (see HeavenlyBasketPlayer); multiplayer needs the server to do that.
	public class MotherOfTerrariaBoss : ModNPC
	{
		private const int FrameCount = 8;
		private const int TicksPerFrame = 6;        // wing-beat speed: one full flap takes 8 x 6 = 48 ticks

		// MOVEMENT (like the Empress of Light: fast, but always to a planned spot). She works through a fixed list of
		// "stations" around the player (offsets in pixels from the player: x to the right, y down). Each new attack she
		// dashes to the next station; the burst also flips her to the mirror-image side between its two rings.
		// Five stations and three attacks means every attack is eventually thrown from every station.
		private static readonly Vector2[] Stations =
		{
			new Vector2(-480f, -290f),  // high left
			new Vector2(480f, -290f),   // high right
			new Vector2(0f, -400f),     // straight above, far
			new Vector2(540f, -110f),   // low right
			new Vector2(-540f, -110f),  // low left
		};
		private const float MaxSpeed = 26f;         // top dash speed (pixels per tick)
		private const float DashEase = 0.16f;       // slows her down as she arrives (bigger = later braking)
		private const float DashTrailSpeed = 11f;   // above this speed she leaves a golden after-image trail

		private const float SpearSpeed = 9f;
		private const int SpearDamage = 150;
		private const int SpearsInFan = 3;
		private const float FanSpread = 0.28f;      // radians between neighbouring spears
		private const int RingSpears = 18;          // spears per ring in the burst
		private const float RingSpearSpeed = 7f;

		private const int BeamStart = 30;           // the beam attack waits this long first so she can reach her station
		private const int LaserDamage = 350;        // punishing but NOT a one-hit kill (was 700): it hurts hard, burns (Hellfire), and a beam you stay in hits again and again

		// Times in ticks (60 per second), counted from the start of each attack.
		private const int FanTell = 30, FanEnd = 90;
		private const int BurstTell = 45, BurstSecondWave = 80, BurstEnd = 130;
		private const int LaserEnd = BeamStart + GoldenDeathray.TellTicks + GoldenDeathray.BeamTicks + 50;

		private enum Attack
		{
			SpearFan = 0,
			SpearBurst = 1,
			DeathBeam = 2,
		}

		private Attack CurrentAttack
		{
			get => (Attack)(int)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private ref float Timer => ref NPC.ai[1];
		private ref float StationIndex => ref NPC.ai[2];
		private ref float Mirrored => ref NPC.ai[3]; // 1 = the station is flipped left-to-right

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
			NPC.width = 110;         // her body (the wings are scenery, not a hitbox); the 296x220 picture is centred on it
			NPC.height = 170;
			NPC.damage = 200;        // contact damage (placeholder)
			NPC.defense = 100;       // placeholder
			NPC.lifeMax = 600000;    // placeholder, doubled by the owner on 2026-10-24 (Phase 6 balances it)
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
			if (NPC.velocity.Length() > DashTrailSpeed)
			{
				Texture2D texture = TextureAssets.Npc[Type].Value;
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
			return true;
		}

		// Plays the wing-beat animation on a loop.
		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			NPC.frame.Y = (int)(NPC.frameCounter / TicksPerFrame % FrameCount) * frameHeight;
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
			bool holding = CurrentAttack == Attack.DeathBeam && Timer >= BeamStart - 5;
			Vector2 station = Stations[(int)StationIndex % Stations.Length];
			if (Mirrored == 1f)
			{
				station.X = -station.X;
			}
			Vector2 goal = target.Center + station + new Vector2(0f, 10f * (float)System.Math.Sin(Main.GameUpdateCount / 20f)); // a gentle bob
			Vector2 toGoal = goal - NPC.Center;
			float speed = holding ? 0f : System.Math.Min(MaxSpeed, toGoal.Length() * DashEase);
			NPC.velocity = Vector2.Lerp(NPC.velocity, toGoal.SafeNormalize(Vector2.Zero) * speed, holding ? 0.15f : 0.25f);
			NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

			Timer++;
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
			}
		}

		// Moves on to the next attack in the cycle (fan -> burst -> beam -> fan ...).
		private void NextAttack()
		{
			Timer = 0;
			CurrentAttack = (Attack)(((int)CurrentAttack + 1) % 3);
			StationIndex = (StationIndex + 1) % Stations.Length; // dash to the next station
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
						ThrowSpear(aim.RotatedBy(angle) * SpearSpeed);
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
			if (Timer == BurstTell + 8)
			{
				Mirrored = 1f - Mirrored; // dash across to the mirror-image side before the second ring
				NPC.netUpdate = true;
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
						ThrowSpear((start + i * step).ToRotationVector2() * RingSpearSpeed);
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

		// One Celestial Spear: her own spear, which uses the boss's stronger homing (see Projectiles/HeavenlySpear.cs).
		private void ThrowSpear(Vector2 velocity)
		{
			int damage = NPC.GetAttackDamage_ForProjectiles(SpearDamage, SpearDamage * 0.8f);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
				ModContent.ProjectileType<CelestialSpear>(), damage, 2f, Main.myPlayer);
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
