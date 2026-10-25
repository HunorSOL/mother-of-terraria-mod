using MotherOfTerraria.NPCs.Bosses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Items.Debug
{
	// TESTING ONLY - delete this item (and its texture) before release.
	// A sword that does almost no damage but, when it hits the Mother of Terraria, sets her health to exactly the stage 2 threshold
	// (MotherOfTerrariaBoss.StageTwoAt), so the stage change (circle, Enchanted Crystals) starts right away without grinding half
	// her health away. Craft it from 1 Dirt + 1 Stone. It does nothing to her once the stage change has begun.
	// Texture: Items/Debug/DebugStageTester.png
	public class DebugStageTester : ModItem
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
			if (target.type != ModContent.NPCType<MotherOfTerrariaBoss>())
			{
				return;
			}
			int threshold = (int)System.Math.Ceiling(target.lifeMax * MotherOfTerrariaBoss.StageTwoAt);
			if (target.life > threshold)
			{
				target.life = threshold; // her own AI starts the stage change on its next tick
				target.netUpdate = true;
			}
		}
	}
}
