using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Tiles.Trees
{
	// The Heavenly Tree: a real, tile-based Terraria tree (a trunk of tiles, branches, and a crown on top that
	// sways in the wind), but with the owner's golden bark and pastel cloud foliage instead of forest art.
	// A ModTree grows from the tile listed in GrowsOnTileId, here the golden Heavens turf, so it never appears
	// anywhere else. World generation grows them with the game's own tree growth routine (see HeavensTerrainPass).
	// Textures (all made by tools/make_tree_textures.py from the owner's design):
	//   Tiles/Trees/HeavenlyTree.png            the trunk pieces
	//   Tiles/Trees/HeavenlyTree_Branches.png   the side branches with foliage
	//   Tiles/Trees/HeavenlyTree_Tops.png       three crown variants (the owner's canopies)
	public class HeavenlyTree : ModTree
	{
		// The crown frames are bigger than the game's default 80x80, so we tell it their size.
		// Must match CROWN_W / CROWN_H in tools/make_tree_textures.py.
		private const int CrownWidth = 104;
		private const int CrownHeight = 84;

		private Asset<Texture2D> trunkTexture;
		private Asset<Texture2D> branchTexture;
		private Asset<Texture2D> topTexture;

		// How the tree reacts to paint. These are the same values tModLoader's own example tree uses.
		public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings
		{
			UseSpecialGroups = true,
			SpecialGroupMinimalHueValue = 11f / 72f,
			SpecialGroupMaximumHueValue = 0.25f,
			SpecialGroupMinimumSaturationValue = 0.88f,
			SpecialGroupMaximumSaturationValue = 1f
		};

		public override void SetStaticDefaults()
		{
			GrowsOnTileId = new[] { ModContent.TileType<HeavensTurf>() };
			trunkTexture = ModContent.Request<Texture2D>("MotherOfTerraria/Tiles/Trees/HeavenlyTree");
			branchTexture = ModContent.Request<Texture2D>("MotherOfTerraria/Tiles/Trees/HeavenlyTree_Branches");
			topTexture = ModContent.Request<Texture2D>("MotherOfTerraria/Tiles/Trees/HeavenlyTree_Tops");
		}

		public override Asset<Texture2D> GetTexture() => trunkTexture;

		public override Asset<Texture2D> GetBranchTextures() => branchTexture;

		public override Asset<Texture2D> GetTopTextures() => topTexture;

		public override void SetTreeFoliageSettings(int i, int j, Tile tile, int xoffset, ref int treeFrame, int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
		{
			topTextureFrameWidth = CrownWidth;
			topTextureFrameHeight = CrownHeight;
		}

		// Required by the game. These trees cannot be chopped in the Heavens (terrain protection), so this only
		// matters if one were ever felled elsewhere.
		public override int DropWood() => ItemID.Wood;
	}
}
