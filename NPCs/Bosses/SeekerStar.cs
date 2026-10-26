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
	// One of the twenty homing stars of the STAR HELL attack (owner design, 2026-10-26; see MotherOfTerrariaBoss.DoStarHell). It is a real (small) enemy
	// so the player's weapons can shoot it down: exactly 3000 health, defense 5. It fans out from her, then LOCKS ON and homes on the player extremely hard
	// (it turns almost at once, so sidestepping does not work); it is fast but a player with wings or boots can outrun it. If it touches the player it
	// hurts (400, ignoring armor) and explodes; if it is shot down it pops harmlessly; after SeekerLife ticks it fizzles out. It drops nothing.
	// ai[0] = the boss's NPC index, ai[1] = the direction (radians) it fans out in. localAI[0] = its age in ticks.
	// ALL NUMBERS ARE PLACEHOLDERS (Phase 6 balances them). SINGLEPLAYER-ONLY: none known; not tested in multiplayer.
	public class SeekerStar : ModNPC
	{
		public const int StarLife = 3000;          // owner, 2026-10-26: 1000 first, tripled the same day to make it harder
		public const int SeekerLife = 600;         // ticks before it gives up (10 s)
		private const float Speed = 10.8f;         // pixels per tick (was 9, +20%, owner 2026-10-26): fast, but wings and boots can beat it
		private const float FanSpeed = 13f;        // the first moments, while it fans out
		private const int FanTicks = 22;           // how long it fans out before it locks on
		private const float LockTurn = 0.2f;       // how much of its velocity it swaps for "straight at you" every tick: very strong homing

		// The same picture as the falling stars (no file of its own).
		public override string Texture => "MotherOfTerraria/Projectiles/GoldCrystalStar";

		public override void SetStaticDefaults()
		{
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Hide = true };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 40;
			NPC.height = 40;
			NPC.damage = 400;       // owner, 2026-10-26: 200 first, then 300, then 400
			NPC.defense = 5;        // owner, 2026-10-26
			NPC.lifeMax = StarLife;
			NPC.HitSound = SoundID.Item27;      // glass
			NPC.DeathSound = null;              // the pop is made in Pop()
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

		// The game scales health by difficulty; put it back to exactly StarLife.
		public override void OnSpawn(IEntitySource source)
		{
			NPC.lifeMax = StarLife;
			NPC.life = StarLife;
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)NPC.ai[0]];
			if (!owner.active || owner.ModNPC is not MotherOfTerrariaBoss)
			{
				Pop();
				NPC.active = false;
				return;
			}
			NPC.timeLeft = 600;
			NPC.localAI[0]++;
			Player target = Main.player[owner.target];
			if (!target.active || target.dead || NPC.localAI[0] > SeekerLife)
			{
				Pop();
				NPC.active = false; // it fizzles out
				return;
			}

			if (NPC.localAI[0] <= FanTicks)
			{
				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.ai[1].ToRotationVector2() * FanSpeed, 0.2f);
			}
			else
			{
				Vector2 straight = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * Speed;
				NPC.velocity = Vector2.Lerp(NPC.velocity, straight, LockTurn);
				if (NPC.localAI[0] == FanTicks + 1)
				{
					SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.5f, Pitch = 0.4f }, NPC.Center); // the lock-on
				}
			}
			NPC.rotation += 0.12f;
			Lighting.AddLight(NPC.Center, 1.1f, 0.9f, 0.35f);

			// A blue and gold spark trail behind it.
			Dust dust = Dust.NewDustPerfect(NPC.Center - NPC.velocity * 1.5f + Main.rand.NextVector2Circular(10f, 10f),
				Main.rand.NextBool(3) ? DustID.GoldCoin : DustID.GemSapphire, -NPC.velocity * 0.1f, 0, default, 1.2f);
			dust.noGravity = true;
		}

		// Armor penetration (owner, 2026-10-26: the player's armor was cancelling the whole hit): a touch ignores ALL of the player's defense.
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			modifiers.ScalingArmorPenetration += 1f;
		}

		// It touched the player (200 damage is the contact damage): it explodes.
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			Pop();
			NPC.active = false;
		}

		// Shot down: it pops harmlessly (no loot).
		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0)
			{
				Pop();
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = TextureAssets.Npc[Type].Value;
			float scale = 0.62f * (1f + 0.08f * (float)System.Math.Sin(NPC.localAI[0] * 0.4f)); // 40 px hitbox, picture matched to it
			float fade = MathHelper.Clamp(NPC.localAI[0] / 10f, 0f, 1f);
			spriteBatch.Draw(texture, NPC.Center - screenPos, null, Color.White * fade, NPC.rotation, new Vector2(40.5f, 41.5f), scale, SpriteEffects.None, 0f);
			return false;
		}

		private void Pop()
		{
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.55f, Pitch = 0.3f }, NPC.Center);
			for (int i = 0; i < 20; i++)
			{
				Dust dust = Dust.NewDustPerfect(NPC.Center, i % 3 == 0 ? DustID.GemSapphire : DustID.GoldCoin, Main.rand.NextVector2Circular(7f, 7f), 0, default, 1.5f);
				dust.noGravity = true;
			}
		}
	}
}
