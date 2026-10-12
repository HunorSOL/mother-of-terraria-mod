using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MotherOfTerraria.Tiles
{
	// Shared setup for the Heavens' multi-tile decorations (statues, trees, bridge posts and shimmer puddles).
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

		// Animated objects stack their frames vertically in the texture. 1 = not animated.
		protected virtual int AnimationFrames => 1;

		// How many ticks (60 per second) each animation frame lasts.
		protected virtual int TicksPerFrame => 6;

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
			if (AnimationFrames > 1)
			{
				// Each animation frame is a copy of the whole object, stacked down the texture.
				AnimationFrameHeight = ObjectHeight * 18;
			}

			AddMapEntry(new Color(236, 190, 56)); // name comes from the localization file
			DustType = DustID.GoldCoin;
		}

		// Plays the animation for objects with more than one frame (visuals only).
		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			if (AnimationFrames <= 1)
			{
				return;
			}
			frameCounter++;
			if (frameCounter >= TicksPerFrame)
			{
				frameCounter = 0;
				frame = (frame + 1) % AnimationFrames;
			}
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

	// A tall heavenly tree: golden trunk, pastel blue-and-gold cloud canopy (the owner's design, 7x12 tiles).
	public class HeavenlyTreeTall : HeavensObjectTile
	{
		public const int TilesWide = 7;
		public const int TilesHigh = 12;

		protected override int ObjectWidth => TilesWide;
		protected override int ObjectHeight => TilesHigh;
		protected override Vector3 Glow => new Vector3(0.22f, 0.24f, 0.3f); // a faint cool light under the canopy
	}

	// A shorter, rounder heavenly tree with hanging golden charms (the owner's design, 6x9 tiles).
	public class HeavenlyTreeRound : HeavensObjectTile
	{
		public const int TilesWide = 6;
		public const int TilesHigh = 9;

		protected override int ObjectWidth => TilesWide;
		protected override int ObjectHeight => TilesHigh;
		protected override Vector3 Glow => new Vector3(0.22f, 0.24f, 0.3f);
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

	// A small flat pool of shimmering light on the turf (2x1 tiles). Pure decoration: it is not solid and not a
	// liquid, so players and enemies simply walk over it and can never fall through it.
	public class ShimmerPuddleSmall : HeavensObjectTile
	{
		public const int TilesWide = 2;
		public const int TilesHigh = 1;

		protected override int ObjectWidth => TilesWide;
		protected override int ObjectHeight => TilesHigh;
		protected override int AnimationFrames => 8;

		// The glow drifts between pink, blue and lilac as it shimmers.
		protected override Vector3 Glow
		{
			get
			{
				float t = Main.GameUpdateCount * 0.04f;
				return new Vector3(0.38f + 0.12f * (float)System.Math.Sin(t), 0.3f + 0.1f * (float)System.Math.Sin(t + 2f), 0.5f + 0.12f * (float)System.Math.Sin(t + 4f));
			}
		}
	}

	// The same, 4 tiles wide.
	public class ShimmerPuddleLarge : HeavensObjectTile
	{
		public const int TilesWide = 4;
		public const int TilesHigh = 1;

		protected override int ObjectWidth => TilesWide;
		protected override int ObjectHeight => TilesHigh;
		protected override int AnimationFrames => 8;

		protected override Vector3 Glow
		{
			get
			{
				float t = Main.GameUpdateCount * 0.04f;
				return new Vector3(0.38f + 0.12f * (float)System.Math.Sin(t), 0.3f + 0.1f * (float)System.Math.Sin(t + 2f), 0.5f + 0.12f * (float)System.Math.Sin(t + 4f));
			}
		}
	}
}
