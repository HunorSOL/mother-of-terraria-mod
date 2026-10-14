using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MotherOfTerraria.Items;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Heavens
{
	// The Heavens Dragon: a long golden wyvern. It flies at the player, gives a short warning (it slows and
	// light gathers at its head), then SWOOPS in a homing strike, flies past, circles back and strikes again.
	// Anything it hits is set On Fire! It is stronger than the Heavenly Knight and spawns less often.
	// Texture: NPCs/Heavens/HeavensDragon.png (owner's design, 8 animation frames of 202x91 made by
	// tools/make_dragon_sheet.py). The art faces RIGHT and is long, so we draw it ourselves (PreDraw) with the
	// HEAD on the NPC's hitbox; the long body trails behind it.
	// ALL NUMBERS BELOW ARE PLACEHOLDERS, to be tuned with the owner after the enemies exist.
	public class HeavensDragon : ModNPC
	{
		// Where the hitbox sits on the picture: a spot just behind the nose (frame coordinates, art facing right).
		private const float HeadAnchorX = 170f;
		private const float HeadAnchorY = 43f;

		private const float ApproachSpeed = 11f;
		private const int ApproachMaxTime = 180;    // ticks (60 per second); gives up circling and strikes anyway
		private const float StrikeRange = 360f;     // starts the warning when this close (pixels)
		private const int TelegraphTime = 30;       // the warning
		private const float StrikeSpeed = 16f;
		private const float StrikeTurnRate = 0.05f; // radians per tick the strike can curve toward the player
		private const int StrikeMaxTime = 55;
		private const int RetreatTime = 50;         // flies off before circling back
		private const float StrikeDamageMultiplier = 1.3f;
		private const int BurnTicks = 60 * 5;       // On Fire! lasts 5 seconds

		private enum State
		{
			Approach = 0,
			Telegraph = 1,
			Strike = 2,
			Retreat = 3,
		}

		private State CurrentState
		{
			get => (State)(int)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private ref float Timer => ref NPC.ai[1];

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 8;
		}

		public override void SetDefaults()
		{
			NPC.width = 72;
			NPC.height = 44;
			NPC.damage = 190;       // contact damage (placeholder; stronger than the knight's 150)
			NPC.defense = 80;       // tuned 2026-10-14
			NPC.lifeMax = 50000;    // tuned 2026-10-14: sturdier than the knight
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath8;
			NPC.knockBackResist = 0.05f; // takes a little knockback (owner, 2026-10-14; 1 = full, 0 = immune)
			NPC.value = Item.buyPrice(gold: 4);
			NPC.aiStyle = -1;       // our own AI below
			NPC.noGravity = true;   // it flies
			NPC.noTileCollide = true; // like the vanilla wyverns, it flies through the islands instead of getting stuck
			NPC.buffImmune[BuffID.OnFire] = true;
			NPC.buffImmune[BuffID.Confused] = true;
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];

			// No living player left: fly up and away, and despawn.
			if (!target.active || target.dead)
			{
				NPC.velocity.Y -= 0.3f;
				NPC.EncourageDespawn(60);
				return;
			}

			Lighting.AddLight(NPC.Center, 0.8f, 0.7f, 0.3f);

			switch (CurrentState)
			{
				case State.Approach:
					DoApproach(target);
					break;
				case State.Telegraph:
					DoTelegraph();
					break;
				case State.Strike:
					DoStrike(target);
					break;
				case State.Retreat:
					DoRetreat(target);
					break;
			}

			// Face the way it is flying (with a little hysteresis so it does not flicker when nearly still).
			if (Math.Abs(NPC.velocity.X) > 1.5f)
			{
				NPC.direction = NPC.velocity.X > 0f ? 1 : -1;
			}

			// Lean into its flight path. Gentle most of the time, steeper during the strike.
			float angle = NPC.velocity.ToRotation();
			float rotation = NPC.direction == 1 ? MathHelper.WrapAngle(angle) : MathHelper.WrapAngle(angle - MathHelper.Pi);
			float limit = CurrentState == State.Strike ? 1.1f : 0.5f;
			NPC.rotation = MathHelper.Lerp(NPC.rotation, MathHelper.Clamp(rotation, -limit, limit), 0.15f);
		}

		// Fly toward the player (bobbing a little), then warn and strike.
		private void DoApproach(Player target)
		{
			float bob = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 30f;
			Vector2 goal = target.Center + new Vector2(0f, -40f + bob);
			Vector2 desired = (goal - NPC.Center).SafeNormalize(Vector2.UnitX) * ApproachSpeed;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.05f);

			Timer++;
			if (Vector2.Distance(NPC.Center, target.Center) < StrikeRange || Timer >= ApproachMaxTime)
			{
				Timer = 0;
				CurrentState = State.Telegraph;
				NPC.netUpdate = true;
			}
		}

		// The warning: it slows, roars, and golden-blue light streams into its head. Get ready to dodge sideways.
		private void DoTelegraph()
		{
			NPC.velocity *= 0.9f;
			Vector2 head = HeadPosition();

			if (Timer == 0)
			{
				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
			}

			for (int i = 0; i < 2; i++)
			{
				Vector2 spawn = head + Main.rand.NextVector2CircularEdge(60f, 60f);
				int type = Main.rand.NextBool() ? DustID.GoldCoin : DustID.GemSapphire;
				Dust dust = Dust.NewDustPerfect(spawn, type, (head - spawn) * 0.1f, 0, default, 1.4f);
				dust.noGravity = true;
			}

			Timer++;
			if (Timer >= TelegraphTime)
			{
				Timer = 0;
				CurrentState = State.Strike;
				NPC.netUpdate = true;
			}
		}

		// A homing swoop: it speeds up and curves toward the player, but can only turn so fast,
		// so a well-timed sidestep makes it miss and fly past.
		private void DoStrike(Player target)
		{
			Vector2 toTarget = target.Center - NPC.Center;
			float currentAngle = NPC.velocity.LengthSquared() > 1f ? NPC.velocity.ToRotation() : toTarget.ToRotation();
			float wanted = toTarget.ToRotation();
			float difference = MathHelper.WrapAngle(wanted - currentAngle);
			float newAngle = currentAngle + MathHelper.Clamp(difference, -StrikeTurnRate, StrikeTurnRate);

			float speed = Math.Min(NPC.velocity.Length() + 0.9f, StrikeSpeed);
			NPC.velocity = newAngle.ToRotationVector2() * speed;

			// Golden-blue sparkles trail behind the strike.
			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustPerfect(HeadPosition(), Main.rand.NextBool() ? DustID.GoldCoin : DustID.GemSapphire,
					-NPC.velocity * 0.1f, 0, default, 1.3f);
				dust.noGravity = true;
			}

			Timer++;
			bool passedPlayer = Vector2.Dot(NPC.velocity, toTarget) < 0f && toTarget.Length() > 120f;
			if (Timer >= StrikeMaxTime || passedPlayer)
			{
				Timer = 0;
				CurrentState = State.Retreat;
				NPC.netUpdate = true;
			}
		}

		// After the strike it flies off to a spot far to one side and above, then comes back.
		private void DoRetreat(Player target)
		{
			float side = NPC.Center.X < target.Center.X ? -1f : 1f; // go to the side it is already on
			Vector2 goal = target.Center + new Vector2(side * 520f, -260f);
			Vector2 desired = (goal - NPC.Center).SafeNormalize(Vector2.UnitX) * ApproachSpeed;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desired, 0.06f);

			Timer++;
			if (Timer >= RetreatTime)
			{
				Timer = 0;
				CurrentState = State.Approach;
				NPC.netUpdate = true;
			}
		}

		// Where the head is in the world (a bit in front of the hitbox centre, along the flight direction).
		private Vector2 HeadPosition()
		{
			return NPC.Center + new Vector2(NPC.direction * 30f, 0f).RotatedBy(NPC.rotation);
		}

		// Hits harder while striking.
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			if (CurrentState == State.Strike)
			{
				modifiers.SourceDamage *= StrikeDamageMultiplier;
			}
		}

		// Anything it touches is set On Fire!.
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			target.AddBuff(BuffID.OnFire, BurnTicks);
		}

		// The body ripples and the wings flap: 8 frames, played faster the faster it flies.
		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter += 1.0 + NPC.velocity.Length() * 0.08;
			int frame = (int)(NPC.frameCounter / 4) % 8;
			NPC.frame.Y = frame * frameHeight;
		}

		// The picture is much longer than the hitbox and its head is at the right end, so draw it by hand:
		// pin the head spot of the picture to the hitbox centre, and let the body trail behind it.
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Rectangle frame = NPC.frame;
			Vector2 origin = new Vector2(HeadAnchorX, HeadAnchorY);
			SpriteEffects effects = SpriteEffects.None;
			if (NPC.direction == -1)
			{
				effects = SpriteEffects.FlipHorizontally;
				origin.X = frame.Width - HeadAnchorX;
			}
			spriteBatch.Draw(texture, NPC.Center - screenPos, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects, 0f);
			return false;
		}

		// Drops: a better chance than the knight (placeholder: 1 in 2) of 1 to 3 Heavens Crystals.
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<HeavensCrystal>(), 2, 1, 3));
		}
	}
}
