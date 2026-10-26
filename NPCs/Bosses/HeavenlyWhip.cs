using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Bosses
{
	// The HEAVENLY WHIP of the Mother of Terraria's stage 2 attack (owner's idea, 2026-10-26; see MotherOfTerrariaBoss.DoHeavenlyWhip). It is an NPC (and not a projectile)
	// so that the player's weapons can hit it. States (ai[2]):
	//   0 AIM:     it sits at her hand while a thin line follows the player, then the line locks and turns thick (the tell, FollowTicks + LockTicks).
	//   1 LASH:    it flies down the locked line. If it touches the player, they are CAUGHT. If it flies its whole range without catching anyone it retracts.
	//   2 CAUGHT:  it wraps the player: they cannot move, and it hurts them CaughtDamage every CaughtEvery ticks (100 a second) until it is destroyed (7,000 health).
	//   3 RETRACT: it flies back to her and vanishes.
	// ai[0] = the boss's NPC index, ai[1] = ticks in the current state, ai[2] = state, ai[3] = the locked angle.
	// localAI[0], localAI[1] = where the caught player is pinned, localAI[2] = ticks until the next hurt, localAI[3] = the follow angle during the aim.
	// WHIP ATTACK: the owner is not sure about it. To remove it: delete this file, tools/make_whip.py, NPCs/Bosses/HeavenlyWhip.png and everything marked "WHIP" in the boss class.
	// ALL NUMBERS ARE PLACEHOLDERS. SINGLEPLAYER-ONLY: the pinning and the damage run on this machine (the player's own state); the multiplayer upgrade must move them.
	public class HeavenlyWhip : ModNPC
	{
		public const int WhipLife = 7000;               // owner, 2026-10-26
		public const int FollowTicks = 45;
		public const int LockTicks = 20;
		private const float LashSpeed = 30f;
		private const float LashRange = 1300f;
		private const float RetractSpeed = 45f;
		private const int CaughtDamage = 100;           // owner: "100 damage every second"
		private const int CaughtEvery = 60;
		private const float LineHalfWidth = 40f;

		private ref float Ticks => ref NPC.ai[1];
		private ref float State => ref NPC.ai[2];
		private ref float LockedAngle => ref NPC.ai[3];

		public override void SetStaticDefaults()
		{
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Hide = true };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 90;
			NPC.height = 100;
			NPC.damage = 0;                   // it never hurts by touch; the catch and the damage over time are done in AI
			NPC.defense = 0;
			NPC.lifeMax = WhipLife;
			NPC.HitSound = SoundID.Item27;    // glass, like the crystals
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

		// The game scales health by difficulty; set it back to exactly WhipLife, as for the boss.
		public override void OnSpawn(IEntitySource source)
		{
			NPC.lifeMax = WhipLife;
			NPC.life = WhipLife;
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)NPC.ai[0]];
			if (!owner.active || owner.type != ModContent.NPCType<MotherOfTerrariaBoss>())
			{
				NPC.active = false; // she is gone, so is it
				return;
			}
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];
			if (!target.active || target.dead)
			{
				NPC.active = false;
				return;
			}
			Ticks++;
			NPC.rotation += State == 1f ? 0.35f : 0.03f;
			Lighting.AddLight(NPC.Center, 0.6f, 0.75f, 1f);

			switch ((int)State)
			{
				case 0: // AIM
					NPC.Center = owner.Center;
					NPC.velocity = Vector2.Zero;
					if (Ticks <= FollowTicks)
					{
						NPC.localAI[3] = (target.Center - NPC.Center).ToRotation(); // the line keeps pointing at the player until it locks
					}
					if (Ticks == FollowTicks)
					{
						SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f }, NPC.Center); // the lock
					}
					if (Ticks >= FollowTicks + LockTicks)
					{
						LockedAngle = NPC.localAI[3];
						State = 1f;
						Ticks = 0f;
						NPC.velocity = LockedAngle.ToRotationVector2() * LashSpeed;
						SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
						NPC.netUpdate = true;
					}
					break;

				case 1: // LASH
					if (NPC.Hitbox.Intersects(target.Hitbox))
					{
						Catch(target);
					}
					else if (Ticks * LashSpeed >= LashRange)
					{
						State = 3f;
						Ticks = 0f;
						NPC.netUpdate = true;
					}
					break;

				case 2: // CAUGHT
					Vector2 pin = new Vector2(NPC.localAI[0], NPC.localAI[1]);
					target.position = pin;               // the player cannot move (not even with a teleport or a hook)
					target.velocity = Vector2.Zero;
					target.RemoveAllGrapplingHooks();
					NPC.Center = target.Center;
					NPC.velocity = Vector2.Zero;
					NPC.localAI[2]--;
					if (NPC.localAI[2] <= 0f)
					{
						NPC.localAI[2] = CaughtEvery;
						Squeeze(target);
					}
					break;

				case 3: // RETRACT
					Vector2 back = owner.Center - NPC.Center;
					if (back.Length() < RetractSpeed + 40f)
					{
						NPC.active = false; // home again: no drops, no sound
						return;
					}
					NPC.velocity = back.SafeNormalize(Vector2.Zero) * RetractSpeed;
					break;
			}
		}

		private void Catch(Player target)
		{
			State = 2f;
			Ticks = 0f;
			NPC.localAI[0] = target.position.X;
			NPC.localAI[1] = target.position.Y;
			NPC.localAI[2] = CaughtEvery;
			NPC.velocity = Vector2.Zero;
			SoundEngine.PlaySound(SoundID.Item56, target.Center);
			for (int i = 0; i < 30; i++)
			{
				Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(40f, 50f), DustID.GoldCoin, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.6f);
				dust.noGravity = true;
			}
			NPC.netUpdate = true;
		}

		// The whip's damage over time: exactly CaughtDamage, whatever the player's armor or invincibility frames (owner: "100 damage every second").
		private void Squeeze(Player target)
		{
			SoundEngine.PlaySound(SoundID.Item56, target.Center);
			target.statLife -= CaughtDamage;
			CombatText.NewText(target.Hitbox, CombatText.DamagedHostile, CaughtDamage);
			if (target.statLife <= 0)
			{
				target.statLife = 0;
				target.KillMe(PlayerDeathReason.ByNPC(NPC.whoAmI), CaughtDamage, 0);
			}
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(30f, 40f), DustID.GoldCoin, Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.3f);
				dust.noGravity = true;
			}
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0)
			{
				SoundEngine.PlaySound(SoundID.Item107, NPC.Center);
				for (int i = 0; i < 50; i++)
				{
					Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(50f, 60f), i % 3 == 0 ? DustID.GemSapphire : DustID.GoldCoin, Main.rand.NextVector2Circular(9f, 9f), 0, default, 1.8f);
					dust.noGravity = true;
				}
			}
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => null; // the normal small health bar shows how close it is to breaking

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle one = new Rectangle(0, 0, 1, 1);
			NPC owner = Main.npc[(int)NPC.ai[0]];
			Vector2 here = NPC.Center - screenPos;
			Vector2 hand = owner.Center - screenPos;
			Vector2 mid = new Vector2(0f, 0.5f);

			if (State == 0f)
			{
				// the aim line: thin and following, then a thick locked corridor (the same style as the other locked lines)
				float angle = NPC.localAI[3];
				float length = LashRange;
				if (Ticks < FollowTicks)
				{
					spriteBatch.Draw(pixel, here, one, new Color(255, 205, 60) * (0.25f + 0.35f * Ticks / FollowTicks), angle, mid, new Vector2(length, 6f), SpriteEffects.None, 0f);
				}
				else
				{
					float lock01 = MathHelper.Clamp((Ticks - FollowTicks) / LockTicks, 0f, 1f);
					float pulse = 0.85f + 0.15f * (float)System.Math.Sin(Ticks * 0.5f);
					float fade = 0.35f + 0.55f * lock01;
					Vector2 side = (angle + MathHelper.PiOver2).ToRotationVector2();
					float width = LineHalfWidth * 2f;
					spriteBatch.Draw(pixel, here, one, new Color(40, 25, 110) * (0.4f * fade), angle, mid, new Vector2(length, width + 10f), SpriteEffects.None, 0f);
					spriteBatch.Draw(pixel, here, one, new Color(255, 205, 60) * (0.28f * fade * pulse), angle, mid, new Vector2(length, width), SpriteEffects.None, 0f);
					for (int s = -1; s <= 1; s += 2)
					{
						spriteBatch.Draw(pixel, here + side * (s * width / 2f), one, new Color(255, 205, 60) * (0.9f * fade * pulse), angle, mid, new Vector2(length, 8f), SpriteEffects.None, 0f);
					}
				}
			}
			else
			{
				// the rope from her hand to the whip: a wavy gold and blue line (a gentle sag while it hangs on the caught player)
				Vector2 span = here - hand;
				float ropeLength = span.Length();
				if (ropeLength > 1f)
				{
					Vector2 along = span / ropeLength;
					Vector2 across = new Vector2(-along.Y, along.X);
					int pieces = System.Math.Max(2, (int)(ropeLength / 24f));
					float wave = State == 2f ? 0.18f : 0.06f;
					Vector2 previous = hand;
					for (int i = 1; i <= pieces; i++)
					{
						float t = i / (float)pieces;
						Vector2 point = hand + span * t + across * ((float)System.Math.Sin(t * 9f + Main.GlobalTimeWrappedHourly * 6f) * ropeLength * wave * 0.05f * System.Math.Min(1f, 4f * t * (1f - t)));
						Vector2 piece = point - previous;
						float rotation = piece.ToRotation();
						spriteBatch.Draw(pixel, previous, one, new Color(40, 25, 110), rotation, mid, new Vector2(piece.Length() + 1f, 12f), SpriteEffects.None, 0f);
						spriteBatch.Draw(pixel, previous, one, new Color(90, 170, 255), rotation, mid, new Vector2(piece.Length() + 1f, 8f), SpriteEffects.None, 0f);
						spriteBatch.Draw(pixel, previous, one, new Color(255, 215, 90), rotation, mid, new Vector2(piece.Length() + 1f, 3f), SpriteEffects.None, 0f);
						previous = point;
					}
				}
			}

			Texture2D whip = TextureAssets.Npc[Type].Value;
			float scale = State == 2f ? 1.5f : 1f;
			spriteBatch.Draw(whip, here, null, Color.White, NPC.rotation, whip.Size() / 2f, scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
