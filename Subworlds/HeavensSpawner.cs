using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MotherOfTerraria.NPCs.Heavens;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Subworlds
{
	// Decides when and where Heavens enemies appear. We do this by hand (instead of Terraria's normal
	// spawn system) so we control exactly how many exist and where they come from.
	// Called every tick from HeavensSubworld.Update, which only runs on the server / in singleplayer.
	public static class HeavensSpawner
	{
		private const int CheckInterval = 240; // ticks (60 per second) between spawn attempts
		private const int MaxKnights = 4;      // PLACEHOLDER cap, to be tuned

		public static void Update()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || Main.GameUpdateCount % CheckInterval != 0)
			{
				return;
			}

			int knightType = ModContent.NPCType<HeavenlyKnight>();
			int knights = 0;
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == knightType)
				{
					knights++;
				}
			}
			if (knights >= MaxKnights)
			{
				return;
			}

			// Pick a random living player to spawn near.
			var players = new List<Player>();
			foreach (Player player in Main.ActivePlayers)
			{
				if (!player.dead)
				{
					players.Add(player);
				}
			}
			if (players.Count == 0)
			{
				return;
			}
			Player target = players[Main.rand.Next(players.Count)];

			// Appear in the air 55-80 tiles to one side, just off-screen. Skip spots inside solid terrain.
			for (int attempt = 0; attempt < 10; attempt++)
			{
				float side = Main.rand.NextBool() ? -1f : 1f;
				Vector2 spawn = target.Center + new Vector2(side * Main.rand.Next(55, 81) * 16f, -Main.rand.Next(4, 15) * 16f);
				if (spawn.X < 400f || spawn.X > (Main.maxTilesX - 25) * 16f || spawn.Y < 400f)
				{
					continue;
				}
				if (Collision.SolidCollision(spawn - new Vector2(24f, 32f), 48, 64))
				{
					continue;
				}
				NPC.NewNPC(new EntitySource_WorldEvent(), (int)spawn.X, (int)spawn.Y, knightType);
				return;
			}
		}
	}
}
