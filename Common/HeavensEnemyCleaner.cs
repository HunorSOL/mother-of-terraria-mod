using MotherOfTerraria.NPCs.Heavens;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Common
{
	// Removes every Heavens enemy (knights and dragons). Used when the boss summon starts and while the boss is alive,
	// so the boss fight is the boss alone. They vanish in a puff of gold dust WITHOUT dying, so they drop nothing.
	public static class HeavensEnemyCleaner
	{
		public static void Clear()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}
			int knight = ModContent.NPCType<HeavenlyKnight>();
			int dragon = ModContent.NPCType<HeavensDragon>();
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type != knight && npc.type != dragon)
				{
					continue;
				}
				for (int i = 0; i < 16; i++)
				{
					Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GoldCoin, 0f, 0f, 0, default, 1.3f);
					dust.noGravity = true;
				}
				npc.active = false;
				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
				}
			}
		}

		// True while any player's summon sequence or boss fight is running (the Basket's FightActive).
		public static bool FightRunning()
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.GetModPlayer<HeavenlyBasketPlayer>().FightActive)
				{
					return true;
				}
			}
			return false;
		}
	}
}
