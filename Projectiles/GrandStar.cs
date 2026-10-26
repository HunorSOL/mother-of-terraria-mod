using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using MotherOfTerraria.Systems;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// One star of the stage 2 GRAND STARS attack (owner design, 2026-10-26: "a bigger and more powerful version of the falling stars, with the same
	// thick coloured indicator lines, coming vertically and horizontally too, so the dodge gap is even smaller"). It works like the GoldCrystalStar
	// (hover with a thick line showing its path, then go) but is 1.6 times the size, and it comes in two kinds:
	//   a FALLING star (ai[2] = 0): hangs above the player, then drops and leaves a burning patch where it lands;
	//   a SIDEWAYS star (ai[2] = +1 or -1): hangs at the edge of the screen, then flies straight across (+1 = to the right), through everything.
	// ai[0] = ticks to hover before it goes, ai[2] = the kind. Uses the same picture as the Gold Crystal Star (no file of its own).
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them). SINGLEPLAYER-ONLY: the patch is made on the owner's machine.
	public class GrandStar : ModProjectile
	{
		private const float Size = 1.6f;                     // against the stage 1 star
		private const float Acceleration = 1.0f;             // pixels per tick, per tick
		private const float MaxSpeed = 21f;
		private const float LineReach = 2400f;               // how far a sideways star's line runs
		private static readonly Vector2 StarCentre = new Vector2(40.5f, 41.5f);

		public override string Texture => "MotherOfTerraria/Projectiles/GoldCrystalStar";

		private bool Going => Projectile.localAI[0] >= Projectile.ai[0];
		private int Kind => (int)Projectile.ai[2];            // 0 falls, +1 flies right, -1 flies left
		private float groundY;

		public override void SetDefaults()
		{
			Projectile.width = (int)(43 * Size);
			Projectile.height = (int)(43 * Size);
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 600;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.localAI[0]++;
			Lighting.AddLight(Projectile.Center, 1.3f, 1.1f, 0.4f);

			if (!Going)
			{
				Projectile.velocity = Vector2.Zero;
				if (Kind == 0 && Projectile.localAI[0] % 6 == 1)
				{
					groundY = GoldCrystalStar.FindGroundY(Projectile.Center);
				}
				return;
			}
			if (Projectile.localAI[0] == Projectile.ai[0])
			{
				SoundEngine.PlaySound(SoundID.Item9 with { Pitch = -0.3f }, Projectile.Center);
				Projectile.timeLeft = 170; // a sideways star has crossed everything long before this
			}
			if (Kind == 0)
			{
				Projectile.velocity.Y = System.Math.Min(Projectile.velocity.Y + Acceleration, MaxSpeed);
				// A star that spawned inside an island falls through it harmlessly and only starts hitting once it is in the open.
				if (!Projectile.tileCollide && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
				{
					Projectile.tileCollide = true;
				}
			}
			else
			{
				Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X + Kind * Acceleration, -MaxSpeed, MaxSpeed);
			}

			// A comet trail streams out behind it.
			Vector2 behind = Kind == 0 ? new Vector2(0f, -1f) : new Vector2(-Kind, 0f);
			for (int i = 0; i < 3; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center + behind * Main.rand.NextFloat(30f, 75f) + Main.rand.NextVector2Circular(28f, 28f),
					Main.rand.NextBool(3) ? DustID.GoldCoin : DustID.GemSapphire, behind * Main.rand.NextFloat(1f, 3f), 0, default, 1.5f);
				dust.noGravity = true;
			}
		}

		// Landing: a burst of light and a burning patch on the ground, like the stage 1 stars.
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Kind == 0 && oldVelocity.Y > 0f && Projectile.owner == Main.myPlayer && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Vector2 centre = new Vector2(Projectile.Center.X, Projectile.Bottom.Y - StarfirePatch.PatchHeight / 2f);
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), centre, Vector2.Zero, ModContent.ProjectileType<StarfirePatch>(),
					(int)(Projectile.damage * StarfirePatch.DamageFraction), 0f, Main.myPlayer);
			}
			return true;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f }, Projectile.Center);
			for (int i = 0; i < 36; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.GemSapphire : DustID.GoldCoin,
					Main.rand.NextVector2Circular(9f, 9f), 0, default, 1.8f);
				dust.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 position = Projectile.Center - Main.screenPosition;
			float scale = 1f;

			if (!Going)
			{
				// The tell: the same thick coloured line as the stage 1 stars (dark edge, gold body, white core), a little wider, showing the path.
				// A sideways star's line fades in late, so the screen is not a mess of lines from the very start.
				float ramp = Projectile.ai[0] <= 0f ? 1f : MathHelper.Clamp(Projectile.localAI[0] / Projectile.ai[0], 0f, 1f);
				float lineFade = Kind == 0 ? 0.55f + 0.45f * ramp : 0.3f + 0.7f * MathHelper.Clamp((Projectile.localAI[0] - (Projectile.ai[0] - 80f)) / 80f, 0f, 1f);
				scale = 1f + 0.12f * (float)System.Math.Sin(Projectile.localAI[0] * 0.35f);
				float pulse = 0.85f + 0.15f * (float)System.Math.Sin(Projectile.localAI[0] * 0.4f);
				Texture2D pixel = TextureAssets.MagicPixel.Value;
				Rectangle one = new Rectangle(0, 0, 1, 1);
				float x = Projectile.Center.X - Main.screenPosition.X;
				float y = Projectile.Center.Y - Main.screenPosition.Y;

				if (Kind == 0)
				{
					float top = System.Math.Max(Projectile.Center.Y, Main.screenPosition.Y - 40f);
					float bottom = System.Math.Min(groundY, Main.screenPosition.Y + Main.screenHeight + 40f);
					if (groundY > Projectile.Center.Y && bottom > top)
					{
						float length = bottom - top;
						float lineY = top - Main.screenPosition.Y;
						Main.spriteBatch.Draw(pixel, new Vector2(x - 9f, lineY), one, new Color(40, 25, 110) * (0.55f * lineFade), 0f, Vector2.Zero, new Vector2(18f, length), SpriteEffects.None, 0f);
						Main.spriteBatch.Draw(pixel, new Vector2(x - 6f, lineY), one, new Color(255, 205, 60) * (0.85f * lineFade * pulse), 0f, Vector2.Zero, new Vector2(12f, length), SpriteEffects.None, 0f);
						Main.spriteBatch.Draw(pixel, new Vector2(x - 2f, lineY), one, new Color(255, 250, 215) * lineFade, 0f, Vector2.Zero, new Vector2(4f, length), SpriteEffects.None, 0f);
						if (groundY < Main.screenPosition.Y + Main.screenHeight + 200f && HeavensSkyTextures.Ready)
						{
							Texture2D glow = HeavensSkyTextures.Soft;
							Vector2 spot = new Vector2(x, groundY - Main.screenPosition.Y);
							float size = (230f + 150f * ramp) * pulse;
							Main.spriteBatch.End();
							Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None,
								RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
							Main.spriteBatch.Draw(glow, spot, null, new Color(255, 200, 70, 0) * (0.5f + 0.5f * ramp), 0f, glow.Size() / 2f,
								new Vector2(size, size * 0.35f) / glow.Width, SpriteEffects.None, 0f);
							Main.spriteBatch.End();
							Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None,
								RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
						}
					}
				}
				else
				{
					// From the star across to the far edge of the screen (only the part on the screen matters).
					float start = Kind > 0 ? x : x - LineReach;
					float length = LineReach;
					Main.spriteBatch.Draw(pixel, new Vector2(start, y - 9f), one, new Color(40, 25, 110) * (0.55f * lineFade), 0f, Vector2.Zero, new Vector2(length, 18f), SpriteEffects.None, 0f);
					Main.spriteBatch.Draw(pixel, new Vector2(start, y - 6f), one, new Color(255, 205, 60) * (0.85f * lineFade * pulse), 0f, Vector2.Zero, new Vector2(length, 12f), SpriteEffects.None, 0f);
					Main.spriteBatch.Draw(pixel, new Vector2(start, y - 2f), one, new Color(255, 250, 215) * lineFade, 0f, Vector2.Zero, new Vector2(length, 4f), SpriteEffects.None, 0f);
				}
			}

			float fadeIn = MathHelper.Clamp(Projectile.localAI[0] / 15f, 0f, 1f);
			float pictureScale = 43f / 52f * Size; // the picture matches the hitbox, as for the stage 1 star
			Main.spriteBatch.Draw(texture, position, null, Color.White * fadeIn, 0f, StarCentre, scale * pictureScale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool? CanHitNPC(NPC target) => false;
		public override bool CanHitPlayer(Player target) => Going && Projectile.localAI[0] > Projectile.ai[0] + 2f;
	}
}
