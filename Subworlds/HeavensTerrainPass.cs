using System;
using MotherOfTerraria.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace MotherOfTerraria.Subworlds
{
	// The world generation step that builds the Heavens: long floating islands joined by cloud bridges,
	// golden statues and trees. The layout is hand-authored (fixed coordinates), so every visit is identical.
	// Phase 2C will add finer detail on top of this.
	public class HeavensTerrainPass : GenPass
	{
		// Fixed seed so tree shapes come out the same every time too.
		private const int Seed = 777;

		// One island: centre column, height of its top surface, half-width, thickness at the middle,
		// how far the top edges droop below the middle (0 = perfectly flat), and whether it is the boss arena.
		private record Island(int Cx, int Top, int HalfWidth, int Thickness, int EdgeDrop, bool Arena = false)
		{
			public int Left => Cx - HalfWidth;
			public int Right => Cx + HalfWidth;

			// Row of the top surface in a given column: a gentle arch, highest in the middle.
			public int SurfaceY(int x)
			{
				double d = Math.Abs(x - Cx) / (double)HalfWidth;
				return Top + (int)Math.Round(EdgeDrop * d * d);
			}

			// How many tiles thick the island is at a column: thickest in the middle, tapering to the edges.
			public int Depth(int x)
			{
				double d = Math.Abs(x - Cx) / (double)HalfWidth;
				return Math.Max(3, (int)Math.Round(Thickness * (1 - d * d)));
			}
		}

		private static readonly Island Arrival = new(155, 235, 115, 26, 5);
		private static readonly Island High = new(400, 200, 90, 20, 4);
		private static readonly Island Low = new(390, 270, 90, 22, 4);
		private static readonly Island Arena = new(655, 245, 125, 34, 0, Arena: true); // wide and flat: the future boss fight

		// Long islands that players can walk on.
		private static readonly Island[] MainIslands = { Arrival, High, Low, Arena };

		// Small decorative islands with no bridges.
		private static readonly Island[] Floaters =
		{
			new(60, 160, 26, 10, 2),
			new(720, 140, 32, 12, 2),
			new(400, 120, 22, 8, 2),
			new(540, 190, 18, 7, 1),
		};

		// Bridges run from the right edge of one island to the left edge of the next.
		private static readonly (Island From, Island To)[] Bridges =
		{
			(Arrival, High),
			(Arrival, Low),
			(High, Arena),
			(Low, Arena),
		};

		// Statues: which island and which column. They stand flanking the arrival point,
		// guarding the arena, and crowning the middle islands.
		private static readonly (Island On, int X)[] Statues =
		{
			(Arrival, 100), (Arrival, 210),
			(High, 400),
			(Low, 390),
			(Arena, 570), (Arena, 740),
		};

		// "Heavens Terrain" is the step name shown in logs; the number is its rough weight for the progress bar.
		public HeavensTerrainPass() : base("Heavens Terrain", 1) { }

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			// Make the game's world-gen random numbers repeatable, so trees look the same every visit.
			WorldGen._genRand = new UnifiedRandom(Seed);

			// Tell the game where "surface" and "underground" are, so the sky and lighting render as open sky.
			Main.worldSurface = 330;
			Main.rockLayer = 360;

			progress.Message = "Raising the islands";
			foreach (Island island in MainIslands)
			{
				BuildIsland(island);
			}
			foreach (Island island in Floaters)
			{
				BuildIsland(island);
			}
			progress.Set(0.3f);

			progress.Message = "Laying the bridges";
			foreach (var (from, to) in Bridges)
			{
				BuildBridge(from, to);
			}
			progress.Set(0.5f);

			// Fix up tile shapes so clouds and grass blend smoothly instead of looking like raw squares.
			progress.Message = "Smoothing the clouds";
			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = 80; y < 340; y++)
				{
					if (Main.tile[x, y].HasTile)
					{
						WorldGen.SquareTileFrame(x, y, true);
					}
				}
			}
			progress.Set(0.7f);

			progress.Message = "Raising the statues";
			foreach (var (on, x) in Statues)
			{
				PlaceStatue(on, x);
			}
			progress.Set(0.8f);

			progress.Message = "Growing the trees";
			int grown = 0, tried = 0;
			foreach (Island island in new[] { Arrival, High, Low, Arena })
			{
				for (int x = island.Left + 6; x <= island.Right - 6; x += 11)
				{
					// The arena stays open in the middle; only its outer edges get trees.
					if (island.Arena && x - island.Left > 24 && island.Right - x > 24)
					{
						continue;
					}
					if (NearStatue(x))
					{
						continue;
					}
					tried++;
					if (PlantTree(island, x))
					{
						grown++;
					}
				}
			}
			Mod.Logger.Info($"Heavens trees: {grown} grown of {tried} tried.");
			progress.Set(1f);

			// Players arrive on the grand arrival island, between its two statues.
			Main.spawnTileX = Arrival.Cx;
			Main.spawnTileY = Arrival.SurfaceY(Arrival.Cx) - 1;
		}

		private static Mod Mod => ModContent.GetInstance<MotherOfTerraria>();

		private static void BuildIsland(Island island)
		{
			for (int x = island.Left; x <= island.Right; x++)
			{
				int top = island.SurfaceY(x);
				int depth = island.Depth(x);
				for (int k = 0; k < depth; k++)
				{
					SetTile(x, top + k, k == 0 ? TileID.HallowedGrass : TileID.Cloud);
				}
			}
		}

		// A thin cloud path that sags a little in the middle.
		private static void BuildBridge(Island from, Island to)
		{
			int x1 = from.Right - 2, x2 = to.Left + 2;
			int y1 = from.SurfaceY(x1), y2 = to.SurfaceY(x2);
			for (int x = x1; x <= x2; x++)
			{
				double t = (x - x1) / (double)(x2 - x1);
				int y = (int)Math.Round(y1 + (y2 - y1) * t + 3 * Math.Sin(Math.PI * t));
				for (int k = 0; k < 3; k++)
				{
					SetTile(x, y + k, TileID.Cloud);
				}
			}
		}

		// Levels a small pad under the statue, clears the air above it, then places the statue.
		private static void PlaceStatue(Island island, int x)
		{
			int ground = island.SurfaceY(x);
			for (int dx = -1; dx <= 1; dx++)
			{
				SetTile(x + dx, ground, TileID.HallowedGrass);
				for (int k = 1; k <= 3; k++)
				{
					SetTile(x + dx, ground + k, TileID.Cloud);
				}
				for (int k = 1; k <= 8; k++)
				{
					Tile air = Main.tile[x + dx, ground - k];
					air.HasTile = false;
				}
				WorldGen.SquareTileFrame(x + dx, ground, true);
			}
			WorldGen.PlaceObject(x, ground - 1, ModContent.TileType<GoldenStatue>(), true);
		}

		private static bool NearStatue(int x)
		{
			foreach (var (_, sx) in Statues)
			{
				if (Math.Abs(sx - x) <= 6)
				{
					return true;
				}
			}
			return false;
		}

		// Plants a sapling on the grass and grows it instantly into a tree. Returns false if the game refused.
		private static bool PlantTree(Island island, int x)
		{
			int ground = island.SurfaceY(x);
			WorldGen.PlaceTile(x, ground - 1, TileID.Saplings, true, true);
			return WorldGen.GrowTree(x, ground - 1);
		}

		private static void SetTile(int x, int y, int type)
		{
			Tile tile = Main.tile[x, y];
			tile.HasTile = true;
			tile.TileType = (ushort)type;
		}
	}
}
