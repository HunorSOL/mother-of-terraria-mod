using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// The line and the afterglow of ONE dash of the Mother of Terraria's MIRROR DASH attack (stage 2; see MotherOfTerrariaBoss.DoMirrorDash). Three stages:
	//   1. FOLLOW (FollowTicks): a thin line runs from where she stands through the player and keeps pointing at them (no damage).
	//   2. LOCK (LockTicks): the line stops, turns into a thick corridor the width of her body and flashes. This is the time to step out of it.
	//   3. DASH (DashTicks): she charges down the corridor (her body does the big damage; this projectile only draws the trail behind her).
	//   4. AFTERGLOW (TrailTicks): the corridor she crossed burns for a moment and hurts a little if you stand in it.
	// ai[0] = the boss's NPC index. ai[1] = the locked angle (set at the lock). localAI[0] = age in ticks. The boss keeps the follow angle in its localAI[1].
	// Drawn in code, no picture. The boss and this class share the timings (public constants). ALL NUMBERS ARE PLACEHOLDERS (the owner will tune them).
	// SINGLEPLAYER-ONLY: the follow angle is passed through NPC.localAI, which is not synced.
	public class MirrorDashLine : ModProjectile
	{
		public const int FollowTicks = 40;
		public const int LockTicks = 36;
		public const int DashTicks = 30;
		public const int TrailTicks = 90;
		public const int TotalTicks = FollowTicks + LockTicks + DashTicks + TrailTicks;
		public const float DashDistance = 1500f;      // how far she travels along the line (the player is about 650 px from the start)
		public const float CorridorHalfWidth = 85f;   // the corridor is a little wider than her 130 px body, so the line is honest

		private int Age => (int)Projectile.localAI[0];
		private bool Dashing => Age >= FollowTicks + LockTicks;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3000;
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
			Projectile.timeLeft = TotalTicks + 5;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.localAI[0]++;
			Projectile.velocity = Vector2.Zero;
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (!owner.active && Age < FollowTicks + LockTicks)
			{
				Projectile.Kill();
				return;
			}
			if (Age < FollowTicks)
			{
				Projectile.ai[1] = owner.localAI[1]; // the boss keeps pointing the line at the player
			}
			if (Age == FollowTicks)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f }, Projectile.Center); // the lock
			}
			if (Dashing && Main.rand.NextBool(2) && Age < FollowTicks + LockTicks + DashTicks + TrailTicks / 2)
			{
				float reach = DashDistance * Progress();
				Vector2 point = Projectile.Center + Projectile.ai[1].ToRotationVector2() * Main.rand.NextFloat(0f, reach)
					+ Main.rand.NextVector2Circular(CorridorHalfWidth, CorridorHalfWidth);
				Dust dust = Dust.NewDustPerfect(point, DustID.GoldCoin, new Vector2(0f, -Main.rand.NextFloat(0.5f, 2f)), 0, default, 1.2f);
				dust.noGravity = true;
			}
			Lighting.AddLight(Projectile.Center + Projectile.ai[1].ToRotationVector2() * DashDistance * 0.5f * Progress(), 0.9f, 0.75f, 0.3f);
			if (Age >= TotalTicks)
			{
				Projectile.Kill();
			}
		}

		// How much of the corridor she has crossed so far (0 before the dash, 1 when she has arrived).
		private float Progress() => Dashing ? MathHelper.Clamp((Age - FollowTicks - LockTicks) / (float)DashTicks, 0f, 1f) : 0f;

		// Only the corridor she has already crossed hurts (the afterglow); the big damage is her own body (the boss raises its contact damage for the dash).
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!Dashing)
			{
				return false;
			}
			Vector2 end = Projectile.Center + Projectile.ai[1].ToRotationVector2() * DashDistance * Progress();
			float unused = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, end, CorridorHalfWidth * 2f, ref unused);
		}

		public override bool CanHitPlayer(Player target) => Dashing;

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle one = new Rectangle(0, 0, 1, 1);
			float age = Age;
			float angle = Projectile.ai[1];
			Vector2 start = Projectile.Center - Main.screenPosition;
			Vector2 side = (angle + MathHelper.PiOver2).ToRotationVector2();
			Vector2 mid = new Vector2(0f, 0.5f); // origin: the middle of the left end of the 1x1 pixel, so it rotates about the line's own axis

			if (age < FollowTicks)
			{
				// the thin tell line: it follows the player, brightening as the lock comes
				float follow01 = age / FollowTicks;
				Main.spriteBatch.Draw(pixel, start, one, new Color(255, 205, 60) * (0.25f + 0.35f * follow01), angle, mid, new Vector2(DashDistance, 6f), SpriteEffects.None, 0f);
			}
			else if (age < FollowTicks + LockTicks)
			{
				// the locked corridor: indigo edge, gold body, bright edge lines (the same style as the Falling Stars' and Judgment Mark's lines)
				float lock01 = (age - FollowTicks) / LockTicks;
				float pulse = 0.85f + 0.15f * (float)System.Math.Sin(age * 0.5f);
				float fade = 0.35f + 0.55f * lock01;
				float width = CorridorHalfWidth * 2f;
				Main.spriteBatch.Draw(pixel, start, one, new Color(40, 25, 110) * (0.4f * fade), angle, mid, new Vector2(DashDistance, width + 10f), SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(pixel, start, one, new Color(255, 205, 60) * (0.28f * fade * pulse), angle, mid, new Vector2(DashDistance, width), SpriteEffects.None, 0f);
				for (int s = -1; s <= 1; s += 2)
				{
					Main.spriteBatch.Draw(pixel, start + side * (s * width / 2f), one, new Color(255, 205, 60) * (0.9f * fade * pulse), angle, mid, new Vector2(DashDistance, 8f), SpriteEffects.None, 0f);
				}
			}
			else
			{
				// the afterglow: the part she has crossed, fading out
				float since = age - FollowTicks - LockTicks - DashTicks;
				float fade = since <= 0f ? 1f : MathHelper.Clamp(1f - since / TrailTicks, 0f, 1f);
				float length = DashDistance * Progress();
				float width = CorridorHalfWidth * 2f;
				Main.spriteBatch.Draw(pixel, start, one, new Color(255, 215, 90) * (0.55f * fade), angle, mid, new Vector2(length, width), SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(pixel, start, one, new Color(255, 250, 220) * (0.8f * fade), angle, mid, new Vector2(length, width * 0.3f), SpriteEffects.None, 0f);
			}
			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
