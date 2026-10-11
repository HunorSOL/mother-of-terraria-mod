using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Items
{
	// The key item of the mod: it will open the portal to the Heavens (Phase 2).
	// For now it only has stats, a recipe, and a placeholder message when used.
	// Texture: Items/HeavenlyTeleporter.png (same name as the class, so it loads automatically).
	public class HeavenlyTeleporter : ModItem
	{
		public override void SetStaticDefaults()
		{
			// In Journey mode, only 1 is needed to duplicate this item.
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 64;
			Item.height = 64;
			Item.maxStack = 1;
			Item.useStyle = ItemUseStyleID.HoldUp; // raise the item overhead, like a Magic Mirror
			Item.useTime = 45;
			Item.useAnimation = 45;
			Item.rare = ItemRarityID.Purple; // above Moon Lord's rarity: this is endgame content
			Item.value = Item.sellPrice(gold: 10);
			Item.UseSound = SoundID.Item29;
		}

		// Runs when the player uses the item. Phase 2 will replace this with the real teleport.
		public override bool? UseItem(Player player)
		{
			// Only the player who used the item sees the message (local-only feedback, not gameplay logic).
			if (player.whoAmI == Main.myPlayer)
			{
				Main.NewText("The Heavens are not ready yet... (coming in Phase 2)", 255, 215, 0);
			}
			return true;
		}

		// Celestial Sigil + Copper Shortsword -> Heavenly Teleporter.
		// Terraria recipes accept ingredients with any prefix (e.g. "Legendary Copper Shortsword"),
		// so no extra code is needed for that rule.
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.CelestialSigil)
				.AddIngredient(ItemID.CopperShortsword)
				.AddTile(TileID.LunarCraftingStation) // Ancient Manipulator, the same station that makes the Sigil
				.Register();
		}
	}
}
