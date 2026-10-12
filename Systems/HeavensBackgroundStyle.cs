using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace MotherOfTerraria.Systems
{
	// The "surface background" is the scenery layers behind the world: mountains, hills, forests.
	// Terraria picks them by biome. The Heavens has no ground to see, so this style replaces all of them
	// with a fully transparent image: only the sky (HeavensSky) and the clouds show through.
	// Texture: Backgrounds/HeavensEmpty.png. Phase 2C can swap in real scenery (e.g. distant floating islands).
	public class HeavensBackgroundStyle : ModSurfaceBackgroundStyle
	{
		private int EmptySlot => BackgroundTextureLoader.GetBackgroundSlot(Mod, "Backgrounds/HeavensEmpty");

		// Fade this style in and every vanilla one out.
		public override void ModifyFarFades(float[] fades, float transitionSpeed)
		{
			for (int i = 0; i < fades.Length; i++)
			{
				if (i == Slot)
				{
					fades[i] += transitionSpeed;
					if (fades[i] > 1f)
					{
						fades[i] = 1f;
					}
				}
				else
				{
					fades[i] -= transitionSpeed;
					if (fades[i] < 0f)
					{
						fades[i] = 0f;
					}
				}
			}
		}

		public override int ChooseFarTexture() => EmptySlot;

		public override int ChooseMiddleTexture() => EmptySlot;

		public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => EmptySlot;

		// Skip drawing the closest layer altogether.
		public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) => false;
	}
}
