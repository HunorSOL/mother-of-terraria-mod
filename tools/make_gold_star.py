#!/usr/bin/env python3
"""Makes the Gold Crystal Star projectile sprite (Projectiles/GoldCrystalStar.png) from the owner's boss design sheet.

Input : DesignRefs/boss_star.png  (cut out by tools/make_boss_sheet.py; the comet trail streams to the LEFT)
Output: Projectiles/GoldCrystalStar.png  (turned a quarter turn so the trail streams UP, which is right for a falling star)
Shrunk with a box filter to HEIGHT px tall and reduced to a small palette (Terraria look).

Run from the project folder:  python3 tools/make_gold_star.py      (needs ImageMagick)
"""
import subprocess

HEIGHT = 72
subprocess.check_call([
    "magick", "DesignRefs/boss_star.png", "-background", "none", "-rotate", "90", "-trim", "+repage",
    "-filter", "Box", "-resize", f"x{HEIGHT}", "-channel", "A", "-threshold", "45%", "+channel",
    "-colors", "32", "+dither", "-trim", "+repage", "Projectiles/GoldCrystalStar.png"])
print(subprocess.check_output(["magick", "identify", "Projectiles/GoldCrystalStar.png"]).decode())
