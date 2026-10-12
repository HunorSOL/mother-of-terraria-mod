using System.Collections.Generic;
using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Common
{
	// A GlobalItem sees every item in the game. This one stops items that teleport you home
	// (Magic Mirror, Recall Potion, etc.) from working inside the Heavens.
	// The Heavenly Teleporter is the only way out. Wormholes and Teleportation Potions stay
	// allowed because they only move you around inside the Heavens.
	public class HeavensEscapeBlocker : GlobalItem
	{
		private static readonly HashSet<int> EscapeItems = new HashSet<int>
		{
			ItemID.MagicMirror,
			ItemID.IceMirror,
			ItemID.RecallPotion,
			ItemID.PotionOfReturn,
			ItemID.CellPhone,
			ItemID.Shellphone,
			ItemID.ShellphoneSpawn,
			ItemID.ShellphoneOcean,
			ItemID.ShellphoneHell,
			ItemID.ShellphoneDummy,
			ItemID.MagicConch,
			ItemID.DemonConch,
		};

		// Don't print the message more than once per second while the mouse button is held.
		private static uint lastMessageTick;

		public override bool CanUseItem(Item item, Player player)
		{
			if (!EscapeItems.Contains(item.type) || !SubworldSystem.IsActive<HeavensSubworld>())
			{
				return true;
			}

			// Chat feedback is only for the player who tried (not gameplay logic).
			if (player.whoAmI == Main.myPlayer && Main.GameUpdateCount - lastMessageTick > 60)
			{
				lastMessageTick = Main.GameUpdateCount;
				Main.NewText("Only the Heavenly Teleporter can take you home.", 255, 215, 0);
			}
			return false;
		}
	}
}
