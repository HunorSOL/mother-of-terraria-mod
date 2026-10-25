using MotherOfTerraria.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Items.Debug
{
	// TESTING ONLY - delete this item (and its texture) before release.
	// A sword that does almost no damage but, when it hits the Mother of Terraria during stage 1, starts STAGE 2 at once: the Enchanted
	// Crystals are skipped (any that exist are removed), her health is set to the stage 2 threshold and she swaps to her stage 2 look and
	// attacks. Craft it from 1 Dirt + 1 Stone. (The Debug Stage Tester is the other one: it starts the crystal fight.)
	// Texture: Items/Debug/DebugStageTwoSkipper.png
	public class DebugStageTwoSkipper : ModItem
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
			Item.damage = 1;
			Item.DamageType = DamageClass.Melee;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.knockBack = 0f;
			Item.rare = ItemRarityID.Expert;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.DirtBlock)
				.AddIngredient(ItemID.StoneBlock)
				.Register();
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.ModNPC is MotherOfTerrariaBoss boss)
			{
				boss.DebugSkipToStageTwo();
			}
		}
	}
}
