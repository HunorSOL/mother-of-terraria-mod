using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Systems
{
	// A ModSceneEffect says "when the player is HERE, use this music and these visuals".
	// "Here" is the Heavens subworld. Client-side only: it affects what the local player sees and hears.
	public class HeavensSceneEffect : ModSceneEffect
	{
		public override bool IsSceneEffectActive(Player player)
		{
			return SubworldSystem.IsActive<HeavensSubworld>();
		}

		// Above normal biomes, below boss music (so the boss fight can take over later).
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		// No mountains or hills behind the islands: this is the sky.
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<HeavensBackgroundStyle>();

		// MUSIC SLOT (placeholder): vanilla Space music for now.
		// To use a real track later, put an .ogg/.wav/.mp3 in the mod and return
		// MusicLoader.GetMusicSlot(Mod, "Assets/Music/Heavens") here instead.
		public override int Music => MusicID.Space;

		// Turns the custom sky on while the player is in the Heavens, and off otherwise.
		// (Not Player.ManageSpecialBiomeVisuals: that also expects a screen filter registered under the
		// same name and crashes the game when there is none. We only have a sky, so drive it directly.)
		public override void SpecialVisuals(Player player, bool isActive)
		{
			CustomSky sky = SkyManager.Instance[HeavensSkySystem.SkyKey];
			if (sky == null || sky.IsActive() == isActive)
			{
				return;
			}

			if (isActive)
			{
				SkyManager.Instance.Activate(HeavensSkySystem.SkyKey);
			}
			else
			{
				SkyManager.Instance.Deactivate(HeavensSkySystem.SkyKey);
			}
		}
	}
}
