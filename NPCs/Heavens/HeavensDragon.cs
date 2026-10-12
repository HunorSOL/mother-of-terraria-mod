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
	// The Heavens Dragon: a small wyvern-like flier. It hovers beside and above the player, then rears back
	// with its mouth glowing (the tell) and DASHES straight at where the player was. Anything it hits is set
	// On Fire! After the dash it is exposed for a moment.
	// Texture: NPCs/Heavens/HeavensDragon.png (PLACEHOLDER sheet made by tools/make_dragon_placeholder.py;
	// the owner has no design yet). It faces RIGHT, like the knight.
	// ALL NUMBERS BELOW ARE PLACEHOLDERS, to be tuned with the owner after the enemies exist.
	public class HeavensDragon : ModNPC
	{
		private const int HoverTime = 100;          // ticks (60 per second) of circling before a dash
		private const int WindUpTime = 50;          // the tell: it stops, rears back, and its mouth glows
		private const float DashSpeed = 14f;
		private const int DashTime = 24;            // about 21 tiles of travel
		private const int RecoverTime = 70;         // exposed after the dash
		private const float DashDamageMultiplier = 1.5f;
		private const int BurnTicks = 60 * 5;       // On Fire! lasts 5 seconds

		private enum State
		{
			Hover = 0,
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
		private ref float DashAngle => ref NPC.ai[2]; // direction of the dash in radians, locked when it starts; synced

		public override void SetStaticDefaults()
		{
			// The sheet stacks 4 frames: 0 = wings up, 1 = wings down, 2 = wind-up, 3 = dash.
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
			NPC.noGravity = true;   // it flies
			NPC.noTileCollide = false;
			NPC.buffImmune[BuffID.OnFire] = true; // a fire dragon does not burn
			NPC.buffImmune[BuffID.Confused] = true;
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];

			// No living player left: fly up and away, and despawn.
			if (!target.active || target.dead)
			{
				NPC.velocity.Y -= 0.2f;
				NPC.EncourageDespawn(60);
				return;
			}

			// It keeps facing the player, except during a dash, when it faces along its path.
			if (CurrentState == State.Dash)
			{
				NPC.direction = Math.Cos(DashAngle) >= 0 ? 1 : -1;
			}
			else
			{
				NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
			}
			// The art faces right and Terraria flips when spriteDirection is 1, hence the reversed sign
			// (same as the knight).
			NPC.spriteDirection = -NPC.direction;

			Lighting.AddLight(NPC.Center, 0.7f, 0.35f, 0.1f);

			switch (CurrentState)
			{
				case State.Hover:
					DoHover(target);
					break;
				case State.WindUp:
					DoWindUp(target);
					break;
				case State.Dash:
					DoDash();
					break;
				case State.Recover:
					DoRecover();
					break;
			}

			// Tilt: a gentle lean into its movement; during the dash, nose pointing along the path.
			if (CurrentState == State.Dash)
			{
				float angle = DashAngle;
				NPC.rotation = NPC.direction == 1 ? angle : MathHelper.WrapAngle(angle - MathHelper.Pi);
			}
			else
			{
				NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.02f, -0.3f, 0.3f);
			}
		}

		// Circle beside and above the player, bobbing gently, then pick the next action.
		private void DoHover(Player target)
		{
			float side = NPC.Center.X < target.Center.X ? -1f : 1f; // stay on the side it is already on
			float bob = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 20f;
			Vector2 goal = target.Center + new Vector2(side * 300f, -170f + bob);
			Vector2 desired = (goal - NPC.Center) * 0.06f;
			if (desired.Length() > 9f)
			{
				desired = desired.SafeNormalize(Vector2.Zero) * 9f;
			}
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.08f);

			Timer++;
			if (Timer >= HoverTime && Vector2.Distance(NPC.Center, target.Center) < 900f)
			{
				Timer = 0;
				CurrentState = State.WindUp;
				NPC.netUpdate = true;
			}
		}

		// The tell: it stops, rears back, and fire gathers at its mouth. Move out of its line before it goes.
		private void DoWindUp(Player target)
		{
			NPC.velocity *= 0.85f;
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
				// Lock the path at the player's current position: moving away in time dodges it.
				DashAngle = (target.Center - NPC.Center).ToRotation();
				Timer = 0;
				CurrentState = State.Dash;
				NPC.netUpdate = true;
			}
		}

		// A fast straight charge. It ends early if it hits terrain.
		private void DoDash()
		{
			NPC.velocity = DashAngle.ToRotationVector2() * DashSpeed;

			// Flames trail behind it.
			Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, -NPC.velocity.X * 0.2f, -NPC.velocity.Y * 0.2f, 0, default, 1.6f);
			dust.noGravity = true;

			Timer++;
			if (Timer >= DashTime || NPC.collideX || NPC.collideY)
			{
				NPC.velocity *= 0.3f;
				Timer = 0;
				CurrentState = State.Recover;
				NPC.netUpdate = true;
			}
		}

		private void DoRecover()
		{
			NPC.velocity *= 0.93f;
			Timer++;
			if (Timer >= RecoverTime)
			{
				Timer = 0;
				CurrentState = State.Hover;
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
					NPC.frameCounter++;
					frame = (int)(NPC.frameCounter / 7) % 2; // wings up, wings down, every 7 ticks
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
