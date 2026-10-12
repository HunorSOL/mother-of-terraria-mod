using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace MotherOfTerraria.Subworlds
{
	// One world generation step for the Heavens: lays down a flat floor of cloud blocks
	// and puts the spawn point on top of it. Phase 2.4 will replace this with real terrain.
	public class HeavensFloorPass : GenPass
	{
		// Row (counted from the top of the world) where the floor's top surface sits.
		public const int FloorTop = 200;
		public const int FloorThickness = 10;

		// "Heavens Floor" is the step name shown in logs; the number is its rough weight for the progress bar.
		public HeavensFloorPass() : base("Heavens Floor", 1) { }

		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Shaping the Heavens";

			// Tell the game where "surface" and "underground" are, so the sky and lighting render as open sky.
			Main.worldSurface = FloorTop + FloorThickness + 20;
			Main.rockLayer = Main.worldSurface + 20;

			for (int x = 0; x < Main.maxTilesX; x++)
			{
				for (int y = FloorTop; y < FloorTop + FloorThickness; y++)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = TileID.Cloud; // placeholder material
				}
				progress.Set((float)x / Main.maxTilesX);
			}

			// Spawn in the middle, standing on the floor.
			Main.spawnTileX = Main.maxTilesX / 2;
			Main.spawnTileY = FloorTop - 1;
		}
	}
}
