using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// The spear thrown by the Heavenly Knight: flies in a straight line, hits hard, and is destroyed by
	// the first thing it hits. A ModProjectile is a flying object spawned by an NPC, item or boss.
	// Texture: Projectiles/HeavenlySpear.png, drawn pointing UP; the code rotates it to face its direction.
	public class HeavenlySpear : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;   // hurts players, not enemies
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 240;   // disappears after 4 seconds if it hits nothing
			Projectile.aiStyle = -1;     // our own AI below, not a vanilla one
		}

		public override void AI()
		{
			// Point the (upward-drawn) sprite along the direction of travel.
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Lighting.AddLight(Projectile.Center, 0.3f, 0.6f, 0.9f);

			// A trail of golden and blue sparkles behind the spear.
			if (Main.rand.NextBool(2))
			{
				int type = Main.rand.NextBool() ? DustID.GoldCoin : DustID.GemSapphire;
				Dust dust = Dust.NewDustPerfect(Projectile.Center, type, Projectile.velocity * -0.1f, 0, default, 1.1f);
				dust.noGravity = true;
			}
		}

		// The spear is long and thin, so check a line along its length instead of the small 16x16 box.
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
			float unused = 0f;
			return Collision.CheckAABBvLineCollision(
				targetHitbox.TopLeft(), targetHitbox.Size(),
				Projectile.Center - direction * 24f, Projectile.Center + direction * 24f,
				14f, ref unused);
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			for (int i = 0; i < 12; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					i % 2 == 0 ? DustID.GoldCoin : DustID.GemSapphire, 0f, 0f, 0, default, 1.3f);
				dust.velocity *= 2f;
				dust.noGravity = true;
			}
		}
	}
}
