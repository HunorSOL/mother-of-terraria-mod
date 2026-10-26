using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MotherOfTerraria.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Projectiles
{
	// The GRAVITY WELL of the Mother of Terraria's stage 2 attack (owner's design, 2026-10-26; see MotherOfTerrariaBoss.DoGravityWell).
	// A dark point opens in the air (OpenTicks: it grows, nothing else happens: part of the tell), then for PullTicks it PULLS the player toward it
	// (a gentle tug at the edge of its reach that gets stronger the closer you are) and sends Starlight Lances (GravityLance) spiralling in toward
	// it from a ring around it. Touching the dark core hurts. Then it collapses (CollapseTicks) in a flash and is gone.
	// ai[0] = the boss's NPC index. localAI[0] = its age in ticks. Drawn entirely in code (no picture file).
	// The pull changes the LOCAL player's velocity (every client does that for itself), so it works in multiplayer too.
	// ALL NUMBERS ARE PLACEHOLDERS (the owner will edit them to taste).
	public class GravityWell : ModProjectile
	{
		public const int OpenTicks = 45;
		public const int PullTicks = 240;
		public const int CollapseTicks = 35;
		public const int TotalTicks = OpenTicks + PullTicks + CollapseTicks;

		private const float PullRadius = 900f;      // how far away you still feel it
		private const float PullAtEdge = 0.16f;     // pull per tick (a speed gain in pixels per tick, per tick) at the edge of the radius ...
		private const float PullAtCore = 0.5f;      // ... and right at the middle
		private const int PullRamp = 30;            // the pull fades in over this many ticks so it does not jerk you
		private const float MaxPullSpeed = 14f;     // it never speeds you up beyond this toward the well
		private const float CoreSize = 90f;         // the damaging core (a square this wide)

		private const int LanceEvery = 14;          // ticks between pairs of lances
		private const float LanceRing = 640f;       // the lances start this far from the well
		private const int LanceDamagePercent = 55;  // of the well's damage

		private int Age => (int)Projectile.localAI[0];
		private bool Pulling => Age >= OpenTicks && Age < OpenTicks + PullTicks;

		// No picture file: borrow a vanilla projectile texture (never drawn).
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathLaser;

		public override void SetDefaults()
		{
			Projectile.width = (int)CoreSize;
			Projectile.height = (int)CoreSize;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = TotalTicks + 5;
			Projectile.aiStyle = -1;
			Projectile.hide = false;
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
			float visible = Visible();
			Lighting.AddLight(Projectile.Center, 0.5f * visible, 0.3f * visible, 0.8f * visible);

			if (Age == 1)
			{
				SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.6f }, Projectile.Center);
			}

			// Light streams in toward the core all the time: the swirl.
			for (int i = 0; i < 3; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float radius = Main.rand.NextFloat(80f, 330f) * visible;
				Vector2 from = Projectile.Center + angle.ToRotationVector2() * radius;
				Vector2 tangent = (angle + MathHelper.PiOver2).ToRotationVector2();
				Dust dust = Dust.NewDustPerfect(from, Main.rand.NextBool(3) ? DustID.GemAmethyst : DustID.GoldCoin,
					tangent * 5f - angle.ToRotationVector2() * 2f, 0, default, 1.3f);
				dust.noGravity = true;
			}

			if (Pulling)
			{
				PullLocalPlayer();
				if (Age >= OpenTicks + 10 && Age < OpenTicks + PullTicks - 50 && (Age - OpenTicks) % LanceEvery == 0
					&& Projectile.owner == Main.myPlayer && Main.netMode != NetmodeID.MultiplayerClient)
				{
					// two lances on opposite sides of the ring, at a random angle each time; they spiral in the same way
					float angle = Main.rand.NextFloat(MathHelper.TwoPi);
					int damage = Projectile.damage * LanceDamagePercent / 100;
					for (int i = 0; i < 2; i++)
					{
						Vector2 from = Projectile.Center + (angle + i * MathHelper.Pi).ToRotationVector2() * LanceRing;
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), from, Vector2.Zero, ModContent.ProjectileType<GravityLance>(),
							damage, 0f, Main.myPlayer, Projectile.Center.X, Projectile.Center.Y);
					}
				}
			}
			if (Age == OpenTicks + PullTicks)
			{
				// the collapse: a flash and a ring of light
				SoundEngine.PlaySound(SoundID.Item107, Projectile.Center);
				for (int i = 0; i < 50; i++)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, Main.rand.NextVector2CircularEdge(12f, 12f) * Main.rand.NextFloat(0.5f, 1f), 0, default, 1.8f);
					dust.noGravity = true;
				}
			}
			if (Age >= TotalTicks)
			{
				Projectile.Kill();
			}
		}

		// 0 to 1: grows while it opens, full during the pull, shrinks away as it collapses.
		private float Visible()
		{
			if (Age < OpenTicks)
			{
				return Age / (float)OpenTicks;
			}
			if (Age >= OpenTicks + PullTicks)
			{
				return MathHelper.Clamp(1f - (Age - OpenTicks - PullTicks) / (float)CollapseTicks, 0f, 1f);
			}
			return 1f;
		}

		// The pull: a speed gain toward the core every tick, stronger the closer you are, never beyond MaxPullSpeed toward the well.
		private void PullLocalPlayer()
		{
			Player player = Main.LocalPlayer;
			if (!player.active || player.dead)
			{
				return;
			}
			Vector2 toWell = Projectile.Center - player.Center;
			float distance = toWell.Length();
			if (distance < 8f || distance > PullRadius)
			{
				return;
			}
			Vector2 direction = toWell / distance;
			float closeness = 1f - distance / PullRadius;
			float ramp = MathHelper.Clamp((Age - OpenTicks) / (float)PullRamp, 0f, 1f);
			float pull = (PullAtEdge + (PullAtCore - PullAtEdge) * closeness) * ramp;
			if (Vector2.Dot(player.velocity, direction) < MaxPullSpeed)
			{
				player.velocity += direction * pull;
			}
		}

		// Only the dark core hurts, and only while the well is pulling.
		public override bool CanHitPlayer(Player target) => Pulling;

		public override bool PreDraw(ref Color lightColor)
		{
			if (!HeavensSkyTextures.Ready)
			{
				return false;
			}
			float visible = Visible();
			Texture2D soft = HeavensSkyTextures.Soft;
			Texture2D ring = HeavensSkyTextures.Ring;
			Vector2 position = Projectile.Center - Main.screenPosition;
			float spin = Age * 0.04f;

			// the dark core, drawn with ordinary blending so it is dark against the bright sky
			Main.spriteBatch.Draw(soft, position, null, new Color(18, 8, 40) * (0.95f * visible), 0f, soft.Size() / 2f, 150f * visible / soft.Width * 1.6f, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(soft, position, null, new Color(6, 2, 16) * visible, 0f, soft.Size() / 2f, 90f * visible / soft.Width * 1.6f, SpriteEffects.None, 0f);

			// the glow and the gold rings, added on top (a faint outer ring marks the pull's reach during the pull)
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(soft, position, null, new Color(150, 90, 255, 0) * (0.5f * visible), 0f, soft.Size() / 2f, 360f * visible / soft.Width, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(ring, position, null, new Color(255, 205, 80, 0) * (0.9f * visible), spin, ring.Size() / 2f, 210f * visible / ring.Width, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(ring, position, null, new Color(255, 230, 150, 0) * (0.6f * visible), -spin * 1.7f, ring.Size() / 2f, 150f * visible / ring.Width, SpriteEffects.None, 0f);
			if (Pulling)
			{
				float reach = 0.25f + 0.1f * (float)System.Math.Sin(Age * 0.1f);
				Main.spriteBatch.Draw(ring, position, null, new Color(255, 205, 80, 0) * reach, 0f, ring.Size() / 2f, PullRadius * 2f / ring.Width * 1.25f, SpriteEffects.None, 0f);
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
