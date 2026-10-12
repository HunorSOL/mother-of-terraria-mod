using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Items
{
	// The key item of the mod: the only way into (and out of) the Heavens.
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

		// Runs when the player uses the item: enters the Heavens, or returns home if already there.
		public override bool? UseItem(Player player)
		{
			// UseItem can run on several machines for the same use; only the user's own game acts on it.
			// In multiplayer, SubworldLibrary turns Enter/Exit into a request to the server,
			// which then moves just this player. In singleplayer it switches worlds directly.
			if (player.whoAmI == Main.myPlayer)
			{
				if (SubworldSystem.IsActive<HeavensSubworld>())
				{
					SubworldSystem.Exit();
				}
				else
				{
					SubworldSystem.Enter<HeavensSubworld>();
				}
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
