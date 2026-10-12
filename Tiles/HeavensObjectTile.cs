using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MotherOfTerraria.Tiles
{
	// Shared setup for the Heavens' multi-tile decorations (statues, columns, ruins, bridge posts).
	// Each one is a ModTile (a block-like object in the world) with no item: world generation places them and
	// they are protected from breaking (see Subworlds/HeavensSubworld.IsProtectedTile).
	// A subclass only says how many tiles wide and tall it is. Its texture is the PNG with the same name as the
	// class in Tiles/ (16 px tiles with 2 px padding between them), so real art can simply replace the placeholder.
	public abstract class HeavensObjectTile : ModTile
	{
		protected abstract int ObjectWidth { get; }
		protected abstract int ObjectHeight { get; }

		// Bridge posts stand on golden platforms, so they accept a platform as their base too.
		protected virtual bool CanStandOnPlatform => false;

		// A soft golden glow around the object (red, green, blue strengths).
		protected virtual Vector3 Glow => new Vector3(0.55f, 0.42f, 0.08f);

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true; // multi-tile objects need this so their frames are kept
			Main.tileLavaDeath[Type] = false;

			TileObjectData.newTile.Width = ObjectWidth;
			TileObjectData.newTile.Height = ObjectHeight;
			int[] heights = new int[ObjectHeight];
			for (int i = 0; i < heights.Length; i++)
			{
				heights[i] = 16;
			}
			TileObjectData.newTile.CoordinateHeights = heights;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(ObjectWidth / 2, ObjectHeight - 1); // bottom-middle tile
			TileObjectData.newTile.UsesCustomCanPlace = true;
			AnchorType anchor = AnchorType.SolidTile | AnchorType.SolidWithTop;
			if (CanStandOnPlatform)
			{
				anchor |= AnchorType.Platform;
			}
			TileObjectData.newTile.AnchorBottom = new AnchorData(anchor, ObjectWidth, 0);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(236, 190, 56)); // name comes from the localization file
			DustType = DustID.GoldCoin;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = Glow.X;
			g = Glow.Y;
			b = Glow.Z;
		}
	}

	// A tall golden statue: a robed figure with a halo and spear (3x6 tiles).
	public class GoldenStatue : HeavensObjectTile
	{
		protected override int ObjectWidth => 3;
		protected override int ObjectHeight => 6;
	}

	// A tall intact golden marble column (2x6 tiles).
	public class GoldenColumn : HeavensObjectTile
	{
		protected override int ObjectWidth => 2;
		protected override int ObjectHeight => 6;
	}

	// A snapped column stump with rubble (2x3 tiles).
	public class BrokenColumn : HeavensObjectTile
	{
		protected override int ObjectWidth => 2;
		protected override int ObjectHeight => 3;
	}

	// Two columns and a broken stone lintel between them: a ruined temple gateway (7x6 tiles).
	public class RuinedArch : HeavensObjectTile
	{
		protected override int ObjectWidth => 7;
		protected override int ObjectHeight => 6;
	}

	// An ornate post with a glowing orb that stands along the golden bridges (1x3 tiles).
	public class GoldenBridgePost : HeavensObjectTile
	{
		protected override int ObjectWidth => 1;
		protected override int ObjectHeight => 3;
		protected override bool CanStandOnPlatform => true;
		protected override Vector3 Glow => new Vector3(0.35f, 0.55f, 0.8f); // the orb glows blue-white
	}
}
