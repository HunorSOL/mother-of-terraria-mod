using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MotherOfTerraria.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// One JUDGMENT MARK of the Mother of Terraria's stage 2 attack (owner's idea, 2026-10-26; see MotherOfTerrariaBoss.DoJudgmentMark). Four stages:
	//   1. FOLLOW (FollowTicks): a thin golden sigil slides after the player, at a speed a little slower than a running player, so keep moving.
	//   2. LOCK (LockTicks): it stops, flashes white and shrinks to its true size, and a thick line (the same style as the Falling Stars' lines) shows the
	//      column the lance will fall down. This is the time to get out.
	//   3. STRIKE (StrikeTicks): a giant Starlight Lance falls down the column and hits whatever is in it (the big damage).
	//   4. LINGER (LingerTicks): a patch of light stays on the spot and hurts a little (40%) if you stand in it.
	// ai[0] = the boss's NPC index. localAI[0] = age in ticks. Drawn in code; the lance uses the Starlight Lance picture.
	// ALL NUMBERS ARE PLACEHOLDERS (the owner will edit them to taste). SINGLEPLAYER-ONLY: the following is decided on the owner's machine.
	public class JudgmentMark : ModProjectile
	{
		public const int FollowTicks = 70;
		public const int LockTicks = 35;
		public const int StrikeTicks = 10;
		public const int LingerTicks = 90;
		public const int TotalTicks = FollowTicks + LockTicks + StrikeTicks + LingerTicks;

		private const float FollowSpeed = 5.5f;       // pixels per tick (a running player does about 7 to 9, wings and boots more)
		private const float ColumnHalfWidth = 75f;    // the lance's column is 150 wide
		// The lance now hits the WHOLE height of the map in its column (owner, 2026-10-26: "from top to bottom"): from y = 0 to the bottom of the world.
		private static float ColumnTop => 0f;
		private static float ColumnBottom => Main.maxTilesY * 16f;
		private const float LingerHalfWidth = 95f;
		private const float LingerHalfHeight = 55f;
		private const float LingerDamage = 0.4f;      // of the strike's damage
		private const float LanceScale = 3.2f;        // the Starlight Lance picture (49x104) at this size is about 157 px wide: the column

		private int Age => (int)Projectile.localAI[0];
		private bool Striking => Age >= FollowTicks + LockTicks && Age < FollowTicks + LockTicks + StrikeTicks;
		private bool Lingering => Age >= FollowTicks + LockTicks + StrikeTicks;

		public override string Texture => "MotherOfTerraria/Projectiles/StarlightLance";

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
			if (!owner.active)
			{
				Projectile.Kill();
				return;
			}

			if (Age < FollowTicks)
			{
				// slide after the player (decided on the owner's machine)
				if (Projectile.owner == Main.myPlayer)
				{
					Player target = Main.player[owner.target];
					Vector2 toPlayer = target.Center - Projectile.Center;
					float move = System.Math.Min(FollowSpeed, toPlayer.Length());
					Projectile.Center += toPlayer.SafeNormalize(Vector2.Zero) * move;
					Projectile.netUpdate = true;
				}
				Lighting.AddLight(Projectile.Center, 0.5f, 0.4f, 0.15f);
			}
			else
			{
				Lighting.AddLight(Projectile.Center, 1.2f, 1f, 0.5f);
			}

			if (Age == FollowTicks)
			{
				SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f }, Projectile.Center); // the lock
			}
			if (Age == FollowTicks + LockTicks)
			{
				SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
				Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Vector2.UnitY, 6f, 14f, 18, 1800f, "MotherOfTerraria:JudgmentStrike"));
				for (int i = 0; i < 60; i++)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-ColumnHalfWidth, ColumnHalfWidth), Main.rand.NextFloat(-30f, 30f)),
						i % 3 == 0 ? DustID.GemAmethyst : DustID.GoldCoin, new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-9f, -1f)), 0, default, 1.8f);
					dust.noGravity = true;
				}
			}
			if (Lingering && Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(-LingerHalfWidth, LingerHalfWidth), Main.rand.NextFloat(-10f, LingerHalfHeight)),
					DustID.GoldCoin, new Vector2(0f, -Main.rand.NextFloat(0.5f, 2f)), 0, default, 1.3f);
				dust.noGravity = true;
			}
			if (Age >= TotalTicks)
			{
				Projectile.Kill();
			}
		}

		// The strike hurts everything in the lance's column; the lingering light hurts a smaller area. Nothing else does.
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Striking)
			{
				Rectangle column = new Rectangle((int)(Projectile.Center.X - ColumnHalfWidth), (int)ColumnTop, (int)(ColumnHalfWidth * 2f), (int)(ColumnBottom - ColumnTop));
				return column.Intersects(targetHitbox);
			}
			if (Lingering)
			{
				Rectangle patch = new Rectangle((int)(Projectile.Center.X - LingerHalfWidth), (int)(Projectile.Center.Y - LingerHalfHeight), (int)(LingerHalfWidth * 2f), (int)(LingerHalfHeight * 2f));
				return patch.Intersects(targetHitbox);
			}
			return false;
		}

		public override bool CanHitPlayer(Player target) => Striking || Lingering;

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			if (Lingering)
			{
				modifiers.SourceDamage *= LingerDamage;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!HeavensSkyTextures.Ready)
			{
				return false;
			}
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle one = new Rectangle(0, 0, 1, 1);
			Texture2D ring = HeavensSkyTextures.Ring;
			Texture2D soft = HeavensSkyTextures.Soft;
			Vector2 centre = Projectile.Center - Main.screenPosition;
			float age = Age;
			float fadeIn = MathHelper.Clamp(age / 12f, 0f, 1f);

			// The column line (the same style as the Falling Stars' lines): only from the moment of the lock until the lance has landed.
			if (age >= FollowTicks && age < FollowTicks + LockTicks + StrikeTicks)
			{
				float lock01 = MathHelper.Clamp((age - FollowTicks) / (float)LockTicks, 0f, 1f);
				float pulse = 0.85f + 0.15f * (float)System.Math.Sin(age * 0.5f);
				float width = ColumnHalfWidth * 2f;
				float top = -40f;                                    // the whole visible height of the screen, top to bottom
				float length = Main.screenHeight + 80f;
				if (length > 0f)
				{
					float fade = 0.35f + 0.55f * lock01;
					Main.spriteBatch.Draw(pixel, new Vector2(centre.X - width / 2f - 5f, top), one, new Color(40, 25, 110) * (0.4f * fade), 0f, Vector2.Zero, new Vector2(width + 10f, length), SpriteEffects.None, 0f);
					Main.spriteBatch.Draw(pixel, new Vector2(centre.X - width / 2f, top), one, new Color(255, 205, 60) * (0.28f * fade * pulse), 0f, Vector2.Zero, new Vector2(width, length), SpriteEffects.None, 0f);
					Main.spriteBatch.Draw(pixel, new Vector2(centre.X - width / 2f - 4f, top), one, new Color(255, 205, 60) * (0.9f * fade * pulse), 0f, Vector2.Zero, new Vector2(8f, length), SpriteEffects.None, 0f);
					Main.spriteBatch.Draw(pixel, new Vector2(centre.X + width / 2f - 4f, top), one, new Color(255, 205, 60) * (0.9f * fade * pulse), 0f, Vector2.Zero, new Vector2(8f, length), SpriteEffects.None, 0f);
				}
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			if (age < FollowTicks + LockTicks + StrikeTicks)
			{
				// the sigil: two rings turning against each other, with rune points. While following it is large and thin; at the lock it flashes and
				// shrinks to the width of the column.
				float lock01 = age < FollowTicks ? 0f : MathHelper.Clamp((age - FollowTicks) / (float)LockTicks, 0f, 1f);
				float radius = MathHelper.Lerp(150f, ColumnHalfWidth, lock01);
				float bright = (age < FollowTicks ? 0.55f : 0.9f) * fadeIn;
				float flash = age >= FollowTicks && age < FollowTicks + 6 ? 1f : 0f;
				Color gold = new Color(255, 205, 80, 0);
				Main.spriteBatch.Draw(ring, centre, null, gold * bright, age * 0.03f, ring.Size() / 2f, radius / 102.4f, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(ring, centre, null, new Color(255, 235, 170, 0) * (bright * 0.8f), -age * 0.05f, ring.Size() / 2f, radius * 0.62f / 102.4f, SpriteEffects.None, 0f);
				for (int i = 0; i < 8; i++)
				{
					float angle = age * 0.03f + i * MathHelper.PiOver4;
					Vector2 point = centre + angle.ToRotationVector2() * radius * 0.85f;
					Main.spriteBatch.Draw(soft, point, null, gold * (bright * 0.9f), 0f, soft.Size() / 2f, 26f / soft.Width, SpriteEffects.None, 0f);
				}
				Main.spriteBatch.Draw(soft, centre, null, new Color(255, 240, 200, 0) * ((0.25f * lock01 + flash) * fadeIn), 0f, soft.Size() / 2f, (radius * 2.2f) / soft.Width, SpriteEffects.None, 0f);
			}

			if (Lingering)
			{
				float fade = MathHelper.Clamp(1f - (age - FollowTicks - LockTicks - StrikeTicks) / (float)LingerTicks, 0f, 1f);
				Main.spriteBatch.Draw(soft, centre, null, new Color(255, 200, 70, 0) * (0.7f * fade), 0f, soft.Size() / 2f, new Vector2(LingerHalfWidth * 2.6f, LingerHalfHeight * 2.6f) / soft.Width, SpriteEffects.None, 0f);
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			// The lance: it falls down the column in StrikeTicks and then stays a moment, stuck in the spot, fading.
			if (age >= FollowTicks + LockTicks && age < FollowTicks + LockTicks + StrikeTicks + 12)
			{
				float fall = MathHelper.Clamp((age - FollowTicks - LockTicks) / (float)(StrikeTicks - 2), 0f, 1f);
				Texture2D lance = TextureAssets.Projectile[Type].Value;
				float lanceLength = lance.Height * LanceScale;
				float tipY = MathHelper.Lerp(ColumnTop, ColumnBottom, fall); // it falls the whole height of the map
				Vector2 lanceCentre = new Vector2(Projectile.Center.X, tipY - lanceLength / 2f) - Main.screenPosition;
				float fade = age < FollowTicks + LockTicks + StrikeTicks ? 1f : MathHelper.Clamp(1f - (age - FollowTicks - LockTicks - StrikeTicks) / 12f, 0f, 1f);
				// the lance crosses the screen in a flash, so the whole column glows bright for the moment of the strike and fades after it
				float glow = age < FollowTicks + LockTicks + StrikeTicks ? 0.75f : 0.75f * fade;
				Main.spriteBatch.Draw(pixel, new Vector2(centre.X - ColumnHalfWidth, -40f), one, new Color(255, 225, 140) * glow, 0f, Vector2.Zero, new Vector2(ColumnHalfWidth * 2f, Main.screenHeight + 80f), SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(lance, lanceCentre, null, Color.White * fade, MathHelper.Pi, lance.Size() / 2f, LanceScale, SpriteEffects.None, 0f);
			}
			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
