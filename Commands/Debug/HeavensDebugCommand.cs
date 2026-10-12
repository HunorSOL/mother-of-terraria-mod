using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace MotherOfTerraria.Commands.Debug
{
	// TESTING ONLY. Remove before release.
	// Chat command for testing the Heavens before the Teleporter is hooked up (Phase 2.2).
	// Type "/heavens" in chat to go in, "/heavens exit" to come back. Singleplayer only.
	public class HeavensDebugCommand : ModCommand
	{
		public override CommandType Type => CommandType.World;
		public override string Command => "heavens";
		public override string Usage => "/heavens  or  /heavens exit";
		public override string Description => "DEBUG: enter or leave the Heavens subworld";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (Main.netMode != Terraria.ID.NetmodeID.SinglePlayer)
			{
				caller.Reply("/heavens is a singleplayer-only debug command.");
				return;
			}

			if (args.Length > 0 && args[0] == "exit")
			{
				if (!SubworldSystem.IsActive<HeavensSubworld>())
				{
					caller.Reply("You are not in the Heavens.");
					return;
				}
				SubworldSystem.Exit(); // back to the normal world
				return;
			}

			if (SubworldSystem.IsActive<HeavensSubworld>())
			{
				caller.Reply("You are already in the Heavens. Use /heavens exit to leave.");
				return;
			}
			SubworldSystem.Enter<HeavensSubworld>();
		}
	}
}
