using System;
using Microsoft.Xna.Framework;
using MotherOfTerraria.Items;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Heavens
{
	// The Heavens Dragon: a small wyvern-like dragon. It stalks the player on the ground, then rears back with
	// its mouth glowing (the tell) and DASHES in a straight line. Anything it hits is set On Fire!
	// After the dash it is exposed for a moment.
	// Texture: NPCs/Heavens/HeavensDragon.png (PLACEHOLDER sheet made by tools/make_dragon_placeholder.py;
	// the owner has no design yet). It faces RIGHT, like the knight.
	// ALL NUMBERS BELOW ARE PLACEHOLDERS, to be tuned with the owner after the enemies exist.
	public class HeavensDragon : ModNPC
	{
		private const float WalkSpeed = 2.8f;
		private const float KeepDistance = 220f;    // it stops walking once this close (pixels)
		private const int WalkTime = 110;           // ticks (60 per second) of stalking before a dash
		private const int WindUpTime = 50;          // the tell: it rears back and its mouth glows
		private const float DashSpeed = 13f;
		private const int DashTime = 26;            // about 21 tiles of travel
		private const int RecoverTime = 70;         // exposed after the dash
		private const float DashDamageMultiplier = 1.5f;
		private const int BurnTicks = 60 * 5;       // On Fire! lasts 5 seconds

		private enum State
		{
			Walk = 0,
			WindUp = 1,
			Dash = 2,
			Recover = 3,
		}

		private State CurrentState
		{
			get => (State)(int)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private ref float Timer => ref NPC.ai[1];
		private ref float DashDirection => ref NPC.ai[2]; // locked at the start of a dash; synced to other players

		public override void SetStaticDefaults()
		{
			// The sheet stacks 4 frames: 0 = walk A, 1 = walk B, 2 = wind-up, 3 = dash.
			Main.npcFrameCount[Type] = 4;
		}

		public override void SetDefaults()
		{
			NPC.width = 56;
			NPC.height = 40;
			NPC.damage = 140;       // contact damage (placeholder)
			NPC.defense = 45;       // placeholder
			NPC.lifeMax = 6500;     // placeholder
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath8;
			NPC.knockBackResist = 0.2f;
			NPC.value = Item.buyPrice(gold: 2);
			NPC.aiStyle = -1;       // our own AI below
			NPC.noGravity = true;   // gravity is applied by hand in AI()
			NPC.noTileCollide = false;
			NPC.buffImmune[BuffID.OnFire] = true; // a fire dragon does not burn
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

			bool onGround = NPC.velocity.Y == 0f;

			// It keeps facing the player, except during a dash, when it stays locked on its path.
			if (CurrentState != State.Dash)
			{
				NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
			}
			// The art faces right and Terraria flips when spriteDirection is 1, hence the reversed sign
			// (same as the knight).
			NPC.spriteDirection = -NPC.direction;

			Lighting.AddLight(NPC.Center, 0.7f, 0.35f, 0.1f);

			switch (CurrentState)
			{
				case State.Walk:
					DoWalk(target, onGround);
					break;
				case State.WindUp:
					DoWindUp();
					break;
				case State.Dash:
					DoDash();
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

		private void DoWalk(Player target, bool onGround)
		{
			float distance = Math.Abs(target.Center.X - NPC.Center.X);
			bool wantsToMove = distance > KeepDistance && !EdgeAhead(NPC.direction, 40f);
			float goalSpeed = wantsToMove ? NPC.direction * WalkSpeed : 0f;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, goalSpeed, 0.1f);

			if (onGround && wantsToMove && (NPC.collideX || target.Center.Y < NPC.Top.Y - 48f))
			{
				NPC.velocity.Y = -8f; // hop over a block, or up toward a player standing higher
			}

			Timer++;
			if (Timer >= WalkTime && onGround && Vector2.Distance(NPC.Center, target.Center) < 800f)
			{
				Timer = 0;
				CurrentState = State.WindUp;
				NPC.netUpdate = true;
			}
		}

		// The tell: it stops, rears back, and fire gathers at its mouth. Move out of its line before it goes.
		private void DoWindUp()
		{
			NPC.velocity.X *= 0.7f;
			Vector2 mouth = NPC.Center + new Vector2(NPC.direction * 30f, -8f);

			if (Timer == 0)
			{
				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
			}

			for (int i = 0; i < 2; i++)
			{
				Vector2 spawn = mouth + Main.rand.NextVector2CircularEdge(40f, 40f);
				Dust dust = Dust.NewDustPerfect(spawn, DustID.Torch, (mouth - spawn) * 0.1f, 0, default, 1.5f);
				dust.noGravity = true;
			}

			Timer++;
			if (Timer >= WindUpTime)
			{
				DashDirection = NPC.direction; // from now on the path is fixed
				Timer = 0;
				CurrentState = State.Dash;
				NPC.netUpdate = true;
			}
		}

		// A fast straight charge. It ends early at a wall or an island edge.
		private void DoDash()
		{
			int dir = (int)DashDirection;
			NPC.direction = dir;
			NPC.velocity.X = dir * DashSpeed;

			// Flames trail behind it.
			Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, -dir * 3f, 0f, 0, default, 1.6f);
			dust.noGravity = true;

			Timer++;
			if (Timer >= DashTime || NPC.collideX || EdgeAhead(dir, 60f))
			{
				NPC.velocity.X *= 0.3f;
				Timer = 0;
				CurrentState = State.Recover;
				NPC.netUpdate = true;
			}
		}

		private void DoRecover()
		{
			NPC.velocity.X *= 0.92f;
			Timer++;
			if (Timer >= RecoverTime)
			{
				Timer = 0;
				CurrentState = State.Walk;
				NPC.netUpdate = true;
			}
		}

		// Hits harder while dashing.
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			if (CurrentState == State.Dash)
			{
				modifiers.SourceDamage *= DashDamageMultiplier;
			}
		}

		// Anything it touches is set On Fire!.
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			target.AddBuff(BuffID.OnFire, BurnTicks);
		}

		// True if there is no solid ground within ~4 tiles below the spot `distance` pixels ahead in `dir`.
		private bool EdgeAhead(int dir, float distance)
		{
			int x = (int)((NPC.Center.X + dir * distance) / 16f);
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

		// Picks which frame of the sheet to draw (visuals only, runs on each player's own game).
		public override void FindFrame(int frameHeight)
		{
			int frame;
			switch (CurrentState)
			{
				case State.WindUp:
					frame = 2;
					break;
				case State.Dash:
					frame = 3;
					break;
				default:
					bool walking = Math.Abs(NPC.velocity.X) > 0.4f && NPC.velocity.Y == 0f;
					if (walking)
					{
						NPC.frameCounter++;
						frame = (int)(NPC.frameCounter / 8) % 2; // swap legs every 8 ticks
					}
					else
					{
						frame = 0;
					}
					break;
			}
			NPC.frame.Y = frame * frameHeight;
		}

		// Drops: a chance (placeholder: 1 in 3) of 1 to 2 Heavens Crystals, same as the knight.
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HeavensCrystal>(), 3, 1, 2));
		}
	}
}
