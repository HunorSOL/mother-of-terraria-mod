using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// The patch of burning golden light a Gold Crystal Star leaves where it lands. It stays for a few seconds and hurts
	// anyone standing in it (it keeps hitting, as often as the player's hit-immunity lets it), so the safe ground shrinks
	// as the Falling Stars attack goes on. It lights up, burns, then fades (it only hurts while clearly lit).
	// Drawn entirely in code from flickering flame strips, no picture file.
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them).
	public class StarfirePatch : ModProjectile
	{
		public const float PatchWidth = 96f;
		public const float PatchHeight = 44f;
		public const float DamageFraction = 0.4f;   // the patch hurts 40% as much as the star's direct hit
		private const int LifeTicks = 60 * 4;
		private const int LightUpTicks = 20;
		private const int FadeTicks = 45;

		// A patch needs no picture file: borrow a vanilla projectile texture (never drawn).
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;

		private float Glow
		{
			get
			{
				float age = LifeTicks - Projectile.timeLeft;
				return System.Math.Min(MathHelper.Clamp(age / LightUpTicks, 0f, 1f), MathHelper.Clamp(Projectile.timeLeft / (float)FadeTicks, 0f, 1f));
			}
		}

		public override void SetDefaults()
		{
			Projectile.width = (int)PatchWidth;
			Projectile.height = (int)PatchHeight;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = LifeTicks;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			Lighting.AddLight(Projectile.Center, 1.0f * Glow, 0.75f * Glow, 0.25f * Glow);
			if (Glow > 0.3f && Main.rand.NextBool(3))
			{
				Vector2 position = Projectile.position + new Vector2(Main.rand.NextFloat(PatchWidth), PatchHeight - 4f);
				Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool(3) ? DustID.GoldCoin : DustID.Torch, new Vector2(0f, Main.rand.NextFloat(-3f, -1f)), 0, default, 1.5f);
				dust.noGravity = true;
			}
		}

		public override bool CanHitPlayer(Player target) => Glow > 0.5f;

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			float glow = Glow;
			Vector2 baseLeft = new Vector2(Projectile.position.X, Projectile.position.Y + PatchHeight) - Main.screenPosition;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			// A soft pool of light along the ground, then flame strips of changing height standing on it.
			Rectangle one = new Rectangle(0, 0, 1, 1);
			Main.spriteBatch.Draw(pixel, baseLeft + new Vector2(0f, -10f), one, new Color(255, 170, 40) * (0.35f * glow), 0f, Vector2.Zero, new Vector2(PatchWidth, 10f), SpriteEffects.None, 0f);
			const int strips = 13;
			float stripWidth = PatchWidth / strips;
			for (int i = 0; i < strips; i++)
			{
				float flicker = 0.55f + 0.45f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 11f + i * 1.7f + Projectile.whoAmI);
				float edge = 1f - System.Math.Abs(i - (strips - 1) / 2f) / (strips / 2f) * 0.5f; // taller in the middle
				float height = PatchHeight * flicker * edge * glow;
				Vector2 at = baseLeft + new Vector2(i * stripWidth, 0f);
				Main.spriteBatch.Draw(pixel, at, one, new Color(255, 190, 50) * (0.55f * glow), 0f, new Vector2(0f, 1f), new Vector2(stripWidth - 1f, height), SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(pixel, at + new Vector2(stripWidth * 0.2f, 0f), one, new Color(255, 245, 200) * (0.6f * glow), 0f, new Vector2(0f, 1f), new Vector2(stripWidth * 0.5f, height * 0.55f), SpriteEffects.None, 0f);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
