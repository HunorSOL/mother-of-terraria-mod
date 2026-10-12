using System.Collections.Generic;
using SubworldLibrary;
using MotherOfTerraria.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace MotherOfTerraria.Subworlds
{
	// The Heavens: a separate world (a "subworld") that lives alongside the player's normal world.
	// SubworldLibrary handles loading it, moving players in and out, and syncing it in multiplayer.
	// Phase 2.1: just a small empty world with a flat placeholder floor.
	public class HeavensSubworld : Subworld
	{
		// Size in tiles. Small on purpose: the Heavens is an arena area, not a full world.
		public override int Width => 800;
		public override int Height => 400;

		// Regenerate it fresh on every visit, so it always looks the same and can't be griefed permanently.
		public override bool ShouldSave => false;

		// Keep the player's inventory/progress changes made inside (e.g. loot picked up there).
		public override bool NoPlayerSaving => false;

		// Block types that make up the Heavens' own terrain. Players cannot break or replace these
		// (see Common/HeavensTerrainProtection). Add each new terrain block type here as terrain grows.
		// Only types players cannot normally place are used (no Dirt), so building with ordinary blocks stays free.
		private static readonly HashSet<int> protectedTiles = new HashSet<int>
		{
			TileID.Cloud,
			TileID.HallowedGrass,
			TileID.Trees,
			TileID.Saplings,
		};

		public static bool IsProtectedTile(int type)
		{
			// Mod tiles get their numeric ID at load time, so they are checked separately from the static set.
			return protectedTiles.Contains(type) || type == ModContent.TileType<GoldenStatue>();
		}

		// The list of world generation steps, run in order when the subworld is created.
		public override List<GenPass> Tasks => new List<GenPass>()
		{
			new HeavensTerrainPass(),
		};

		// Runs every time the subworld finishes loading. Lock it to bright daytime for now.
		public override void OnLoad()
		{
			Main.dayTime = true;
			Main.time = 27000; // midday
		}
	}
}
