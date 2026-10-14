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

		// Above every vanilla biome and environment (Space, Hell, Sandstorm) so the Heavens theme always plays,
		// but below the boss priorities so the boss fight can take over the music later.
		public override SceneEffectPriority Priority => SceneEffectPriority.Event;

		// No mountains or hills behind the islands: this is the sky.
		public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<HeavensBackgroundStyle>();

		// The Heavens theme ("Beneath the Spires", from the owner). Assets/Music/Heavens.ogg was made from
		// DesignRefs/music/Beneath_the_Spires.mp3 and turned down 3 dB (about -16 LUFS).
		// To change the volume, re-run the ffmpeg command noted in PROGRESS.md with a different dB value.
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/Heavens");

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
