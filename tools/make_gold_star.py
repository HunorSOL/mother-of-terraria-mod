#!/usr/bin/env python3
"""Makes the Gold Crystal Star projectile sprite (Projectiles/GoldCrystalStar.png) from the owner's boss design sheet.

Input : DesignRefs/boss_star.png  (cut out by tools/make_boss_sheet.py; the star with a blue comet trail streaming left)
Output: Projectiles/GoldCrystalStar.png  (the STAR ONLY, no trail: the game adds the trail with sparks)
The trail is cut away by cropping to the star (the star spans about x 130-380, y 20-270 of the cut-out), then the
picture is shrunk with a box filter to SIZE px wide and kept at a generous palette so the gold shading and the blue
crystal in the middle survive. (The first version shrank the star with its trail to 51x72 px and 32 colours, which
looked muddy; the owner called it ugly, 2026-10-24.)

Run from the project folder:  python3 tools/make_gold_star.py      (needs ImageMagick)
"""
import subprocess

SIZE = 84
subprocess.check_call([
    "magick", "DesignRefs/boss_star.png", "-background", "none",
    "-crop", "250x252+130+20", "+repage",
    "-filter", "Box", "-resize", f"{SIZE}x",
    "-channel", "A", "-threshold", "50%", "+channel",
    "-colors", "64", "+dither", "-trim", "+repage", "Projectiles/GoldCrystalStar.png"])

# Wipe what is left of the blue comet trail behind the star: bluish pixels outside the crystal in the middle.
import numpy as np
OUT = "Projectiles/GoldCrystalStar.png"
w, h = map(int, subprocess.check_output(["magick", "identify", "-format", "%w %h", OUT]).split())
a = np.frombuffer(subprocess.check_output(["magick", OUT, "-depth", "8", "rgba:-"]), dtype=np.uint8).reshape(h, w, 4).copy()
yy, xx = np.mgrid[0:h, 0:w]
far = np.hypot(xx - w / 2.0, yy - h / 2.0) > 0.22 * w
bluish = a[..., 2].astype(int) > a[..., 0].astype(int) + 25
a[far & bluish, 3] = 0
p = subprocess.Popen(["magick", "-size", f"{w}x{h}", "-depth", "8", "rgba:-", "-trim", "+repage", OUT], stdin=subprocess.PIPE)
p.communicate(a.tobytes())
print(subprocess.check_output(["magick", "identify", OUT]).decode())
