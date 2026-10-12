using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Items
{
	// PLACEHOLDER crystal dropped by the Heavens enemies. Later it will fill the Heavenly Basket to summon
	// the boss (idea only, specifics to be decided). For now it is just a collectible material.
	// Texture: Items/HeavensCrystal.png (same name as the class, so a real design can simply replace it).
	public class HeavensCrystal : ModItem
	{
		public override void SetStaticDefaults()
		{
			// In Journey mode, 25 are needed to duplicate this item (a normal amount for a material).
			Item.ResearchUnlockCount = 25;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 28;
			Item.maxStack = Item.CommonMaxStack; // stacks to 9999 like vanilla materials
			Item.rare = ItemRarityID.Purple;
			Item.value = Item.sellPrice(gold: 1);
		}
	}
}
