using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// A feather from the Mother of Terraria's FEATHER STORM. It flies straight, starting slowly and speeding up a little,
	// passes through walls and islands (it is a bullet pattern, not a physical object), and fades after a few seconds.
	// The storm is made of many of them thrown from a turning spiral, so the pattern is what you dodge.
	// Texture: Projectiles/GoldenFeather.png (the owner's design, 25x80, pointing UP; made by tools/make_feather.py).
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them).
	public class GoldenFeather : ModProjectile
	{
		private const float Acceleration = 1.012f;  // speed multiplier per tick, until MaxSpeed
		private const float MaxSpeed = 11f;

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 420;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			float speed = Projectile.velocity.Length();
			if (speed < MaxSpeed)
			{
				Projectile.velocity *= Acceleration;
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Lighting.AddLight(Projectile.Center, 0.5f, 0.42f, 0.15f);
			if (Main.rand.NextBool(6))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, Projectile.velocity * -0.05f, 0, default, 0.9f);
				dust.noGravity = true;
			}
		}

		// A long thin shape: check a short line along the feather instead of the 14x14 box.
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
			float unused = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
				Projectile.Center - direction * 30f, Projectile.Center + direction * 30f, 10f, ref unused);
		}

		public override Color? GetAlpha(Color lightColor) => Color.White; // glows: ignores the room's darkness
	}
}
