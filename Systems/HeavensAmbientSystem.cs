using Microsoft.Xna.Framework;
using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Systems
{
	// Drifting golden sparkles and pink petals that float across the screen in the Heavens.
	// Purely visual (dust particles on the local client; nothing in gameplay depends on them).
	public class HeavensAmbientSystem : ModSystem
	{
		public override void PostUpdateEverything()
		{
			if (Main.netMode == NetmodeID.Server || Main.gamePaused || !SubworldSystem.IsActive<HeavensSubworld>())
			{
				return;
			}

			// About one new particle every 4 ticks (15 a second); each lives a few seconds.
			if (Main.GameUpdateCount % 4 != 0)
			{
				return;
			}

			Vector2 position = Main.screenPosition + new Vector2(Main.rand.NextFloat(Main.screenWidth), Main.rand.NextFloat(Main.screenHeight));
			bool petal = Main.rand.NextBool(4);
			Dust dust = Dust.NewDustPerfect(
				position,
				petal ? DustID.PinkFairy : DustID.GoldFlame,
				new Vector2(Main.rand.NextFloat(-0.6f, -0.1f), Main.rand.NextFloat(0.1f, 0.5f)),
				150,
				default,
				petal ? 1.0f : Main.rand.NextFloat(0.6f, 1.0f));
			dust.noGravity = true;
			dust.fadeIn = 1.2f;
		}
	}
}
