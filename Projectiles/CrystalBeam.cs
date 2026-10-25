using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MotherOfTerraria.NPCs.Bosses;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// The SHORT beam of an Enchanted Crystal (see NPCs/Bosses/EnchantedCrystal.cs). Like the boss's beam it has a tell and a hit part:
	//  1. THE TELL (0.75 s): a thin pale line from the crystal turns after the player, and holds still for the last 15 ticks. No damage.
	//  2. THE BEAM (0.4 s): a short, thin golden beam along that line. It does not sweep.
	// ai[0] = the crystal's NPC index, ai[1] = the beam's angle (radians).
	// Drawn from the game's white pixel, no picture file (only its 1x1 corner, see GoldenDeathray).
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them).
	public class CrystalBeam : ModProjectile
	{
		private const int TellTicks = 45;
		private const int BeamTicks = 24;
		private const int AimFreezeTicks = 15;
		private const float TrackTurnPerTick = 0.03f;
		private const float BeamLength = 900f;          // the owner liked the beams and asked for them longer (2026-10-25; was 480)
		private const float BeamHitWidth = 22f;
		private const float BeamDrawWidth = 44f;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;

		private bool Firing => Projectile.localAI[0] >= TellTicks;

		// Same fix as the boss's beam: draw it even when the crystal itself is off the screen (see GoldenDeathray).
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1500;
		}

		public override void SetDefaults()
		{
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = TellTicks + BeamTicks;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			NPC crystal = Main.npc[(int)Projectile.ai[0]];
			if (!crystal.active || crystal.type != ModContent.NPCType<EnchantedCrystal>())
			{
				Projectile.Kill();
				return;
			}
			Projectile.Center = crystal.Center;
			Projectile.velocity = Vector2.Zero;
			Projectile.localAI[0]++;

			if (Projectile.owner == Main.myPlayer)
			{
				if (!Firing && Projectile.localAI[0] < TellTicks - AimFreezeTicks)
				{
					Player target = Main.player[crystal.target];
					if (target.active && !target.dead)
					{
						float wanted = (target.Center - Projectile.Center).ToRotation();
						Projectile.ai[1] += MathHelper.Clamp(MathHelper.WrapAngle(wanted - Projectile.ai[1]), -TrackTurnPerTick, TrackTurnPerTick);
					}
				}
				Projectile.netUpdate = true;
			}

			Lighting.AddLight(Projectile.Center, 1f, 0.85f, 0.35f);
			if (Projectile.localAI[0] == TellTicks)
			{
				SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.7f }, Projectile.Center);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!Firing)
			{
				return false;
			}
			float unused = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
				Projectile.Center + Projectile.ai[1].ToRotationVector2() * BeamLength, BeamHitWidth, ref unused);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Vector2 start = Projectile.Center - Main.screenPosition;
			float rotation = Projectile.ai[1];
			Vector2 origin = new Vector2(0f, 0.5f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			if (!Firing)
			{
				float ramp = Projectile.localAI[0] / TellTicks;
				Draw(pixel, start, rotation, origin, BeamLength, 3f + 4f * ramp, new Color(255, 230, 140) * (0.25f + 0.5f * ramp));
			}
			else
			{
				float left = TellTicks + BeamTicks - Projectile.localAI[0];
				float strength = MathHelper.Clamp(System.Math.Min((Projectile.localAI[0] - TellTicks) / 4f, left / 6f), 0f, 1f);
				Draw(pixel, start, rotation, origin, BeamLength, BeamDrawWidth * strength, new Color(255, 170, 30) * 0.30f);
				Draw(pixel, start, rotation, origin, BeamLength, BeamDrawWidth * 0.6f * strength, new Color(255, 205, 70) * 0.55f);
				Draw(pixel, start, rotation, origin, BeamLength, BeamDrawWidth * 0.27f * strength, new Color(255, 250, 220) * 0.95f);
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		private static void Draw(Texture2D pixel, Vector2 start, float rotation, Vector2 origin, float length, float width, Color color)
		{
			Main.spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, rotation, origin, new Vector2(length, width), SpriteEffects.None, 0f);
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
