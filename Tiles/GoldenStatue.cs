using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MotherOfTerraria.Tiles
{
	// A tall golden statue (3 tiles wide, 6 tall) that decorates the Heavens.
	// A ModTile is a block-like object in the world. It has no item: it is only placed by world generation
	// and is protected from breaking (see Subworlds/HeavensSubworld.IsProtectedTile).
	// Texture: Tiles/GoldenStatue.png (52x106: 3x6 tiles of 16px with 2px padding between them).
	public class GoldenStatue : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true; // multi-tile objects need this so their frames are kept
			Main.tileLavaDeath[Type] = false;

			// Describe the object's shape: 3x6 tiles, anchored by its bottom row to solid ground.
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Height = 6;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(1, 5); // bottom-middle tile
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 3, 0);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(236, 190, 56)); // gold dot on the map; name comes from the localization file
			DustType = DustID.GoldCoin;
		}

		// A soft golden glow around the statue.
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.55f;
			g = 0.42f;
			b = 0.08f;
		}
	}
}
