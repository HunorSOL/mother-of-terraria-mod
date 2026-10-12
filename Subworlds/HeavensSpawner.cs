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
		private const int MaxKnights = 3;      // PLACEHOLDER cap, to be tuned

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

			// Appear on the ground 60-85 tiles to one side (just off-screen), at roughly the player's height.
			for (int attempt = 0; attempt < 10; attempt++)
			{
				int side = Main.rand.NextBool() ? -1 : 1;
				int tileX = (int)(target.Center.X / 16f) + side * Main.rand.Next(60, 86);
				if (tileX < 20 || tileX > Main.maxTilesX - 20)
				{
					continue;
				}

				int groundY = FindGround(tileX, (int)(target.Center.Y / 16f) - 8, (int)(target.Center.Y / 16f) + 16);
				if (groundY < 0)
				{
					continue; // nothing to stand on in that column (a gap between islands)
				}

				// Needs a clear 2x5 tile space above the ground.
				Vector2 feet = new Vector2(tileX * 16f + 8f, groundY * 16f);
				if (Collision.SolidCollision(feet - new Vector2(20f, 80f), 40, 78))
				{
					continue;
				}
				NPC.NewNPC(new EntitySource_WorldEvent(), (int)feet.X, (int)feet.Y, knightType);
				return;
			}
		}

		// Finds the first solid tile (with open air above it) between two rows in a column. Returns -1 if none.
		private static int FindGround(int x, int fromY, int toY)
		{
			fromY = System.Math.Max(fromY, 10);
			toY = System.Math.Min(toY, Main.maxTilesY - 10);
			for (int y = fromY; y < toY; y++)
			{
				Tile tile = Main.tile[x, y];
				Tile above = Main.tile[x, y - 1];
				if (tile.HasTile && Main.tileSolid[tile.TileType] && !above.HasTile)
				{
					return y;
				}
			}
			return -1;
		}
	}
}
