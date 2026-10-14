using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;

namespace MotherOfTerraria.Systems
{
	// The Heavens' custom sky: a soft gradient from pale azure at the top to warm gold at the horizon,
	// painted behind everything else, with a sun and halo, slow light beams, three layers of drifting clouds
	// (nearer layers move more as you move: parallax) and floating golden motes of light.
	// It fades in and out smoothly when you enter or leave.
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

			if (!HeavensSkyTextures.Ready)
			{
				return;
			}
			float time = Main.GlobalTimeWrappedHourly;
			Vector2 sun = new Vector2(Main.screenWidth * 0.74f, Main.screenHeight * 0.2f)
				+ new Vector2((Main.screenPosition.X - 5600f) * -0.01f, (Main.screenPosition.Y - 3600f) * -0.01f);

			DrawSun(spriteBatch, sun, time);
			DrawClouds(spriteBatch, time, 0);
			DrawBeams(spriteBatch, sun, time);
			DrawClouds(spriteBatch, time, 1);
			DrawClouds(spriteBatch, time, 2);
			DrawMotes(spriteBatch, time);
		}

		// A glowing additive light: the tint's alpha is 0, so it adds colour instead of covering the sky.
		private static Color Light(int r, int g, int b, float strength) => new Color(r, g, b, 0) * strength;

		private static void Glow(SpriteBatch sb, Texture2D texture, Vector2 center, Vector2 size, float rotation, Color color)
		{
			sb.Draw(texture, center, null, color, rotation, texture.Size() / 2f, size / texture.Size(), SpriteEffects.None, 0f);
		}

		private void DrawSun(SpriteBatch sb, Vector2 sun, float time)
		{
			float pulse = 1f + 0.05f * (float)Math.Sin(time * 0.8f);
			Glow(sb, HeavensSkyTextures.Soft, sun, new Vector2(1500f), 0f, Light(255, 226, 150, 0.45f * intensity));
			Glow(sb, HeavensSkyTextures.Soft, sun, new Vector2(620f * pulse), 0f, Light(255, 238, 190, 0.7f * intensity));
			Glow(sb, HeavensSkyTextures.Ring, sun, new Vector2(760f * pulse), time * 0.02f, Light(255, 240, 200, 0.28f * intensity));
			Glow(sb, HeavensSkyTextures.Ring, sun, new Vector2(1050f), -time * 0.015f, Light(255, 230, 170, 0.14f * intensity));
			Glow(sb, HeavensSkyTextures.Soft, sun, new Vector2(190f), 0f, Light(255, 252, 235, 1f * intensity));
		}

		// Long soft shafts of light fanning down and to the left from the sun, each slowly fading in and out.
		private void DrawBeams(SpriteBatch sb, Vector2 sun, float time)
		{
			const int count = 7;
			for (int i = 0; i < count; i++)
			{
				float angle = MathHelper.ToRadians(98f + i * 11f) + 0.05f * (float)Math.Sin(time * 0.15f + i);
				float length = 1900f + 200f * Hash(i, 1);
				float width = 110f + 120f * Hash(i, 2);
				float strength = 0.10f + 0.07f * (float)Math.Sin(time * (0.25f + 0.1f * Hash(i, 3)) + i * 2f);
				Vector2 direction = angle.ToRotationVector2();
				Glow(sb, HeavensSkyTextures.Soft, sun + direction * length / 2f, new Vector2(length, width), angle,
					Light(255, 236, 170, Math.Max(0f, strength) * intensity));
			}
		}

		// One layer of clouds. Layer 0 is far (slow, pale gold), 2 is near (faster, whiter). Each cloud is a few
		// overlapping squashed puffs. Positions repeat every `period` pixels, wider than any screen, so there is no popping.
		private void DrawClouds(SpriteBatch sb, float time, int layer)
		{
			const float period = 3400f;
			float[] parallax = { 0.04f, 0.10f, 0.20f };
			float[] drift = { 5f, 9f, 15f };
			float[] opacity = { 0.30f, 0.38f, 0.5f };
			Color[] tint = { new Color(255, 232, 180), new Color(255, 244, 214), new Color(255, 252, 240) };
			int[] count = { 6, 6, 5 };
			float[] size = { 0.8f, 1f, 1.3f };

			for (int i = 0; i < count[layer]; i++)
			{
				float baseX = i * period / count[layer] + Hash(i, 10 + layer) * 300f;
				float x = Mod(baseX + time * drift[layer] - Main.screenPosition.X * parallax[layer], period) - 500f;
				if (x > Main.screenWidth + 500f)
				{
					continue;
				}
				float y = Main.screenHeight * (0.3f + 0.17f * layer + 0.28f * Hash(i, 20 + layer))
					- (Main.screenPosition.Y - 3600f) * parallax[layer] * 0.4f;
				float scale = size[layer] * (0.8f + 0.5f * Hash(i, 30 + layer));

				for (int p = 0; p < 5; p++)
				{
					float px = x + (p - 2) * 120f * scale + Hash(i * 7 + p, 40) * 40f;
					float py = y - (float)Math.Sin(p / 4f * Math.PI) * 40f * scale + Hash(i * 7 + p, 50) * 20f;
					float w = (260f + 140f * Hash(i * 7 + p, 60)) * scale;
					Glow(sb, HeavensSkyTextures.Puff, new Vector2(px, py), new Vector2(w, w * 0.5f), 0f,
						tint[layer] * (opacity[layer] * intensity));
				}
			}
		}

		// Little golden lights drifting slowly upward and twinkling.
		private void DrawMotes(SpriteBatch sb, float time)
		{
			const int count = 70;
			for (int i = 0; i < count; i++)
			{
				float h3 = Hash(i, 3);
				float x = Mod(Hash(i, 1) * 3200f + time * (6f + 10f * h3) - Main.screenPosition.X * (0.15f + 0.3f * h3), 3200f) - 300f;
				float y = Mod(Hash(i, 2) * (Main.screenHeight + 400f) - time * (8f + 12f * h3) - Main.screenPosition.Y * 0.2f, Main.screenHeight + 400f) - 200f;
				float twinkle = 0.5f + 0.5f * (float)Math.Sin(time * (1.5f + 2f * h3) + Hash(i, 4) * 20f);
				float size = 14f + 22f * h3;
				Glow(sb, HeavensSkyTextures.Soft, new Vector2(x, y), new Vector2(size), 0f, Light(255, 224, 140, 0.55f * twinkle * intensity));
				Glow(sb, HeavensSkyTextures.Soft, new Vector2(x, y), new Vector2(size * 0.3f), 0f, Light(255, 250, 225, 0.9f * twinkle * intensity));
			}
		}

		// A repeatable pseudo-random number from 0 to 1 for (index, salt), so clouds and motes sit in the same places each frame.
		private static float Hash(int i, int salt)
		{
			uint n = (uint)(i * 374761393 + salt * 668265263);
			n = (n ^ (n >> 13)) * 1274126177u;
			return ((n ^ (n >> 16)) % 10000) / 10000f;
		}

		// A modulo that never returns a negative number.
		private static float Mod(float value, float period) => value - period * (float)Math.Floor(value / period);

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
