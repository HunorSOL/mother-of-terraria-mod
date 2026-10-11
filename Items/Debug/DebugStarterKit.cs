using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Items.Debug
{
	// TESTING ONLY - delete this item (and its texture) before release.
	// Using it gives you the Celestial Sigil, a Copper Shortsword, and a Heavenly Teleporter,
	// so you can test the crafting chain without beating Moon Lord.
	// Texture: Items/Debug/DebugStarterKit.png
	public class DebugStarterKit : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 1;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.rare = ItemRarityID.Expert;
			Item.UseSound = SoundID.Item4;
			// consumable stays false (the default): the kit is reusable.
		}

		// Craftable anywhere (no crafting station) from a single Dirt Block, so it is easy to get in a test world.
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.DirtBlock)
				.Register();
		}

		public override bool? UseItem(Player player)
		{
			// QuickSpawnItem drops the items on the player and handles multiplayer syncing for us.
			var source = player.GetSource_ItemUse(Item);
			player.QuickSpawnItem(source, ItemID.CelestialSigil);
			player.QuickSpawnItem(source, ItemID.CopperShortsword);
			player.QuickSpawnItem(source, ModContent.ItemType<HeavenlyTeleporter>());
			return true;
		}
	}
}
