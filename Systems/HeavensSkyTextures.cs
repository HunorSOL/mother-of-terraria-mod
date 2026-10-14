using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MotherOfTerraria.Systems
{
	// Soft round textures made in code (no art files), used by the Heavens sky for the sun, halo, light beams,
	// clouds and sparkles. In each pixel the colour AND the alpha both hold the brightness, so one texture works
	// for normal blending (clouds) and for additive glow (a tint with alpha 0 adds light instead of covering).
	// Client only; created on the main thread and freed when the mod unloads.
	public class HeavensSkyTextures : ModSystem
	{
		public static Texture2D Soft;  // a smooth glow, bright in the middle
		public static Texture2D Puff;  // a fluffy blob with a fuller middle, for clouds
		public static Texture2D Ring;  // a thin glowing ring, for the sun's halo

		public static bool Ready => Soft != null && Puff != null && Ring != null;

		public override void Load()
		{
			if (Main.netMode == NetmodeID.Server)
			{
				return;
			}
			Main.QueueMainThreadAction(() =>
			{
				Soft = Make(128, r => (float)Math.Pow(1f - r, 2));
				Puff = Make(128, r =>
				{
					float t = MathHelper.Clamp((1f - r) / 0.65f, 0f, 1f);
					return t * t * (3f - 2f * t);
				});
				Ring = Make(256, r => (float)Math.Exp(-Math.Pow((r - 0.8f) / 0.07f, 2)));
			});
		}

		public override void Unload()
		{
			Texture2D soft = Soft, puff = Puff, ring = Ring;
			Soft = Puff = Ring = null;
			if (Main.netMode != NetmodeID.Server)
			{
				Main.QueueMainThreadAction(() =>
				{
					soft?.Dispose();
					puff?.Dispose();
					ring?.Dispose();
				});
			}
		}

		// Builds a square texture whose brightness is f(distance from the centre, 0 at the middle to 1 at the edge).
		private static Texture2D Make(int size, Func<float, float> f)
		{
			var data = new Color[size * size];
			float half = (size - 1) / 2f;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float r = (float)Math.Sqrt((x - half) * (x - half) + (y - half) * (y - half)) / half;
					float v = r >= 1f ? 0f : MathHelper.Clamp(f(r), 0f, 1f);
					byte b = (byte)(v * 255f);
					data[y * size + x] = new Color(b, b, b, b);
				}
			}
			var texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
			texture.SetData(data);
			return texture;
		}
	}
}
