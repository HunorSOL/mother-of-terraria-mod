using MotherOfTerraria.NPCs.Bosses;
using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Items.Debug
{
	// TESTING ONLY - delete this item (and its texture) before release.
	// Summons the Mother of Terraria immediately (inside the Heavens only), skipping the crystals and the 30 s
	// sequence. Killing a boss summoned this way does not touch the Heavenly Basket (no fight is active).
	// Texture: Items/Debug/DebugBossSummoner.png
	public class DebugBossSummoner : ModItem
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
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.DirtBlock)
				.AddIngredient(ItemID.StoneBlock)
				.Register();
		}

		public override bool CanUseItem(Player player)
		{
			return SubworldSystem.IsActive<HeavensSubworld>() && !NPC.AnyNPCs(ModContent.NPCType<MotherOfTerrariaBoss>());
		}

		public override bool? UseItem(Player player)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X, (int)player.Center.Y - 300, ModContent.NPCType<MotherOfTerrariaBoss>());
			}
			return true;
		}
	}
}
