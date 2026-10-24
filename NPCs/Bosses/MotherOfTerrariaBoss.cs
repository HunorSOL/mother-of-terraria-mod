using Microsoft.Xna.Framework;
using MotherOfTerraria.Common;
using MotherOfTerraria.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Bosses
{
	// The Mother of Terraria: the mod's final boss (Phase 5 = the skeleton only: it exists, can be fought and killed).
	// The real design, phases and attacks come in Phase 6; the art in Phase 7.
	// For now she floats above the player and, every few seconds, gathers light for half a second (the tell) and then
	// throws a fan of three slow Heavenly Spears. Contact also hurts.
	// Texture: NPCs/Bosses/MotherOfTerrariaBoss.png (PLACEHOLDER, 160x160, made by tools/make_boss_placeholder.py).
	// ALL NUMBERS BELOW ARE PLACEHOLDERS; the real balance is Phase 6.
	// SINGLEPLAYER-ONLY: OnKill talks to the local player's Basket (see HeavenlyBasketPlayer); multiplayer needs the server to do that.
	public class MotherOfTerrariaBoss : ModNPC
	{
		private const float HoverHeight = 260f;     // how far above the player she floats (pixels)
		private const float SideSwing = 320f;       // how far she drifts left and right of the player
		private const float MoveSpeed = 9f;
		private const int AttackEvery = 150;        // ticks between attacks (60 per second)
		private const int TellTime = 30;            // the visible warning before the spears
		private const int SpearsInFan = 3;
		private const float FanSpread = 0.28f;      // radians between neighbouring spears
		private const float SpearSpeed = 9f;
		private const int SpearDamage = 150;

		private ref float Timer => ref NPC.ai[0];

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
			NPC.lifeMax = 300000;    // placeholder, meant to be tougher than Moon Lord (Phase 6 balances it)
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

			// Float above the player, swinging slowly from side to side.
			float swing = (float)System.Math.Sin(Main.GameUpdateCount / 90f) * SideSwing;
			Vector2 goal = target.Center + new Vector2(swing, -HoverHeight);
			Vector2 toGoal = goal - NPC.Center;
			float speed = System.Math.Min(MoveSpeed, toGoal.Length() * 0.1f);
			NPC.velocity = Vector2.Lerp(NPC.velocity, toGoal.SafeNormalize(Vector2.Zero) * speed, 0.1f);
			NPC.spriteDirection = NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

			Timer++;

			// The tell: light gathers around her for the last half second before the attack.
			if (Timer > AttackEvery - TellTime)
			{
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldCoin, 0f, -2f, 0, default, 1.4f);
				dust.noGravity = true;
				Lighting.AddLight(NPC.Center, 1.4f, 1.2f, 0.5f);
			}

			if (Timer >= AttackEvery)
			{
				Timer = 0;
				ThrowSpearFan(target);
			}
		}

		// Three Heavenly Spears in a fan, aimed at the player. Only the server (or singleplayer) creates projectiles.
		private void ThrowSpearFan(Player target)
		{
			SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			Vector2 aim = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
			int damage = NPC.GetAttackDamage_ForProjectiles(SpearDamage, SpearDamage * 0.8f);
			for (int i = 0; i < SpearsInFan; i++)
			{
				float angle = (i - (SpearsInFan - 1) / 2f) * FanSpread;
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, aim.RotatedBy(angle) * SpearSpeed,
					ModContent.ProjectileType<HeavenlySpear>(), damage, 2f, Main.myPlayer);
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
