namespace MotherOfTerraria.Projectiles
{
	// The Mother of Terraria's STAGE 2 spear (the owner's "Starlight Lance" design, 2026-10-25). It flies, homes and hits exactly like the
	// Celestial Spear (it is the stage 2 version of the same attack and hits much harder, see MotherOfTerrariaBoss.StarlightDamage).
	// Texture: Projectiles/StarlightLance.png (49x104, pointing UP; made by tools/make_boss_stage2_sheet.py).
	public class StarlightLance : HeavenlySpear
	{
		protected override bool FromBoss => true;
		protected override float HalfLength => 46f;
	}
}
