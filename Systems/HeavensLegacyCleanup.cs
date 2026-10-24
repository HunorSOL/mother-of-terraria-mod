using Microsoft.Xna.Framework;
using SubworldLibrary;
using MotherOfTerraria.Subworlds;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MotherOfTerraria.Systems
{
	// The Heavens is saved with the world now, so a change to the terrain code does not reach a Heavens that was already saved.
	// This removes, ONCE per saved Heavens, the small floating island that used to hang above the Heavenly Basket
	// (removed from the terrain code at the owner's request, 2026-10-24). A brand-new Heavens never has it, and the
	// clean-up there changes nothing. The "done" flag is saved with the Heavens so it never runs twice
	// (so anything the player builds there later is safe).
	public class HeavensLegacyCleanup : ModSystem
	{
		// Tile rectangle that held the island, its trees, flowers and vines (the Basket's island is far below it).
		private static readonly Rectangle OldFloater = new Rectangle(300, 105, 61, 71);

		private bool floaterRemoved;

		public override void OnWorldLoad()
		{
			floaterRemoved = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (floaterRemoved)
			{
				tag["floaterRemoved"] = true;
			}
		}

		public override void LoadWorldData(TagCompound tag)
		{
			floaterRemoved = tag.GetBool("floaterRemoved");
		}

		// Runs a moment after the world has finished loading (so LoadWorldData has already run).
		public override void PostUpdateEverything()
		{
			if (floaterRemoved || Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient || !SubworldSystem.IsActive<HeavensSubworld>())
			{
				return;
			}
			floaterRemoved = true;
			for (int i = OldFloater.Left; i < OldFloater.Right; i++)
			{
				for (int j = OldFloater.Top; j < OldFloater.Bottom; j++)
				{
					if (WorldGen.InWorld(i, j, 2))
					{
						Main.tile[i, j].ClearTile();
					}
				}
			}
		}
	}
}
