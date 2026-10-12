using System;
using System.Collections.Generic;
using MotherOfTerraria.Tiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace MotherOfTerraria.Subworlds
{
	// The world generation step that builds the Heavens: long floating islands with luminous golden turf,
	// joined by golden bridges, with golden statues and the owner's heavenly trees, saplings and stumps.
	// The layout is hand-authored (fixed coordinates), so every visit is identical.
	public class HeavensTerrainPass : GenPass
	{
		// The island tops are ordinary grass painted this colour and made to glow ("illuminant"), as a stand-in
		// for a custom golden turf tile. Change the colour here if it looks wrong in-game.
		private const byte TurfPaint = PaintID.YellowPaint;

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

		// The islands sit close together (gaps of about 15-20 tiles), at similar heights so the bridges stay gentle.
		private static readonly Island Arrival = new(140, 235, 100, 24, 5);
		private static readonly Island High = new(330, 222, 75, 18, 4);
		private static readonly Island Low = new(335, 250, 75, 20, 4);
		private static readonly Island Arena = new(538, 240, 110, 32, 0, Arena: true); // wide and flat: the future boss fight

		// Long islands that players can walk on.
		private static readonly Island[] MainIslands = { Arrival, High, Low, Arena };

		// Small decorative islands with no bridges.
		private static readonly Island[] Floaters =
		{
			new(60, 165, 26, 10, 2),
			new(610, 145, 32, 12, 2),
			new(330, 150, 22, 8, 2),
			new(425, 178, 16, 7, 1),
		};

		// Bridges run from the right edge of one island to the left edge of the next.
		private static readonly (Island From, Island To)[] Bridges =
		{
			(Arrival, High),
			(Arrival, Low),
			(High, Arena),
			(Low, Arena),
		};

		// Golden statues: which island and which column. They flank the arrival point, crown the middle islands
		// and guard the arena.
		private static readonly (Island On, int X)[] Statues =
		{
			(Arrival, 95), (Arrival, 185),
			(High, 330),
			(Low, 335),
			(Arena, 490), (Arena, 590),
		};

		// "Heavens Terrain" is the step name shown in logs; the number is its rough weight for the progress bar.
		public HeavensTerrainPass() : base("Heavens Terrain", 1) { }

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
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
			progress.Set(0.25f);

			progress.Message = "Laying the golden bridges";
			var bridgeColumns = new List<(int X, int Y)>();
			foreach (var (from, to) in Bridges)
			{
				BuildBridge(from, to, bridgeColumns);
			}
			progress.Set(0.4f);

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
			progress.Set(0.6f);

			progress.Message = "Raising the statues";
			foreach (var (on, x) in Statues)
			{
				PlaceObject(on, x, Kind.Statue);
			}
			foreach (var (x, y) in bridgeColumns)
			{
				WorldGen.PlaceObject(x, y - 1, ModContent.TileType<GoldenBridgePost>(), true);
			}
			progress.Set(0.8f);

			progress.Message = "Planting the heavenly trees";
			int planted = 0;
			foreach (Island island in MainIslands)
			{
				// A tall tree, then a round one, and so on along the island, with a sapling or stump in each gap.
				int n = 0;
				for (int x = island.Left + 9; x <= island.Right - 9; x += 14, n++)
				{
					// The arena stays open in the middle; only its outer edges get scenery.
					if (island.Arena && x - island.Left > 30 && island.Right - x > 30)
					{
						continue;
					}
					bool tall = n % 2 == 0;
					if (!NearStatue(x, 9))
					{
						PlaceObject(island, x, tall ? Kind.TreeTall : Kind.TreeRound);
						planted++;
					}
					int smallX = x + 7;
					if (smallX <= island.Right - 6 && !NearStatue(smallX, 6))
					{
						PlaceObject(island, smallX, n % 3 == 0 ? Kind.Stump : Kind.Sapling);
					}
				}
			}
			Mod.Logger.Info($"Heavens trees planted: {planted}.");

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
					SetTile(x, top + k, k == 0 ? TileID.Grass : TileID.Cloud, glowingGold: k == 0);
				}
			}
		}

		// A golden walkway of platforms that sags a little in the middle. Remembers some columns for posts.
		private static void BuildBridge(Island from, Island to, List<(int X, int Y)> postSpots)
		{
			int x1 = from.Right - 2, x2 = to.Left + 2;
			int y1 = from.SurfaceY(x1), y2 = to.SurfaceY(x2);
			short frameY = (short)(HeavensSubworld.GoldenPlatformStyle * 18);
			var placed = new List<(int X, int Y)>();
			for (int x = x1; x <= x2; x++)
			{
				double t = (x - x1) / (double)(x2 - x1);
				int y = (int)Math.Round(y1 + (y2 - y1) * t + 2 * Math.Sin(Math.PI * t));
				if (Main.tile[x, y].HasTile)
				{
					continue; // still inside an island: the bridge starts where the ground ends
				}
				Tile tile = Main.tile[x, y];
				tile.HasTile = true;
				tile.TileType = TileID.Platforms;
				tile.TileFrameY = frameY;
				placed.Add((x, y));
			}

			// Ornate posts at both ends and about every 7 tiles between.
			for (int i = 0; i < placed.Count; i++)
			{
				if (i == 0 || i == placed.Count - 1 || i % 7 == 0)
				{
					postSpots.Add(placed[i]);
				}
			}
		}

		private enum Kind
		{
			Statue,
			TreeTall,
			TreeRound,
			Sapling,
			Stump,
		}

		private static (int Type, int Width, int Height) Describe(Kind what)
		{
			switch (what)
			{
				case Kind.TreeTall:
					return (ModContent.TileType<HeavenlyTreeTall>(), HeavenlyTreeTall.TilesWide, HeavenlyTreeTall.TilesHigh);
				case Kind.TreeRound:
					return (ModContent.TileType<HeavenlyTreeRound>(), HeavenlyTreeRound.TilesWide, HeavenlyTreeRound.TilesHigh);
				case Kind.Sapling:
					return (ModContent.TileType<HeavenlySapling>(), HeavenlySapling.TilesWide, HeavenlySapling.TilesHigh);
				case Kind.Stump:
					return (ModContent.TileType<HeavenlyStump>(), HeavenlyStump.TilesWide, HeavenlyStump.TilesHigh);
				default:
					return (ModContent.TileType<GoldenStatue>(), GoldenStatue.TilesWide, GoldenStatue.TilesHigh);
			}
		}

		// Levels a pad under the object, clears the air above it, then places it.
		private static void PlaceObject(Island island, int x, Kind what)
		{
			(int type, int w, int h) = Describe(what);
			int originX = w / 2; // an object's origin tile is its bottom-middle
			int ground = island.SurfaceY(x);
			for (int dx = 0; dx < w; dx++)
			{
				int tx = x - originX + dx;
				SetTile(tx, ground, TileID.Grass, glowingGold: true);
				for (int k = 1; k <= 3; k++)
				{
					SetTile(tx, ground + k, TileID.Cloud);
				}
				for (int k = 1; k <= h + 3; k++)
				{
					Tile air = Main.tile[tx, ground - k];
					air.HasTile = false;
				}
				WorldGen.SquareTileFrame(tx, ground, true);
			}
			WorldGen.PlaceObject(x, ground - 1, type, true);
		}

		// True if a statue stands within `margin` tiles (plus half the statue's width) of this column.
		private static bool NearStatue(int x, int margin)
		{
			foreach (var (_, sx) in Statues)
			{
				if (Math.Abs(sx - x) <= GoldenStatue.TilesWide / 2 + margin)
				{
					return true;
				}
			}
			return false;
		}

		private static void SetTile(int x, int y, int type, bool glowingGold = false)
		{
			Tile tile = Main.tile[x, y];
			tile.HasTile = true;
			tile.TileType = (ushort)type;
			if (glowingGold)
			{
				tile.TileColor = TurfPaint;
				tile.IsTileFullbright = true; // "illuminant": glows, so the turf looks luminous
			}
		}
	}
}
