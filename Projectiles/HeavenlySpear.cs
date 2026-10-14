using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// The spear thrown by the Heavenly Knight: flies nearly straight but curves a little toward the nearest
	// player (weak homing), hits hard, and is destroyed by the first thing it hits. A ModProjectile is a flying object spawned by an NPC, item or boss.
	// Texture: Projectiles/HeavenlySpear.png (owner's design, 22x82), drawn pointing UP; the code rotates it to face its direction.
	public class HeavenlySpear : ModProjectile
	{
		// Weak homing (placeholder numbers, to tune): it can bend about 1.8 degrees per tick (108 degrees a second),
		// only for the first 1.5 s, and stops steering once it is close, so a late sidestep still works
		// but simply jumping over a straight shot does not.
		private const float TurnPerTick = MathHelper.Pi / 100f;
		private const int HomingTicks = 90;
		private const float StopHomingDistance = 140f;

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

			Home();

			// A trail of golden and blue sparkles behind the spear.
			if (Main.rand.NextBool(2))
			{
				int type = Main.rand.NextBool() ? DustID.GoldCoin : DustID.GemSapphire;
				Dust dust = Dust.NewDustPerfect(Projectile.Center, type, Projectile.velocity * -0.1f, 0, default, 1.1f);
				dust.noGravity = true;
			}
		}

		// Steer the velocity slightly toward the closest living player. Runs where the projectile is owned
		// (the server, or singleplayer); the new direction is sent to other clients every few ticks.
		private void Home()
		{
			if (Projectile.owner != Main.myPlayer || Projectile.timeLeft < 240 - HomingTicks)
			{
				return;
			}

			Player target = null;
			float best = 1400f;
			foreach (Player player in Main.player)
			{
				if (player.active && !player.dead)
				{
					float distance = Vector2.Distance(player.Center, Projectile.Center);
					if (distance < best)
					{
						best = distance;
						target = player;
					}
				}
			}
			if (target == null || best < StopHomingDistance)
			{
				return;
			}

			float speed = Projectile.velocity.Length();
			float current = Projectile.velocity.ToRotation();
			float wanted = (target.Center - Projectile.Center).ToRotation();
			float turn = MathHelper.Clamp(MathHelper.WrapAngle(wanted - current), -TurnPerTick, TurnPerTick);
			Projectile.velocity = (current + turn).ToRotationVector2() * speed;

			if (Projectile.timeLeft % 5 == 0)
			{
				Projectile.netUpdate = true;
			}
		}

		// The spear is long and thin (about 80 px), so check a line along its length instead of the small 16x16 box.
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
			float unused = 0f;
			return Collision.CheckAABBvLineCollision(
				targetHitbox.TopLeft(), targetHitbox.Size(),
				Projectile.Center - direction * 36f, Projectile.Center + direction * 36f,
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
