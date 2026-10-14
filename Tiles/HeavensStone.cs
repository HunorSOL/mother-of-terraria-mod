using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Tiles
{
	// Pale marble-like stone with gold veins: the body of the Heavens' floating rocks.
	// Texture: Tiles/HeavensStone.png (standard block sheet, made by tools/make_stone_sheet.py; placeholder).
	// Protected from breaking inside the Heavens (see HeavensSubworld.IsProtectedTile).
	public class HeavensStone : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;

			AddMapEntry(new Color(190, 204, 232));
			DustType = DustID.Marble;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.3f;
			g = 0.34f;
			b = 0.45f;
		}
	}
}
