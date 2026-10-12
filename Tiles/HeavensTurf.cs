using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Tiles
{
	// The Heavens' own ground: luminous golden turf that tops every island. A ModTile is a block type.
	// The heavenly trees (see Tiles/Trees/HeavenlyTree.cs) grow ON this tile, which is how the game knows to give
	// them their own look instead of a forest tree's.
	// Texture: Tiles/HeavensTurf.png, the standard tModLoader block sheet (made by tools/make_turf_sheet.py);
	// real art can replace it as long as it keeps the same layout (288x270).
	// Not an item: only world generation places it, and the Heavens' terrain protection stops it being broken.
	public class HeavensTurf : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true; // lets it give off light (see ModifyLight)

			AddMapEntry(new Color(240, 200, 80));
			DustType = DustID.GoldCoin;
		}

		// A warm golden glow, so the turf looks luminous.
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.5f;
			g = 0.4f;
			b = 0.12f;
		}
	}
}
