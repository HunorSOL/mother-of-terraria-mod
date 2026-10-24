using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// One star of the Mother of Terraria's FALLING STARS attack (the owner's "Gold Crystal Star").
	// 1. It appears high above the arena and HOVERS, pulsing, with a thin golden line down to the ground where it will land (the tell).
	// 2. When its delay is over it FALLS, faster and faster. The stars are given increasing delays, so they fall in a sweep (a rhythm).
	// 3. Where it lands it explodes and leaves a burning patch of light (StarfirePatch) that stays for a few seconds.
	// ai[0] = ticks to hover before falling, ai[1] = 1 for the "tracking" star that follows the player's sideways position
	// while it hovers and locks 25 ticks before it falls.
	// Texture: Projectiles/GoldCrystalStar.png (81x83, the star only; made by tools/make_gold_star.py). The comet trail is made of sparks while it falls.
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them). SINGLEPLAYER-ONLY: the patch is made on the owner's machine.
	public class GoldCrystalStar : ModProjectile
	{
		private const float FallAcceleration = 0.9f;  // pixels per tick, per tick
		private const float MaxFallSpeed = 22f;
		private const int LockBeforeFall = 25;        // the tracking star stops following this long before it drops
		private const float FollowSpeed = 9f;
		private static readonly Vector2 StarCentre = new Vector2(40.5f, 41.5f); // the middle of the 81x83 picture (the star only; the game draws the trail as sparks)

		private bool Falling => Projectile.localAI[0] >= Projectile.ai[0];
		private float groundY; // where the line to the ground ends (a world position)

		public override void SetDefaults()
		{
			Projectile.width = 52;
			Projectile.height = 52;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false; // switched on once it is falling and out of any island it spawned inside
			Projectile.timeLeft = 600;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Projectile.localAI[0]++;
			Lighting.AddLight(Projectile.Center, 1f, 0.85f, 0.3f);

			if (!Falling)
			{
				Projectile.velocity = Vector2.Zero;
				// The tracking star slides toward the player's sideways position, until just before it drops.
				if (Projectile.ai[1] == 1f && Projectile.owner == Main.myPlayer && Projectile.localAI[0] < Projectile.ai[0] - LockBeforeFall)
				{
					Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
					float dx = MathHelper.Clamp(target.Center.X - Projectile.Center.X, -FollowSpeed, FollowSpeed);
					Projectile.position.X += dx;
					Projectile.netUpdate = true;
				}
				groundY = FindGroundY();
				return;
			}

			if (Projectile.localAI[0] == Projectile.ai[0])
			{
				SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
			}
			Projectile.velocity.Y = System.Math.Min(Projectile.velocity.Y + FallAcceleration, MaxFallSpeed);
			Projectile.rotation = 0f;

			// A star that spawned inside an island falls through it harmlessly and only starts hitting once it is in the open.
			if (!Projectile.tileCollide && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
			{
				Projectile.tileCollide = true;
			}

			// A comet trail of blue and gold sparks streaming up behind it.
			for (int i = 0; i < 2; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-18f, 18f), Main.rand.NextFloat(-45f, -20f)),
					Main.rand.NextBool(3) ? DustID.GoldCoin : DustID.GemSapphire, new Vector2(0f, -Main.rand.NextFloat(1f, 3f)), 0, default, 1.3f);
				dust.noGravity = true;
			}
		}

		// Scans down from the star for the first solid tile (or platform), for the warning line. Capped at about 70 tiles.
		private float FindGroundY()
		{
			int x = (int)(Projectile.Center.X / 16f);
			int startY = (int)(Projectile.Center.Y / 16f);
			for (int y = startY; y < startY + 70 && y < Main.maxTilesY - 2; y++)
			{
				if (!WorldGen.InWorld(x, y, 2))
				{
					break;
				}
				Tile tile = Main.tile[x, y];
				if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
				{
					return y * 16f;
				}
			}
			return Projectile.Center.Y + 70 * 16f;
		}

		// Landing: a burst of light and a burning patch on the ground (made by the owner machine only).
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (oldVelocity.Y > 0f && Projectile.owner == Main.myPlayer && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Vector2 centre = new Vector2(Projectile.Center.X, Projectile.Bottom.Y - StarfirePatch.PatchHeight / 2f);
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), centre, Vector2.Zero, ModContent.ProjectileType<StarfirePatch>(),
					(int)(Projectile.damage * StarfirePatch.DamageFraction), 0f, Main.myPlayer);
			}
			return true;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f }, Projectile.Center);
			for (int i = 0; i < 24; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, i % 3 == 0 ? DustID.GemSapphire : DustID.GoldCoin,
					Main.rand.NextVector2Circular(7f, 7f), 0, default, 1.6f);
				dust.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 position = Projectile.Center - Main.screenPosition;
			float scale = 1f;

			if (!Falling)
			{
				// The tell: a thin line to where it will land (it brightens as the drop gets near) and a pulsing star.
				float ramp = Projectile.ai[0] <= 0f ? 1f : MathHelper.Clamp(Projectile.localAI[0] / Projectile.ai[0], 0f, 1f);
				scale = 1f + 0.12f * (float)System.Math.Sin(Projectile.localAI[0] * 0.35f);
				float length = groundY - Projectile.Center.Y;
				if (length > 0f)
				{
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None,
						RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
					// Only the 1x1 corner of the magic pixel is used (the texture is bigger).
					Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, position, new Rectangle(0, 0, 1, 1), new Color(255, 225, 120) * (0.15f + 0.45f * ramp),
						MathHelper.PiOver2, new Vector2(0f, 0.5f), new Vector2(length, 2f + 3f * ramp), SpriteEffects.None, 0f);
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None,
						RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				}
			}

			float fadeIn = MathHelper.Clamp(Projectile.localAI[0] / 15f, 0f, 1f);
			Main.spriteBatch.Draw(texture, position, null, Color.White * fadeIn, 0f, StarCentre, scale, SpriteEffects.None, 0f);
			return false;
		}

		// The damaging part is the star itself, not the trail above it.
		public override bool? CanHitNPC(NPC target) => false;
		public override bool CanHitPlayer(Player target) => Falling && Projectile.localAI[0] > Projectile.ai[0] + 2f;
	}
}
