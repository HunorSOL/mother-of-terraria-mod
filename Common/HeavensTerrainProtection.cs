using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria.ModLoader;

namespace MotherOfTerraria.Common
{
	// A GlobalTile sees every tile in the game. This one stops the Heavens' own terrain
	// (the blocks listed in HeavensSubworld.ProtectedTiles) from being broken, blown up or replaced.
	// Players can still place and remove their own blocks of other types.
	// These hooks run on both the client and the server, so the rule holds in multiplayer too.
	public class HeavensTerrainProtection : GlobalTile
	{
		private static bool IsProtected(int type)
		{
			return SubworldSystem.IsActive<HeavensSubworld>() && HeavensSubworld.ProtectedTiles.Contains(type);
		}

		// Pickaxes, hammers, drills.
		public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
		{
			return !IsProtected(type);
		}

		// Bombs, dynamite, explosive projectiles.
		public override bool CanExplode(int i, int j, int type)
		{
			return !IsProtected(type);
		}

		// Placing a different block on top of an existing one (e.g. paint-style replacement).
		public override bool CanReplace(int i, int j, int type, int tileTypeBeingPlaced)
		{
			return !IsProtected(type);
		}
	}
}
