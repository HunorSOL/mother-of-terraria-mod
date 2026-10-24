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

		// PLACEHOLDER caps, to be tuned: how many of each can exist at once.
		private const int MaxKnights = 3;
		private const int MaxDragons = 1;

		public static void Update()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || Main.GameUpdateCount % CheckInterval != 0)
			{
				return;
			}

			// No new Heavens enemies while the Mother of Terraria is alive: the fight is the boss alone.
			if (NPC.AnyNPCs(ModContent.NPCType<NPCs.Bosses.MotherOfTerrariaBoss>()))
			{
				return;
			}

			int knightType = ModContent.NPCType<HeavenlyKnight>();
			int dragonType = ModContent.NPCType<HeavensDragon>();
			int knights = 0, dragons = 0;
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == knightType)
				{
					knights++;
				}
				else if (npc.type == dragonType)
				{
					dragons++;
				}
			}

			// Which types still have room? Pick one at random. Entries are repeated to weight the choice,
			// so knights are picked twice as often as dragons (dragons are the rarer, stronger enemy).
			var open = new List<int>();
			if (knights < MaxKnights)
			{
				open.Add(knightType);
				open.Add(knightType);
			}
			if (dragons < MaxDragons)
			{
				open.Add(dragonType);
			}
			if (open.Count == 0)
			{
				return;
			}
			int type = open[Main.rand.Next(open.Count)];

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

			// Fliers (the dragon) appear in the air 55-80 tiles to one side, just above the player's height.
			if (type == dragonType)
			{
				for (int attempt = 0; attempt < 10; attempt++)
				{
					float airSide = Main.rand.NextBool() ? -1f : 1f;
					Vector2 spawn = target.Center + new Vector2(airSide * Main.rand.Next(55, 81) * 16f, -Main.rand.Next(4, 15) * 16f);
					if (spawn.X < 400f || spawn.X > (Main.maxTilesX - 25) * 16f || spawn.Y < 400f)
					{
						continue;
					}
					if (Collision.SolidCollision(spawn - new Vector2(36f, 22f), 72, 44))
					{
						continue;
					}
					NPC.NewNPC(new EntitySource_WorldEvent(), (int)spawn.X, (int)spawn.Y, type);
					return;
				}
				return;
			}

			// Walkers (the knight) appear on the ground 60-85 tiles to one side (just off-screen), at roughly the player's height.
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

				// Needs a clear space above the ground (the knight is the tallest at 76 px).
				Vector2 feet = new Vector2(tileX * 16f + 8f, groundY * 16f);
				if (Collision.SolidCollision(feet - new Vector2(28f, 80f), 56, 78))
				{
					continue;
				}
				NPC.NewNPC(new EntitySource_WorldEvent(), (int)feet.X, (int)feet.Y, type);
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
