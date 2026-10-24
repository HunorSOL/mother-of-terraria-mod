using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// One ring of the Mother of Terraria's HALO RINGS attack: a thin golden ring that spreads outward from her, like a ripple.
	// It hurts anything touching the ring, EXCEPT inside its one GAP (an arc of 45 degrees). The gap is marked by bright caps
	// on both edges, so a hit is the player's mistake.
	// ai[0] = the direction of the middle of the gap (radians). The ring is centred where she was when it was thrown.
	// It is drawn in code (a ring of short bars, no picture file).
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them).
	public class HaloRing : ModProjectile
	{
		public const float GapWidth = MathHelper.Pi / 4f;   // 45 degrees (owner, 2026-10-24)
		private const float StartRadius = 90f;
		private const float Speed = 6f;                     // pixels per tick: slow enough to read (owner, 2026-10-24)
		private const float MaxRadius = 1700f;
		private const float HitHalfThickness = 14f;         // how thick the damaging band is (each side of the middle)
		private const float PlayerReach = 12f;              // roughly the player's half size
		private const float SegmentLength = 12f;            // the ring is drawn as short bars this long

		// A ring needs no picture file: borrow a vanilla projectile texture (never drawn).
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;

		private float Radius => StartRadius + Speed * Projectile.localAI[0];
		private float GapAngle => Projectile.ai[0];

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = (int)((MaxRadius - StartRadius) / Speed) + 2;
		}

		public override void AI()
		{
			Projectile.localAI[0]++;
			Projectile.velocity = Vector2.Zero;
			Lighting.AddLight(Projectile.Center + GapAngle.ToRotationVector2() * Radius, 0.9f, 0.75f, 0.3f);
		}

		// A player is hit when their middle is within the ring's band, and not inside the gap (with a little leniency at the gap's edges).
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 offset = targetHitbox.Center.ToVector2() - Projectile.Center;
			float distance = offset.Length();
			if (distance < 1f || System.Math.Abs(distance - Radius) > HitHalfThickness + PlayerReach)
			{
				return false;
			}
			float angleFromGap = System.Math.Abs(MathHelper.WrapAngle(offset.ToRotation() - GapAngle));
			float leniency = PlayerReach / distance; // a player standing half in the gap is not hit
			return angleFromGap > GapWidth / 2f + leniency;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle one = new Rectangle(0, 0, 1, 1);
			float radius = Radius;
			Vector2 centre = Projectile.Center - Main.screenPosition;
			float fadeIn = MathHelper.Clamp(Projectile.localAI[0] / 10f, 0f, 1f);
			float fadeOut = MathHelper.Clamp((MaxRadius - radius) / 300f, 0f, 1f);
			float strength = fadeIn * fadeOut;

			int segments = System.Math.Max(24, (int)(MathHelper.TwoPi * radius / SegmentLength));
			float step = MathHelper.TwoPi / segments;
			float screenReach = new Vector2(Main.screenWidth, Main.screenHeight).Length() / 2f + 80f;

			// The ring and its gap caps are drawn with ordinary blending (a dark edge, a gold body, a white core): a glow added on top of the bright sky would hardly show.
			for (int i = 0; i < segments; i++)
			{
				float angle = i * step;
				float fromGap = System.Math.Abs(MathHelper.WrapAngle(angle - GapAngle));
				if (fromGap < GapWidth / 2f)
				{
					continue; // the gap
				}
				Vector2 pos = centre + angle.ToRotationVector2() * radius;
				// skip bars that are far off the screen
				if ((Projectile.Center + angle.ToRotationVector2() * radius - Main.screenPosition - new Vector2(Main.screenWidth, Main.screenHeight) / 2f).Length() > screenReach)
				{
					continue;
				}
				bool cap = fromGap < GapWidth / 2f + 0.07f * (500f / System.Math.Max(radius, 200f)); // the bars next to the gap glow brighter
				float rotation = angle + MathHelper.PiOver2; // the bar lies along the ring
				float length = SegmentLength * 1.35f;
				Vector2 origin = new Vector2(0.5f, 0.5f);
				Main.spriteBatch.Draw(pixel, pos, one, new Color(40, 25, 110) * (0.6f * strength), rotation, origin, new Vector2(length, 17f), SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(pixel, pos, one, (cap ? new Color(255, 245, 170) : new Color(255, 205, 60)) * (0.9f * strength), rotation, origin, new Vector2(length, cap ? 14f : 10f), SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(pixel, pos, one, new Color(255, 252, 225) * strength, rotation, origin, new Vector2(length, cap ? 7f : 4f), SpriteEffects.None, 0f);
			}
			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
