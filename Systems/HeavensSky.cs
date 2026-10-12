using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;

namespace MotherOfTerraria.Systems
{
	// The Heavens' custom sky: a soft gradient from pale azure at the top to warm gold at the horizon,
	// painted behind everything else. It fades in and out smoothly when you enter or leave.
	// This is client-side drawing only (nothing in gameplay depends on it).
	public class HeavensSky : CustomSky
	{
		private static readonly Color TopColor = new Color(120, 175, 255);
		private static readonly Color HorizonColor = new Color(255, 228, 160);

		private bool active;
		private float intensity; // 0 = invisible, 1 = fully shown

		public override void Update(GameTime gameTime)
		{
			intensity = MathHelper.Clamp(intensity + (active ? 0.02f : -0.02f), 0f, 1f);
		}

		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
		{
			// Only paint on the very back layer, so everything else draws on top of the gradient.
			if (maxDepth < 3E+38f || minDepth >= 3E+38f)
			{
				return;
			}

			const int bands = 48;
			int bandHeight = Main.screenHeight / bands + 1;
			for (int i = 0; i < bands; i++)
			{
				Color color = Color.Lerp(TopColor, HorizonColor, i / (float)(bands - 1)) * intensity;
				spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, i * bandHeight, Main.screenWidth, bandHeight), color);
			}
		}

		// A warm golden cast over the world's lighting.
		public override Color OnTileColor(Color inColor)
		{
			Vector4 warm = new Vector4(1f, 0.93f, 0.75f, 1f);
			return new Color(Vector4.Lerp(inColor.ToVector4(), warm, intensity * 0.25f));
		}

		// Fewer vanilla clouds, since the islands are the scenery here.
		public override float GetCloudAlpha()
		{
			return 1f - intensity * 0.6f;
		}

		public override void Activate(Vector2 position, params object[] args) => active = true;
		public override void Deactivate(params object[] args) => active = false;
		public override void Reset() { active = false; intensity = 0f; }
		public override bool IsActive() => active || intensity > 0f;
	}
}
