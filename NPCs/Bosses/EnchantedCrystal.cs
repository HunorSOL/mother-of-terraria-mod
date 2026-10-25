using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MotherOfTerraria.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Bosses
{
	// One of the four ENCHANTED CRYSTALS the Mother of Terraria releases when she falls to 60% health (the stage 1 -> stage 2
	// transition, see MotherOfTerrariaBoss.DoCrystalPhase). While any of them lives she cannot be hurt. They circle the player,
	// following them around, and each fires a short beam now and then (see Projectiles/CrystalBeam.cs); the player has to destroy all four.
	// ai[0] = the boss's NPC index, ai[1] = a tick counter, ai[2] = which crystal this is (0-3: its place around the player and when it fires).
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them).
	// SINGLEPLAYER-ONLY: none known; spawning and firing run on the server / single player, but this has not been tested in multiplayer.
	public class EnchantedCrystal : ModNPC
	{
		public const int CrystalLife = 50000;      // owner, 2026-10-25
		private const float OrbitRadius = 330f;     // how far from the player it hovers
		private const float OrbitSpin = 0.012f;     // radians per tick the four crystals turn around the player
		private const float Speed = 7f;
		private const int FirstShot = 90;           // ticks after appearing before the first beam
		private const int ShotEvery = 200;          // ticks between one crystal's beams
		private const int ShotOffset = 50;          // each crystal fires this much later than the one before, so they never all fire together
		private const int BeamDamage = 200;

		// PLACEHOLDER art: the Gold Crystal Star picture stands in until the owner designs the crystal (see ASSETS.md).
		public override string Texture => "MotherOfTerraria/Projectiles/GoldCrystalStar";

		public override void SetStaticDefaults()
		{
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Hide = true };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 56;
			NPC.height = 56;
			NPC.damage = 0;          // it attacks only with its beams
			NPC.defense = 60;
			NPC.lifeMax = CrystalLife;
			NPC.HitSound = SoundID.Item27; // glass
			NPC.DeathSound = SoundID.Item107;
			NPC.knockBackResist = 0f;
			NPC.value = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.lavaImmune = true;
			NPC.npcSlots = 0f;
			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.OnFire] = true;
			NPC.buffImmune[BuffID.Poisoned] = true;
		}

		// The game scales health by difficulty; set it back to exactly CrystalLife, as for the boss.
		public override void OnSpawn(IEntitySource source)
		{
			NPC.lifeMax = CrystalLife;
			NPC.life = CrystalLife;
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)NPC.ai[0]];
			if (!owner.active || owner.type != ModContent.NPCType<MotherOfTerrariaBoss>())
			{
				NPC.active = false; // she is gone, so are they
				return;
			}
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];
			if (!target.active || target.dead)
			{
				return;
			}
			NPC.ai[1]++;

			// Circle the player: the four crystals sit at the corners of a square that turns slowly around them.
			float angle = NPC.ai[2] * MathHelper.PiOver2 + NPC.ai[1] * OrbitSpin;
			Vector2 goal = target.Center + angle.ToRotationVector2() * OrbitRadius;
			Vector2 toGoal = goal - NPC.Center;
			float speed = System.Math.Min(Speed, toGoal.Length() * 0.15f);
			NPC.velocity = Vector2.Lerp(NPC.velocity, toGoal.SafeNormalize(Vector2.Zero) * speed, 0.2f);
			NPC.rotation = NPC.velocity.X * 0.02f;

			Lighting.AddLight(NPC.Center, 0.9f, 0.8f, 0.4f);

			int sinceFirst = (int)NPC.ai[1] - FirstShot - (int)NPC.ai[2] * ShotOffset;
			if (sinceFirst >= 0 && sinceFirst % ShotEvery == 0 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				int damage = NPC.GetAttackDamage_ForProjectiles(BeamDamage, BeamDamage * 0.8f);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CrystalBeam>(),
					damage, 0f, Main.myPlayer, NPC.whoAmI, (target.Center - NPC.Center).ToRotation());
			}
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			int count = NPC.life > 0 ? 3 : 30;
			for (int i = 0; i < count; i++)
			{
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldCoin, 0f, 0f, 0, default, 1.4f);
				dust.noGravity = true;
				dust.velocity *= NPC.life > 0 ? 1f : 4f;
			}
		}

		// A soft gold glow behind the crystal so it reads as enchanted, then the picture.
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Systems.HeavensSkyTextures.Ready)
			{
				Texture2D glow = Systems.HeavensSkyTextures.Soft;
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				float pulse = 0.85f + 0.15f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 5f + NPC.ai[2]);
				spriteBatch.Draw(glow, NPC.Center - screenPos, null, new Color(255, 215, 100, 0) * (0.8f * pulse), 0f, glow.Size() / 2f, 200f / glow.Width, SpriteEffects.None, 0f);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
			return true;
		}

		public override Color? GetAlpha(Color drawColor) => Color.White; // always fully bright
	}
}
