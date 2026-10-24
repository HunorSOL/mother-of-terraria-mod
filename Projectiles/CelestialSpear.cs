namespace MotherOfTerraria.Projectiles
{
	// The Mother of Terraria's own spear (the owner's "Celestial Spear" design). It flies and hits exactly like the
	// Heavenly Spear, using the boss's stronger homing (HeavenlySpear's boss constants), but is longer and has her art.
	// Texture: Projectiles/CelestialSpear.png (42x104, pointing UP; made by tools/make_celestial_spear.py).
	public class CelestialSpear : HeavenlySpear
	{
		protected override bool FromBoss => true;
		protected override float HalfLength => 46f;
	}
}
