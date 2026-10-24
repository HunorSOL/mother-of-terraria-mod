using Microsoft.Xna.Framework;
using MotherOfTerraria.Common;
using MotherOfTerraria.NPCs.Heavens;
using MotherOfTerraria.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Bosses
{
	// The Mother of Terraria: the mod's final boss. Phase 5 built the skeleton; this is the FIRST STAGE with a looping
	// attack cycle (more attacks and a second stage come later, and the final balance is Phase 6):
	//   1. SPEAR FAN:   a short gold glow, then three slightly homing Heavenly Spears aimed at the player.
	//   2. SPEAR BURST: she gathers light, then throws rings of slightly homing spears in ALL directions (two waves,
	//                   the second ring offset so it fills the gaps of the first).
	//   3. DEATH BEAM:  a thin line follows the player (the tell), then locks and a huge golden beam sweeps across.
	//   ...then back to 1.
	// While she is alive no other enemy may be in the Heavens (the spawner stops, and any that are left are removed).
	// Texture: NPCs/Bosses/MotherOfTerrariaBoss.png (PLACEHOLDER, 160x160, made by tools/make_boss_placeholder.py).
	// ALL NUMBERS BELOW ARE PLACEHOLDERS; the real balance is Phase 6.
	// SINGLEPLAYER-ONLY: OnKill talks to the local player's Basket (see HeavenlyBasketPlayer); multiplayer needs the server to do that.
	public class MotherOfTerrariaBoss : ModNPC
	{
		private const float HoverHeight = 260f;     // how far above the player she floats (pixels)
		private const float SideSwing = 320f;       // how far she drifts left and right of the player
		private const float MoveSpeed = 9f;

		private const float SpearSpeed = 9f;
		private const int SpearDamage = 150;
		private const int SpearsInFan = 3;
		private const float FanSpread = 0.28f;      // radians between neighbouring spears
		private const int RingSpears = 18;          // spears per ring in the burst
		private const float RingSpearSpeed = 7f;

		private const int LaserDamage = 700;        // "super punishing": about three beam hits kill an unbuffed player

		// Times in ticks (60 per second), counted from the start of each attack.
		private const int FanTell = 30, FanEnd = 90;
		private const int BurstTell = 45, BurstSecondWave = 80, BurstEnd = 130;
		private const int LaserEnd = GoldenDeathray.TellTicks + GoldenDeathray.BeamTicks + 50;

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

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 1;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Hide = true }; // no bestiary entry until the real design exists
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 90;
			NPC.height = 130;
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
			ClearOtherEnemies();

			// Float above the player, swinging slowly from side to side. During the beam she holds still.
			bool holding = CurrentAttack == Attack.DeathBeam;
			float swing = (float)System.Math.Sin(Main.GameUpdateCount / 90f) * SideSwing;
			Vector2 goal = target.Center + new Vector2(swing, -HoverHeight);
			Vector2 toGoal = goal - NPC.Center;
			float speed = holding ? 0f : System.Math.Min(MoveSpeed, toGoal.Length() * 0.1f);
			NPC.velocity = Vector2.Lerp(NPC.velocity, toGoal.SafeNormalize(Vector2.Zero) * speed, holding ? 0.05f : 0.1f);
			NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

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
			if (Timer == 1)
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
			if (Timer < GoldenDeathray.TellTicks)
			{
				GatherLight(0.8f);
			}
			if (Timer >= LaserEnd)
			{
				NextAttack();
			}
		}

		// One Heavenly Spear, with ai[0] = 1 so it uses the boss's stronger homing (see Projectiles/HeavenlySpear.cs).
		private void ThrowSpear(Vector2 velocity)
		{
			int damage = NPC.GetAttackDamage_ForProjectiles(SpearDamage, SpearDamage * 0.8f);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
				ModContent.ProjectileType<HeavenlySpear>(), damage, 2f, Main.myPlayer, 1f);
		}

		// The fight is the boss alone: any Heavens enemy that is still around (or that arrives some other way)
		// is removed. They vanish without dying, so they drop nothing. The spawner is also paused while she lives.
		private void ClearOtherEnemies()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || Main.GameUpdateCount % 10 != 0)
			{
				return;
			}
			int knight = ModContent.NPCType<HeavenlyKnight>();
			int dragon = ModContent.NPCType<HeavensDragon>();
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == knight || npc.type == dragon)
				{
					for (int i = 0; i < 12; i++)
					{
						Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GoldCoin, 0f, 0f, 0, default, 1.3f);
						dust.noGravity = true;
					}
					npc.active = false;
					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
					}
				}
			}
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
