using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MotherOfTerraria.Common
{
	// A ModPlayer runs code for every player each tick. This one kills a player who falls off the
	// floating islands and reaches the cave background far below (the world's "underground" line).
	public class HeavensFallDeath : ModPlayer
	{
		public override void PostUpdate()
		{
			// Each client kills its own player; Terraria then tells everyone else.
			if (Player.whoAmI != Main.myPlayer || Player.dead || !SubworldSystem.IsActive<HeavensSubworld>())
			{
				return;
			}

			// Main.worldSurface is set by the Heavens terrain pass (row 330); below it the cave background shows.
			if (Player.Center.Y / 16f > Main.worldSurface)
			{
				Player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(Player.name + " fell from the Heavens.")), 9999, 0);
			}
		}
	}
}
