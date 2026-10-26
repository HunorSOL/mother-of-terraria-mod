using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.NPCs.Bosses
{
	// A false Mother of Terraria for the MIRROR COURT attack (owner design, 2026-10-26; see MotherOfTerrariaBoss.DoMirrorCourt). Three of
	// these appear around the player together with the real her, and all four close in slowly. A clone cannot be hurt (attacks pass
	// through it), but it does hurt the player if it reaches them. It looks like her stage 2 picture and flaps in step with her, but without
	// the faint halo the real one has. ai[0] = the boss's NPC index. It vanishes when the boss ends the attack (or is gone).
	// SINGLEPLAYER-ONLY: none known; not tested in multiplayer.
	public class MirrorClone : ModNPC
	{
		private const float StopDistance = 50f;   // it hovers this close to the player once it arrives

		// Same picture as her stage 2 sheet (no file of its own).
		public override string Texture => "MotherOfTerraria/NPCs/Bosses/MotherOfTerrariaBoss_Stage2";

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 8;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Hide = true };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults()
		{
			NPC.width = 130;
			NPC.height = 210;
			NPC.damage = 200;        // contact damage, like hers (placeholder)
			NPC.defense = 0;
			NPC.lifeMax = 100;
			NPC.dontTakeDamage = true; // attacks pass through: the tell that it is a clone
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

		public override void AI()
		{
			NPC owner = Main.npc[(int)NPC.ai[0]];
			if (!owner.active || owner.ModNPC is not MotherOfTerrariaBoss)
			{
				NPC.active = false;
				return;
			}
			NPC.timeLeft = 600;
			Player target = Main.player[owner.target];
			Vector2 toPlayer = target.Center - NPC.Center;
			NPC.velocity = toPlayer.Length() > StopDistance ? toPlayer.SafeNormalize(Vector2.Zero) * MotherOfTerrariaBoss.MirrorSpeed : Vector2.Zero;
			NPC.direction = toPlayer.X > 0f ? 1 : -1;
			Lighting.AddLight(NPC.Center, 1f, 0.85f, 0.4f);
		}

		// Flaps in step with the real her, so the wings never give a clone away.
		public override void FindFrame(int frameHeight)
		{
			NPC owner = Main.npc[(int)NPC.ai[0]];
			NPC.frame.Y = owner.active ? owner.frame.Y : 0;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = TextureAssets.Npc[Type].Value;
			spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), NPC.frame, drawColor, 0f, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0f);
			return false;
		}

		// A puff of gold dust when it is removed by the boss.
		public static void Vanish(NPC clone)
		{
			for (int i = 0; i < 40; i++)
			{
				Dust dust = Dust.NewDustPerfect(clone.Center + Main.rand.NextVector2Circular(60f, 100f), DustID.GoldCoin, Main.rand.NextVector2Circular(6f, 6f), 0, default, 1.6f);
				dust.noGravity = true;
			}
			clone.active = false;
		}
	}
}
