using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Systems
{
	// A ModSystem is a place for mod-wide code that isn't tied to one item or tile.
	// This one registers the Heavens sky with the game when the mod loads (never on a server, which has no screen).
	public class HeavensSkySystem : ModSystem
	{
		public const string SkyKey = "MotherOfTerraria:Heavens";

		public override void OnModLoad()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				SkyManager.Instance[SkyKey] = new HeavensSky();
			}
		}
	}
}
