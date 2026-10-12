using MotherOfTerraria.Subworlds;
using SubworldLibrary;
using Terraria;
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

		// MUSIC SLOT (placeholder): vanilla Space music for now.
		// To use a real track later, put an .ogg/.wav/.mp3 in the mod and return
		// MusicLoader.GetMusicSlot(Mod, "Assets/Music/Heavens") here instead.
		public override int Music => MusicID.Space;

		// Turns the custom sky on while the player is in the Heavens, and off otherwise.
		public override void SpecialVisuals(Player player, bool isActive)
		{
			player.ManageSpecialBiomeVisuals(HeavensSkySystem.SkyKey, isActive);
		}
	}
}
