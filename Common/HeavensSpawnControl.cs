using System.Collections.Generic;
using System.Linq;
using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace MotherOfTerraria.Common
{
	// A GlobalNPC sees every NPC in the game. This one controls which enemies can spawn inside the Heavens:
	// no vanilla enemies or critters at all. Only NPCs from this mod are allowed to spawn there,
	// so the Heavens enemies added in Phase 2C will work without changing this file.
	public class HeavensSpawnControl : GlobalNPC
	{
		// The "spawn pool" is the list of NPCs the game may pick from when spawning near a player.
		// Runs on the server (the Heavens subserver in multiplayer), which is where spawning is decided.
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			if (!SubworldSystem.IsActive<HeavensSubworld>())
			{
				return;
			}

			foreach (int type in pool.Keys.ToList())
			{
				ModNPC modNpc = ModContent.GetModNPC(type);
				if (modNpc == null || modNpc.Mod != Mod)
				{
					pool.Remove(type);
				}
			}
		}
	}
}
