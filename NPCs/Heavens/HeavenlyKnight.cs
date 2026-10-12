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
	// The Heavenly Knight: a golden winged guardian that walks the Heavens' islands toward the player and
	// throws a heavenly spear (with a visible wind-up so it can be dodged). Its body also hurts on contact.
	// A ModNPC is a custom enemy. Its AI() method runs every tick on the server (or in singleplayer).
	// Texture: NPCs/Heavens/HeavenlyKnight.png (owner's design, single frame).
	// ALL NUMBERS BELOW ARE PLACEHOLDERS: health, damage and speeds get tuned with the owner after the enemies exist.
	public class HeavenlyKnight : ModNPC
	{
		private const int SpearDamage = 120;
		private const float SpearSpeed = 10f;   // slow enough to read and dodge
		private const int WalkTime = 150;       // ticks (60 per second) spent walking before the next throw
		private const int WindUpTime = 45;      // the tell: the knight stops and gathers light
		private const int RecoverTime = 40;     // short pause after the throw
		private const float WalkSpeed = 2.4f;
		private const float KeepDistance = 280f; // it stops walking once this close (pixels), then keeps throwing

		private enum State
		{
			Walk = 0,
			WindUp = 1,
			Recover = 2,
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
			// The sheet stacks 7 frames: 0 = standing, 1-6 = walk cycle (made by tools/make_knight_sheet.py).
			Main.npcFrameCount[Type] = 7;
		}

		// Picks which frame of the sheet to draw. Runs on each player's own game (visuals only).
		public override void FindFrame(int frameHeight)
		{
			bool walking = Math.Abs(NPC.velocity.X) > 0.4f && NPC.velocity.Y == 0f;
			if (!walking)
			{
				NPC.frameCounter = 0;
				NPC.frame.Y = 0; // standing (also used while winding up and in the air)
				return;
			}

			NPC.frameCounter++;
			if (NPC.frameCounter >= 6) // a new frame every 6 ticks = 10 per second
			{
				NPC.frameCounter = 0;
				int next = NPC.frame.Y / frameHeight + 1;
				if (next > 6)
				{
					next = 1;
				}
				NPC.frame.Y = next * frameHeight;
			}
			// Coming from the standing frame: make sure we start inside the walk cycle.
			if (NPC.frame.Y == 0)
			{
				NPC.frame.Y = frameHeight;
			}
		}

		public override void SetDefaults()
		{
			NPC.width = 40;
			NPC.height = 76;
			NPC.damage = 150;       // contact damage (placeholder)
			NPC.defense = 60;       // placeholder
			NPC.lifeMax = 13500;    // 9000 + 50% (owner request); placeholder
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;
			NPC.knockBackResist = 0.15f;
			NPC.value = Item.buyPrice(gold: 2);
			NPC.aiStyle = -1;       // our own AI below
			NPC.noGravity = true;   // gravity is applied by hand in AI() so we control it exactly
			NPC.noTileCollide = false;
			NPC.buffImmune[BuffID.Confused] = true;
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];

			// Fell off the islands: remove it.
			if (NPC.Center.Y > 340 * 16)
			{
				NPC.active = false;
				return;
			}

			// No living player left: stop and despawn.
			if (!target.active || target.dead)
			{
				ApplyGravity();
				NPC.velocity.X *= 0.9f;
				NPC.EncourageDespawn(60);
				return;
			}

			// A velocity.Y of exactly 0 at the start of AI means we landed on something last tick.
			bool onGround = NPC.velocity.Y == 0f;

			NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;

			// Which way the picture is drawn. Terraria flips a sprite when spriteDirection is 1 (vanilla art faces
			// left), but the owner's knight art faces RIGHT, so the sign is reversed. If it ever looks backwards,
			// change this to `= NPC.direction`.
			NPC.spriteDirection = -NPC.direction;
			Lighting.AddLight(NPC.Center, 0.8f, 0.6f, 0.2f);

			switch (CurrentState)
			{
				case State.Walk:
					DoWalk(target, onGround);
					break;
				case State.WindUp:
					DoWindUp(target);
					break;
				case State.Recover:
					DoRecover();
					break;
			}

			ApplyGravity();
		}

		private void ApplyGravity()
		{
			NPC.velocity.Y = Math.Min(NPC.velocity.Y + 0.35f, 10f);
		}

		// Walk toward the player (stopping at the island edge and once close enough), then start a throw.
		private void DoWalk(Player target, bool onGround)
		{
			float distance = Math.Abs(target.Center.X - NPC.Center.X);
			bool wantsToMove = distance > KeepDistance && !EdgeAhead();
			float goalSpeed = wantsToMove ? NPC.direction * WalkSpeed : 0f;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, goalSpeed, 0.1f);

			// Hop over a block in the way, or up toward a player standing higher.
			if (onGround && wantsToMove && (NPC.collideX || target.Center.Y < NPC.Top.Y - 48f))
			{
				NPC.velocity.Y = -8f;
			}

			Timer++;
			if (Timer >= WalkTime && Vector2.Distance(NPC.Center, target.Center) < 900f)
			{
				Timer = 0;
				CurrentState = State.WindUp;
				NPC.netUpdate = true;
			}
		}

		// The tell: the knight stops and golden-blue light gathers at the spear hand.
		// Then it throws. This is the moment to dodge.
		private void DoWindUp(Player target)
		{
			NPC.velocity.X *= 0.8f;
			Vector2 hand = HandPosition();

			if (Timer == 0)
			{
				SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
			}

			// Light streams in toward the hand.
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
			NPC.velocity.X *= 0.9f;
			Timer++;
			if (Timer >= RecoverTime)
			{
				Timer = 0;
				CurrentState = State.Walk;
				NPC.netUpdate = true;
			}
		}

		// Where the thrown spear leaves the knight: out in front, around chest height.
		private Vector2 HandPosition()
		{
			return NPC.Center + new Vector2(NPC.direction * 22f, -10f);
		}

		// True if there is no solid ground within ~4 tiles below the spot 2 tiles ahead of the knight,
		// so it stops at island edges instead of walking off into the void.
		private bool EdgeAhead()
		{
			int x = (int)((NPC.Center.X + NPC.direction * 40f) / 16f);
			int startY = (int)(NPC.Bottom.Y / 16f);
			for (int y = startY; y <= startY + 4 && y < Main.maxTilesY - 1; y++)
			{
				Tile tile = Main.tile[x, y];
				if (tile.HasTile && Main.tileSolid[tile.TileType])
				{
					return false;
				}
			}
			return true;
		}

		// Drops: a chance (placeholder: 1 in 3) of 1 to 2 Heavens Crystals.
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HeavensCrystal>(), 3, 1, 2));
		}
	}
}
