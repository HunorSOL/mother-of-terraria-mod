using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace MotherOfTerraria.Systems
{
	// Terraria makes trees, grass and other plants sway in the wind through one drawing function, GetWindCycle.
	// In the Heavens the owner wants everything to stand still, so this hooks that function and makes it return
	// "no sway" while the player is in the Heavens. Everywhere else the wind works as normal.
	// (A hook runs our code in place of, or around, a piece of the game's own code. It is removed again automatically
	// when the mod unloads.) Client-side only: it only affects how things are drawn.
	public class HeavensWindSystem : ModSystem
	{
		public override void Load()
		{
			if (Main.dedServ)
			{
				return; // a dedicated server draws nothing
			}
			On_TileDrawing.GetWindCycle += StillInTheHeavens;
		}

		private static float StillInTheHeavens(On_TileDrawing.orig_GetWindCycle orig, TileDrawing self, int x, int y, double windCounter)
		{
			return SubworldSystem.IsActive<HeavensSubworld>() ? 0f : orig(self, x, y, windCounter);
		}
	}
}
