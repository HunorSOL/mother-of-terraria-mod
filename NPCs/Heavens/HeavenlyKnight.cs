using System;
using Microsoft.Xna.Framework;
using MotherOfTerraria.Items;
using MotherOfTerraria.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Heavens
{
	// The Heavenly Knight: a winged golden guardian that flies around the player, throws a heavenly spear
	// (with a visible wind-up so it can be dodged), and sometimes lands to fight up close.
	// A ModNPC is a custom enemy. Its AI() method runs every tick on the server (or in singleplayer).
	// Texture: NPCs/Heavens/HeavenlyKnight.png (single frame placeholder).
	// ALL NUMBERS BELOW ARE PLACEHOLDERS: health, damage and speeds get tuned with the owner after the enemies exist.
	public class HeavenlyKnight : ModNPC
	{
		private const int SpearDamage = 120;
		private const float SpearSpeed = 16f;
		private const int HoverTime = 150;   // ticks (60 per second) spent flying before the next action
		private const int WindUpTime = 45;   // the tell: the knight stops and gathers light
		private const int RecoverTime = 30;  // short pause after the throw
		private const int LandTime = 240;    // how long it fights on the ground

		private enum State
		{
			Hover = 0,
			WindUp = 1,
			Recover = 2,
			Land = 3,
		}

		// NPC.ai[] values are synced to other players automatically, so they hold anything other players need to know.
		private State CurrentState
		{
			get => (State)(int)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private ref float Timer => ref NPC.ai[1];

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 40;
			NPC.height = 56;
			NPC.damage = 150;       // contact damage (placeholder)
			NPC.defense = 60;       // placeholder
			NPC.lifeMax = 9000;     // placeholder
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
			NPC.knockBackResist = 0.15f;
			NPC.value = Item.buyPrice(gold: 2);
			NPC.aiStyle = -1;       // our own AI below
			NPC.noGravity = true;   // flying; the Land state applies gravity by hand
			NPC.noTileCollide = false;
			NPC.buffImmune[BuffID.Confused] = true;
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];

			// No living player left: fly away and despawn.
			if (!target.active || target.dead)
			{
				NPC.velocity.Y -= 0.2f;
				NPC.EncourageDespawn(60);
				return;
			}

			NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
			Lighting.AddLight(NPC.Center, 0.8f, 0.6f, 0.2f);

			switch (CurrentState)
			{
				case State.Hover:
					DoHover(target);
					break;
				case State.WindUp:
					DoWindUp(target);
					break;
				case State.Recover:
					DoRecover();
					break;
				case State.Land:
					DoLand(target);
					break;
			}
		}

		// Fly to a spot beside and above the player, then pick the next action.
		private void DoHover(Player target)
		{
			NPC.noGravity = true;

			float side = NPC.Center.X < target.Center.X ? -1f : 1f; // stay on the side it is already on
			Vector2 goal = target.Center + new Vector2(side * 320f, -150f);
			Vector2 desired = (goal - NPC.Center) * 0.06f;
			if (desired.Length() > 10f)
			{
				desired = desired.SafeNormalize(Vector2.Zero) * 10f;
			}
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.08f);

			Timer++;
			if (Timer >= HoverTime)
			{
				Timer = 0;
				bool close = Vector2.Distance(NPC.Center, target.Center) < 260f;
				// Close to the player and ground below? Half the time, land and fight up close.
				CurrentState = close && HasGroundBelow() && Main.rand.NextBool() ? State.Land : State.WindUp;
				NPC.netUpdate = true;
			}
		}

		// The tell: the knight slows to a stop and golden-blue light gathers at the spear hand.
		// Then it throws. This is the moment to dodge.
		private void DoWindUp(Player target)
		{
			NPC.velocity *= 0.9f;
			Vector2 hand = HandPosition();

			if (Timer == 0)
			{
				SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
			}

			// Light streams in toward the hand, more densely as the throw gets closer.
			for (int i = 0; i < 2; i++)
			{
				Vector2 spawn = hand + Main.rand.NextVector2CircularEdge(50f, 50f);
				int type = Main.rand.NextBool() ? DustID.GoldCoin : DustID.GemSapphire;
				Dust dust = Dust.NewDustPerfect(spawn, type, (hand - spawn) * 0.1f, 0, default, 1.3f);
				dust.noGravity = true;
			}

			Timer++;
			if (Timer >= WindUpTime)
			{
				// Only the server (or singleplayer) creates projectiles; it then appears for everyone.
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 direction = (target.Center - hand).SafeNormalize(Vector2.UnitX * NPC.direction);
					int damage = NPC.GetAttackDamage_ForProjectiles(SpearDamage, SpearDamage * 0.8f);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), hand, direction * SpearSpeed,
						ModContent.ProjectileType<HeavenlySpear>(), damage, 2f, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item1, NPC.Center);

				Timer = 0;
				CurrentState = State.Recover;
				NPC.netUpdate = true;
			}
		}

		private void DoRecover()
		{
			NPC.velocity *= 0.95f;
			Timer++;
			if (Timer >= RecoverTime)
			{
				Timer = 0;
				CurrentState = State.Hover;
				NPC.netUpdate = true;
			}
		}

		// Land and chase the player on foot. Its body hurts on contact. Then take off again.
		private void DoLand(Player target)
		{
			NPC.noGravity = false;
			NPC.velocity.Y = Math.Min(NPC.velocity.Y + 0.35f, 10f); // gravity, applied by hand

			float targetSpeed = NPC.direction * 3.5f;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, targetSpeed, 0.08f);

			// Hop over a block in the way.
			if (NPC.collideX && NPC.velocity.Y == 0f)
			{
				NPC.velocity.Y = -8f;
			}

			Timer++;
			bool playerTooFar = Vector2.Distance(NPC.Center, target.Center) > 600f;
			if (Timer >= LandTime || playerTooFar)
			{
				Timer = 0;
				NPC.noGravity = true;
				NPC.velocity.Y = -9f; // push off the ground
				CurrentState = State.Hover;
				NPC.netUpdate = true;
			}
		}

		// Where the thrown spear leaves the knight: out in front, around chest height.
		private Vector2 HandPosition()
		{
			return NPC.Center + new Vector2(NPC.direction * 24f, -8f);
		}

		// True if solid ground is within ~30 tiles straight down (so landing cannot mean falling off the world).
		private bool HasGroundBelow()
		{
			int x = (int)(NPC.Center.X / 16f);
			int startY = (int)(NPC.Bottom.Y / 16f);
			for (int y = startY; y < startY + 30 && y < Main.maxTilesY - 1; y++)
			{
				Tile tile = Main.tile[x, y];
				if (tile.HasTile && Main.tileSolid[tile.TileType])
				{
					return true;
				}
			}
			return false;
		}

		// Drops: a chance (placeholder: 1 in 3) of 1 to 2 Heavens Crystals.
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HeavensCrystal>(), 3, 1, 2));
		}
	}
}
