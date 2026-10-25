using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MotherOfTerraria.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Bosses
{
	// One of the four ENCHANTED CRYSTALS the Mother of Terraria releases when she falls to 50% health (the stage 1 -> stage 2
	// transition, see MotherOfTerrariaBoss.DoCrystalPhase). While any of them lives she cannot be hurt. They circle the player,
	// following them around, and each fires a short beam now and then (see Projectiles/CrystalBeam.cs); the player has to destroy all four.
	// ai[0] = the boss's NPC index, ai[1] = a tick counter, ai[2] = which crystal this is (0-3: its place around the player and when it acts),
	// ai[3] = the dasher's state (0 circling, 1 spinning up = the tell, 2 dashing). localAI[0] = the dash angle, localAI[1] = its contact damage,
	// localAI[2] = ticks in the current dash state, localAI[3] = how fast it spins.
	// TWO KINDS (owner's layout, 2026-10-25): crystals 0 and 2 are BEAM STARS (they fire the short beam); crystals 1 and 3 are DASHING STARS:
	// they have long sharp spikes at every point (golden, with blood-red tips), spin up as the tell, lock their aim, then dash through the
	// player and come back. Only the dash hurts (contact damage is zero while circling).
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them).
	// SINGLEPLAYER-ONLY: none known; spawning and firing run on the server / single player, but this has not been tested in multiplayer.
	public class EnchantedCrystal : ModNPC
	{
		public const int CrystalLife = 50000;      // owner, 2026-10-25
		private const float OrbitRadius = 330f;     // how far from the player it hovers
		private const float OrbitSpin = 0.012f;     // radians per tick the four crystals turn around the player
		private const float Speed = 7f;
		private const int FirstShot = 90;           // ticks after appearing before the first beam
		private const int ShotEvery = 200;          // ticks between one crystal's beams
		private const int ShotOffset = 50;          // each crystal fires this much later than the one before, so they never all fire together
		private const int BeamDamage = 200;

		// The dashing stars. Cycle: circle for DashFirst ticks, then spin up for DashTell ticks (the aim locks DashAimFreeze ticks before
		// the dash, and a thin line shows it), dash for DashTicks, repeat every DashEvery. The second dasher is half a cycle behind.
		private const int DashFirst = 130;
		private const int DashEvery = 230;
		private const int DashTell = 55;
		private const int DashAimFreeze = 20;
		private const int DashTicks = 34;
		private const float DashSpeed = 25f;
		private const int DashDamage = 220;
		private const float MaxSpin = 0.55f;        // radians per tick at full spin

		private bool IsDasher => (int)NPC.ai[2] % 2 == 1;

		// PLACEHOLDER art: the Gold Crystal Star picture stands in until the owner designs the crystal (see ASSETS.md).
		public override string Texture => "MotherOfTerraria/Projectiles/GoldCrystalStar";

		public override void SetStaticDefaults()
		{
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Hide = true };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 56;
			NPC.height = 56;
			NPC.damage = DashDamage; // the beam stars never touch (damage is set to 0 in AI); the dashers hurt only while dashing
			NPC.defense = 60;
			NPC.lifeMax = CrystalLife;
			NPC.HitSound = SoundID.Item27; // glass
			NPC.DeathSound = SoundID.Item107;
			NPC.knockBackResist = 0f;
			NPC.value = 0f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.lavaImmune = true;
			NPC.npcSlots = 0f;
			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.OnFire] = true;
			NPC.buffImmune[BuffID.Poisoned] = true;
		}

		// The game scales health by difficulty; set it back to exactly CrystalLife, as for the boss.
		public override void OnSpawn(IEntitySource source)
		{
			NPC.lifeMax = CrystalLife;
			NPC.life = CrystalLife;
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)NPC.ai[0]];
			if (!owner.active || owner.type != ModContent.NPCType<MotherOfTerrariaBoss>())
			{
				NPC.active = false; // she is gone, so are they
				return;
			}
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];
			if (!target.active || target.dead)
			{
				return;
			}
			NPC.ai[1]++;
			if (NPC.localAI[1] == 0f)
			{
				NPC.localAI[1] = NPC.damage; // the damage after the game's difficulty scaling, kept for the dash
			}
			bool dashing = IsDasher && NPC.ai[3] == 2f;
			NPC.damage = dashing ? (int)NPC.localAI[1] : 0;
			if (IsDasher)
			{
				// the spikes reach about 70 px out, so the hitbox grows to cover them while it dashes
				int size = dashing ? 110 : 56;
				if (NPC.width != size)
				{
					Vector2 centre = NPC.Center;
					NPC.width = size;
					NPC.height = size;
					NPC.Center = centre;
				}
			}

			if (dashing)
			{
				NPC.localAI[2]++;
				NPC.rotation += MaxSpin;
				if (NPC.localAI[2] >= DashTicks)
				{
					NPC.ai[3] = 0f; // the dash is over: back to circling
					NPC.localAI[2] = 0f;
					NPC.netUpdate = true;
				}
				Lighting.AddLight(NPC.Center, 1f, 0.5f, 0.3f);
				return; // the velocity set at the start of the dash carries it
			}

			// Circle the player: the four crystals sit at the corners of a square that turns slowly around them.
			float angle = NPC.ai[2] * MathHelper.PiOver2 + NPC.ai[1] * OrbitSpin;
			Vector2 goal = target.Center + angle.ToRotationVector2() * OrbitRadius;
			Vector2 toGoal = goal - NPC.Center;
			float topSpeed = toGoal.Length() > 150f ? Speed * 2.3f : Speed; // a dasher coming back from its dash hurries
			float speed = System.Math.Min(topSpeed, toGoal.Length() * 0.15f);
			NPC.velocity = Vector2.Lerp(NPC.velocity, toGoal.SafeNormalize(Vector2.Zero) * speed, 0.2f);

			Lighting.AddLight(NPC.Center, 0.9f, 0.8f, 0.4f);

			if (IsDasher)
			{
				DoDasher(target);
			}
			else
			{
				NPC.rotation = NPC.velocity.X * 0.02f;
				int sinceFirst = (int)NPC.ai[1] - FirstShot - (int)NPC.ai[2] * ShotOffset;
				if (sinceFirst >= 0 && sinceFirst % ShotEvery == 0 && Main.netMode != NetmodeID.MultiplayerClient)
				{
					int damage = NPC.GetAttackDamage_ForProjectiles(BeamDamage, BeamDamage * 0.8f);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CrystalBeam>(),
						damage, 0f, Main.myPlayer, NPC.whoAmI, (target.Center - NPC.Center).ToRotation());
				}
			}
		}

		// The dashing star: wait, spin up (the tell), lock the aim, dash straight through the player's spot, circle again.
		private void DoDasher(Player target)
		{
			int sinceStart = (int)NPC.ai[1] - DashFirst - ((int)NPC.ai[2] / 2) * (DashEvery / 2);
			int inCycle = sinceStart >= 0 ? sinceStart % DashEvery : -1;
			if (inCycle < 0 || inCycle >= DashTell)
			{
				NPC.localAI[3] = MathHelper.Max(0f, NPC.localAI[3] - 0.02f); // it winds down while circling
				NPC.rotation += NPC.localAI[3];
				NPC.ai[3] = 0f;
				return;
			}

			// The tell: it spins faster and faster and the aim line appears; the aim follows the player, then locks.
			NPC.ai[3] = 1f;
			NPC.localAI[3] = MathHelper.Lerp(0.05f, MaxSpin, inCycle / (float)DashTell);
			NPC.rotation += NPC.localAI[3];
			if (inCycle < DashTell - DashAimFreeze)
			{
				NPC.localAI[0] = (target.Center - NPC.Center).ToRotation();
			}
			if (inCycle % 8 == 0)
			{
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, 0f, 0f, 0, default, 1.2f);
				dust.noGravity = true;
			}
			if (inCycle == DashTell - 1)
			{
				SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f }, NPC.Center);
				NPC.velocity = NPC.localAI[0].ToRotationVector2() * DashSpeed;
				NPC.ai[3] = 2f;
				NPC.localAI[2] = 0f;
				NPC.netUpdate = true;
			}
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			int count = NPC.life > 0 ? 3 : 30;
			for (int i = 0; i < count; i++)
			{
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GoldCoin, 0f, 0f, 0, default, 1.4f);
				dust.noGravity = true;
				dust.velocity *= NPC.life > 0 ? 1f : 4f;
			}
		}

		// A soft gold glow behind the crystal so it reads as enchanted, then (for a dasher) its spikes, then the picture.
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (IsDasher)
			{
				DrawDasherExtras(spriteBatch, screenPos);
			}
			if (Systems.HeavensSkyTextures.Ready)
			{
				Texture2D glow = Systems.HeavensSkyTextures.Soft;
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				float pulse = 0.85f + 0.15f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 5f + NPC.ai[2]);
				spriteBatch.Draw(glow, NPC.Center - screenPos, null, new Color(255, 215, 100, 0) * (0.8f * pulse), 0f, glow.Size() / 2f, 200f / glow.Width, SpriteEffects.None, 0f);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
			return true;
		}

		// A dasher's long sharp spikes: one at each of the star's eight points, golden with a blood-red tip, drawn from strips of the
		// game's white pixel (only its 1x1 corner) that get thinner toward the point. They turn with the star. While the dash is being
		// aimed a thin pale line shows where it will go.
		private void DrawDasherExtras(SpriteBatch spriteBatch, Vector2 screenPos)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle one = new Rectangle(0, 0, 1, 1);
			Vector2 centre = NPC.Center - screenPos;

			if (NPC.ai[3] == 1f && NPC.localAI[3] >= MaxSpin * 0.4f)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				float ramp = NPC.localAI[3] / MaxSpin;
				spriteBatch.Draw(pixel, centre, one, new Color(255, 120, 90) * (0.2f + 0.4f * ramp), NPC.localAI[0], new Vector2(0f, 0.5f), new Vector2(DashSpeed * DashTicks, 4f), SpriteEffects.None, 0f);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			const int pieces = 12;
			for (int k = 0; k < 8; k++)
			{
				bool longPoint = k % 2 == 0;                       // the four long points of the star, then the four short diagonal ones
				float baseRadius = longPoint ? 26f : 20f;
				float length = longPoint ? 44f : 34f;
				float direction = NPC.rotation + k * MathHelper.PiOver4 - MathHelper.PiOver2;
				for (int j = 0; j < pieces; j++)
				{
					float t = j / (float)pieces;
					float width = 15f * (1f - t) + 1.5f;           // wide at the root, a needle at the tip
					Vector2 at = centre + direction.ToRotationVector2() * (baseRadius + t * length);
					float pieceLength = length / pieces + 1.5f;
					bool blood = t > 0.55f;                         // the top of the spike is bloody
					Color body = blood ? Color.Lerp(new Color(190, 20, 25), new Color(110, 5, 15), (t - 0.55f) / 0.45f) : Color.Lerp(new Color(255, 220, 90), new Color(240, 170, 30), t / 0.55f);
					spriteBatch.Draw(pixel, at, one, new Color(60, 25, 20), direction, new Vector2(0.5f, 0.5f), new Vector2(pieceLength, width + 3f), SpriteEffects.None, 0f); // dark outline
					spriteBatch.Draw(pixel, at, one, body, direction, new Vector2(0.5f, 0.5f), new Vector2(pieceLength, width), SpriteEffects.None, 0f);
					if (width > 5f && !blood)
					{
						spriteBatch.Draw(pixel, at + direction.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * (-width * 0.22f), one, new Color(255, 245, 190), direction, new Vector2(0.5f, 0.5f), new Vector2(pieceLength, width * 0.22f), SpriteEffects.None, 0f); // a highlight along one side
					}
				}
			}
		}

		public override Color? GetAlpha(Color drawColor) => Color.White; // always fully bright
	}
}
