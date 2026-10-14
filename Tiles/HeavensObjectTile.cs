using Microsoft.Xna.Framework;
using MotherOfTerraria.Common;
using MotherOfTerraria.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ObjectData;

namespace MotherOfTerraria.Tiles
{
	// Shared setup for the Heavens' multi-tile decorations (the statues, saplings, stumps and bridge posts).
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

		// Hanging objects (vines) hang from the underside of a solid block instead of standing on top of one.
		protected virtual bool Hanging => false;

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
			// The origin is the bottom-middle tile (top-middle for hanging objects).
			TileObjectData.newTile.Origin = new Point16(ObjectWidth / 2, Hanging ? 0 : ObjectHeight - 1);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			AnchorType anchor = AnchorType.SolidTile | AnchorType.SolidWithTop;
			if (CanStandOnPlatform)
			{
				anchor |= AnchorType.Platform;
			}
			if (Hanging)
			{
				TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, ObjectWidth, 0);
			}
			else
			{
				TileObjectData.newTile.AnchorBottom = new AnchorData(anchor, ObjectWidth, 0);
			}
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

	// A grand golden winged statue with a blue gem (the owner's design, 8x7 tiles).
	public class GoldenStatue : HeavensObjectTile
	{
		public const int TilesWide = 8;
		public const int TilesHigh = 7;

		protected override int ObjectWidth => TilesWide;
		protected override int ObjectHeight => TilesHigh;
	}

	// A heavenly sapling (the owner's design, 2x3 tiles).
	public class HeavenlySapling : HeavensObjectTile
	{
		public const int TilesWide = 2;
		public const int TilesHigh = 3;

		protected override int ObjectWidth => TilesWide;
		protected override int ObjectHeight => TilesHigh;
		protected override Vector3 Glow => new Vector3(0.15f, 0.17f, 0.22f);
	}

	// A heavenly tree stump (the owner's design, 3x2 tiles).
	public class HeavenlyStump : HeavensObjectTile
	{
		public const int TilesWide = 3;
		public const int TilesHigh = 2;

		protected override int ObjectWidth => TilesWide;
		protected override int ObjectHeight => TilesHigh;
		protected override Vector3 Glow => new Vector3(0.15f, 0.17f, 0.22f);
	}

	// An ornate post with a glowing orb that stands along the golden bridges (1x3 tiles).
	public class GoldenBridgePost : HeavensObjectTile
	{
		protected override int ObjectWidth => 1;
		protected override int ObjectHeight => 3;
		protected override bool CanStandOnPlatform => true;
		protected override Vector3 Glow => new Vector3(0.35f, 0.55f, 0.8f); // the orb glows blue-white
	}

	// Small glowing flowers that grow on the island tops (placeholder art).
	public class HeavenlyFlowerBlue : HeavensObjectTile
	{
		protected override int ObjectWidth => 1;
		protected override int ObjectHeight => 1;
		protected override Vector3 Glow => new Vector3(0.12f, 0.2f, 0.32f);
	}

	public class HeavenlyFlowerGold : HeavensObjectTile
	{
		protected override int ObjectWidth => 1;
		protected override int ObjectHeight => 2;
		protected override Vector3 Glow => new Vector3(0.35f, 0.28f, 0.08f);
	}

	public class HeavenlyFlowerPink : HeavensObjectTile
	{
		protected override int ObjectWidth => 1;
		protected override int ObjectHeight => 1;
		protected override Vector3 Glow => new Vector3(0.3f, 0.15f, 0.22f);
	}

	// Vines that hang from the undersides of the islands, ending in a glowing bud (placeholder art).
	public class HeavenlyVineShort : HeavensObjectTile
	{
		protected override int ObjectWidth => 1;
		protected override int ObjectHeight => 2;
		protected override bool Hanging => true;
		protected override Vector3 Glow => new Vector3(0.1f, 0.2f, 0.3f);
	}

	public class HeavenlyVineLong : HeavensObjectTile
	{
		protected override int ObjectWidth => 1;
		protected override int ObjectHeight => 4;
		protected override bool Hanging => true;
		protected override Vector3 Glow => new Vector3(0.1f, 0.2f, 0.3f);
	}

	// The Heavenly Basket (4x3 tiles, PLACEHOLDER art): right-click it to put in Heavens Crystals.
	// When it holds 50, the summon fight starts (see Common/HeavenlyBasketPlayer). It glows brighter as it fills.
	public class HeavenlyBasket : HeavensObjectTile
	{
		public const int TilesWide = 4;
		public const int TilesHigh = 3;

		protected override int ObjectWidth => TilesWide;
		protected override int ObjectHeight => TilesHigh;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			TileID.Sets.HasOutlines[Type] = true; // shows the interaction outline when the mouse is over it
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

		public override bool RightClick(int i, int j)
		{
			Main.LocalPlayer.GetModPlayer<HeavenlyBasketPlayer>().TryDeposit();
			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<HeavensCrystal>();
			player.cursorItemIconText = $"{player.GetModPlayer<HeavenlyBasketPlayer>().Crystals}/{HeavenlyBasketPlayer.CrystalsNeeded}";
		}

		// Dim when empty, bright blue-gold when full.
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			float fill = Main.LocalPlayer.GetModPlayer<HeavenlyBasketPlayer>().Crystals / (float)HeavenlyBasketPlayer.CrystalsNeeded;
			r = 0.15f + 0.6f * fill;
			g = 0.2f + 0.6f * fill;
			b = 0.3f + 0.6f * fill;
		}
	}
}
