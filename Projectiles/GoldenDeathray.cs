using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// The Mother of Terraria's laser: a huge golden beam like the Moon Lord's Phantasmal Deathray.
	// It is fixed to the boss's centre and has two parts:
	//  1. THE TELL (about 1.25 s): a thin pale line that follows the player (it turns at a limited speed). It does no damage.
	//  2. THE BEAM (about 2.5 s): the line locks, swells into a big gold beam and sweeps slowly sideways. It does huge
	//     damage and sets the player on Hellfire.
	// ai[0] = the boss's NPC index, ai[1] = the beam's current angle (radians), ai[2] = which way it sweeps (+1 or -1).
	// The owner machine (server / singleplayer) turns the beam and syncs the angle every tick (netUpdate).
	// There is no texture of its own: it is drawn from the game's 1x1 white "magic pixel" in PreDraw.
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them).
	public class GoldenDeathray : ModProjectile
	{
		public const int TellTicks = 75;
		public const int BeamTicks = 150;
		private const float TrackTurnPerTick = 0.035f;  // how fast the tell line follows the player (radians per tick)
		private const float SweepPerTick = 0.0085f;     // how fast the locked beam sweeps (about 29 degrees a second)
		private const float BeamLength = 2600f;         // long enough to cross the screen
		private const float BeamHitWidth = 70f;         // the damaging thickness (pixels)
		private const float BeamDrawWidth = 130f;       // the glowing outer thickness
		private const int HellfireTicks = 60 * 6;

		// A beam is only an effect, so it needs no picture of its own: borrow a vanilla projectile texture (never drawn).
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;

		private NPC Owner => Main.npc[(int)Projectile.ai[0]];
		private bool Firing => Projectile.localAI[0] >= TellTicks;
		private float Strength // 0 -> 1 as the beam swells in, 1 -> 0 as it fades out
		{
			get
			{
				float age = Projectile.localAI[0];
				float swellIn = MathHelper.Clamp((age - TellTicks) / 12f, 0f, 1f);
				float fadeOut = MathHelper.Clamp((TellTicks + BeamTicks - age) / 12f, 0f, 1f);
				return System.Math.Min(swellIn, fadeOut);
			}
		}

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = TellTicks + BeamTicks;
			Projectile.aiStyle = -1;
			Projectile.hide = false;
		}

		public override void AI()
		{
			NPC owner = Owner;
			if (!owner.active || owner.life <= 0)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Center = owner.Center;
			Projectile.velocity = Vector2.Zero;
			Projectile.localAI[0]++;

			if (Projectile.owner == Main.myPlayer)
			{
				if (!Firing)
				{
					// The tell: swing the line toward the nearest player.
					Player target = Main.player[owner.target];
					if (target.active && !target.dead)
					{
						float wanted = (target.Center - Projectile.Center).ToRotation();
						float turn = MathHelper.Clamp(MathHelper.WrapAngle(wanted - Projectile.ai[1]), -TrackTurnPerTick, TrackTurnPerTick);
						Projectile.ai[1] += turn;
					}
				}
				else
				{
					Projectile.ai[1] += Projectile.ai[2] * SweepPerTick;
				}
				Projectile.netUpdate = true;
			}

			Lighting.AddLight(Projectile.Center, 1.4f, 1.1f, 0.4f);

			if (Projectile.localAI[0] == TellTicks)
			{
				SoundEngine.PlaySound(SoundID.Zombie104 with { Volume = 0.8f }, Projectile.Center); // a low boom as it fires
			}

			if (Firing)
			{
				// Sparks along the beam.
				Vector2 direction = Projectile.ai[1].ToRotationVector2();
				for (int i = 0; i < 3; i++)
				{
					Vector2 position = Projectile.Center + direction * Main.rand.NextFloat(80f, 1400f);
					Dust dust = Dust.NewDustPerfect(position, DustID.GoldCoin, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.6f);
					dust.noGravity = true;
				}
			}
		}

		// Only hurts while the beam is really on, and only along its (thick) line, not in a box around the boss.
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!Firing || Strength < 0.5f)
			{
				return false;
			}
			Vector2 direction = Projectile.ai[1].ToRotationVector2();
			float unused = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
				Projectile.Center, Projectile.Center + direction * BeamLength, BeamHitWidth * Strength, ref unused);
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.AddBuff(BuffID.OnFire3, HellfireTicks);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Vector2 start = Projectile.Center - Main.screenPosition;
			float rotation = Projectile.ai[1];
			Vector2 origin = new Vector2(0f, 0.5f); // the pixel's left-middle: it grows rightward from the boss

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			if (!Firing)
			{
				// The tell: a thin line that gets brighter as the beam gets near.
				float ramp = Projectile.localAI[0] / TellTicks;
				Draw(pixel, start, rotation, origin, BeamLength, 4f + 6f * ramp, new Color(255, 230, 140) * (0.25f + 0.5f * ramp));
			}
			else
			{
				float strength = Strength;
				float pulse = 1f + 0.06f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 40f);
				Draw(pixel, start, rotation, origin, BeamLength, BeamDrawWidth * strength * pulse, new Color(255, 170, 30) * 0.30f);  // outer glow
				Draw(pixel, start, rotation, origin, BeamLength, BeamDrawWidth * 0.62f * strength * pulse, new Color(255, 205, 70) * 0.55f); // gold body
				Draw(pixel, start, rotation, origin, BeamLength, BeamDrawWidth * 0.28f * strength, new Color(255, 250, 220) * 0.95f);       // white-hot core
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		private static void Draw(Texture2D pixel, Vector2 start, float rotation, Vector2 origin, float length, float width, Color color)
		{
			Main.spriteBatch.Draw(pixel, start, null, color, rotation, origin, new Vector2(length, width), SpriteEffects.None, 0f);
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
