using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// A wall of golden light from the Mother of Terraria's WING WALL attack. It is a tall vertical wall that sweeps across
	// the arena sideways with ONE GAP in it. You have to be level with the gap when it passes.
	//  1. THE TELL (ai[2] ticks): the wall waits at the edge of the screen, faint, with the gap marked by bright caps.
	//     It does no damage yet. Several walls can be waiting at once, each with its own delay, so they come one after another.
	//  2. THE SWEEP: it turns solid and crosses at a steady speed. Anything touching the bright part hurts a lot.
	// ai[0] = direction (+1 moves right, -1 moves left), ai[1] = the height of the middle of the gap (world pixels),
	// ai[2] = ticks to wait before it starts moving.
	// Drawn entirely in code from the game's white pixel (no picture file).
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them).
	public class LightWall : ModProjectile
	{
		public const float Speed = 12f;
		public const float TravelDistance = 2300f;     // how far it crosses before it disappears
		private const float HalfGap = 115f;            // half the height of the opening
		private const float HalfHeight = 1000f;        // the wall reaches this far above and below the gap
		private const float HitHalfWidth = 22f;
		private const float DrawHalfWidth = 38f;

		// A wall needs no picture file: borrow a vanilla projectile texture (never drawn).
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;

		private float Direction => Projectile.ai[0];
		private float GapY => Projectile.ai[1];
		private float Delay => Projectile.ai[2];
		private bool Moving => Projectile.localAI[0] >= Delay;

		public override void SetDefaults()
		{
			Projectile.width = (int)(HitHalfWidth * 2f);
			Projectile.height = (int)(HalfHeight * 2f);
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 60 * 30; // set properly in AI once the delay is known
		}

		public override void AI()
		{
			Projectile.localAI[0]++;
			if (Projectile.localAI[0] == 1f)
			{
				Projectile.timeLeft = (int)(Delay + TravelDistance / Speed) + 2;
			}

			Projectile.velocity = Moving ? new Vector2(Direction * Speed, 0f) : Vector2.Zero;
			if (Projectile.localAI[0] == Delay)
			{
				SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f }, Projectile.Center); // the wall starts to move
			}

			Lighting.AddLight(new Vector2(Projectile.Center.X, GapY - 200f), 1.1f, 0.9f, 0.35f);
			Lighting.AddLight(new Vector2(Projectile.Center.X, GapY + 200f), 1.1f, 0.9f, 0.35f);

			if (Moving && Main.rand.NextBool(2))
			{
				// Sparks flying off the leading face and falling sideways behind it.
				float y = GapY + Main.rand.NextFloat(-500f, 500f);
				if (System.Math.Abs(y - GapY) > HalfGap)
				{
					Dust dust = Dust.NewDustPerfect(new Vector2(Projectile.Center.X, y), DustID.GoldCoin, new Vector2(-Direction * 2f, Main.rand.NextFloat(-1f, 1f)), 0, default, 1.3f);
					dust.noGravity = true;
				}
			}
		}

		// Two solid slabs: above the gap and below it. The gap itself is safe. Nothing hurts during the tell.
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!Moving)
			{
				return false;
			}
			float x = Projectile.Center.X - HitHalfWidth;
			Rectangle upper = new Rectangle((int)x, (int)(GapY - HalfHeight), (int)(HitHalfWidth * 2f), (int)(HalfHeight - HalfGap));
			Rectangle lower = new Rectangle((int)x, (int)(GapY + HalfGap), (int)(HitHalfWidth * 2f), (int)(HalfHeight - HalfGap));
			return targetHitbox.Intersects(upper) || targetHitbox.Intersects(lower);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle one = new Rectangle(0, 0, 1, 1);
			Vector2 centre = Projectile.Center - Main.screenPosition;
			float gapScreenY = GapY - Main.screenPosition.Y;
			float pulse = 0.5f + 0.5f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 14f);
			// During the tell the wall is faint and breathing; once it moves it is bright.
			float strength = Moving ? 1f : 0.18f + 0.14f * pulse;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			// Upper and lower slabs: a wide soft glow, a gold body and a white-hot core.
			for (int part = 0; part < 2; part++)
			{
				float top = part == 0 ? gapScreenY - HalfHeight : gapScreenY + HalfGap;
				float height = HalfHeight - HalfGap;
				Slab(pixel, one, centre.X, top, height, DrawHalfWidth, new Color(255, 170, 40) * (0.30f * strength));
				Slab(pixel, one, centre.X, top, height, DrawHalfWidth * 0.62f, new Color(255, 210, 80) * (0.55f * strength));
				Slab(pixel, one, centre.X, top, height, DrawHalfWidth * 0.25f, new Color(255, 250, 225) * (0.9f * strength));
			}

			// Bright caps on the edges of the gap, so the opening is easy to see (always, tell included).
			float capStrength = Moving ? 1f : 0.7f + 0.3f * pulse;
			for (int side = -1; side <= 1; side += 2)
			{
				float y = gapScreenY + side * HalfGap;
				Main.spriteBatch.Draw(pixel, new Vector2(centre.X - DrawHalfWidth * 1.4f, y - 3f), one, new Color(255, 245, 190) * (0.9f * capStrength),
					0f, Vector2.Zero, new Vector2(DrawHalfWidth * 2.8f, 6f), SpriteEffects.None, 0f);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		private static void Slab(Texture2D pixel, Rectangle one, float centreX, float top, float height, float halfWidth, Color color)
		{
			Main.spriteBatch.Draw(pixel, new Vector2(centreX - halfWidth, top), one, color, 0f, Vector2.Zero, new Vector2(halfWidth * 2f, height), SpriteEffects.None, 0f);
		}

		public override bool ShouldUpdatePosition() => true;
	}
}
