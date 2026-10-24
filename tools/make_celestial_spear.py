#!/usr/bin/env python3
"""Makes the Celestial Spear projectile sprite (Projectiles/CelestialSpear.png) from the owner's boss design sheet.

Input : DesignRefs/boss_spear.png  (cut out by tools/make_boss_sheet.py; the spear points to the UPPER RIGHT)
Output: Projectiles/CelestialSpear.png  (the spear pointing straight UP, which is how the projectile code expects it:
        it rotates the picture to face its direction of travel, like Projectiles/HeavenlySpear.png)
Turned 45 degrees, shrunk with a box filter to LENGTH px long, and reduced to a small palette (Terraria look).

Run from the project folder:  python3 tools/make_celestial_spear.py      (needs ImageMagick)
"""
import subprocess

LENGTH = 104
subprocess.check_call([
    "magick", "DesignRefs/boss_spear.png", "-background", "none", "-rotate", "-45", "-trim", "+repage",
    "-filter", "Box", "-resize", f"x{LENGTH}", "-channel", "A", "-threshold", "45%", "+channel",
    "-colors", "32", "+dither", "-trim", "+repage", "Projectiles/CelestialSpear.png"])
print(subprocess.check_output(["magick", "identify", "Projectiles/CelestialSpear.png"]).decode())
