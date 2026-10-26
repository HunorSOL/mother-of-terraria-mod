using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// A Starlight Lance dragged toward the Gravity Well (see GravityWell): it starts on a ring around the well and spirals inward, turning
	// as it goes, until it is swallowed by the core. It does not home on the player. ai[0], ai[1] = the well's position (world x and y).
	// Texture: the Starlight Lance picture (no file of its own). ALL NUMBERS ARE PLACEHOLDERS.
	public class GravityLance : ModProjectile
	{
		private const float Speed = 6.5f;
		private const float Inward = 0.85f;        // how much of its heading points at the well ...
		private const float Sideways = 0.5f;       // ... and how much goes round it: this makes the spiral
		private const float Turn = 0.12f;          // how fast it follows that heading
		private const float Swallowed = 36f;       // it vanishes this close to the core
		private const float HalfLength = 46f;

		public override string Texture => "MotherOfTerraria/Projectiles/StarlightLance";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 330;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Vector2 well = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Vector2 toWell = well - Projectile.Center;
			if (toWell.Length() < Swallowed)
			{
				Projectile.Kill();
				return;
			}
			Vector2 inward = toWell.SafeNormalize(Vector2.UnitY);
			Vector2 around = new Vector2(-inward.Y, inward.X);
			Vector2 heading = (inward * Inward + around * Sideways).SafeNormalize(Vector2.UnitY) * Speed;
			if (Projectile.velocity == Vector2.Zero)
			{
				Projectile.velocity = heading;
			}
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, heading, Turn);
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Lighting.AddLight(Projectile.Center, 0.8f, 0.7f, 0.3f);
			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 30f, Main.rand.NextBool(3) ? DustID.GemAmethyst : DustID.GoldCoin, Vector2.Zero, 0, default, 1.2f);
				dust.noGravity = true;
			}
		}

		// The lance is long and thin: check a line along its length.
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
			float unused = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
				Projectile.Center - direction * HalfLength, Projectile.Center + direction * HalfLength, 14f, ref unused);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			float fade = MathHelper.Clamp((330 - Projectile.timeLeft) / 12f, 0f, 1f);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, texture.Size() / 2f, 1f, SpriteEffects.None, 0f);
			return false;
		}
	}
}
