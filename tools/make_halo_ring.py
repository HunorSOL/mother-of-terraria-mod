#!/usr/bin/env python3
"""Makes the strip texture for the Mother of Terraria's Halo Rings (Projectiles/HaloRing.png) from the owner's design.

Input : DesignRefs/Gemini_Generated_Image_sm4f30sm4f30sm4f.jpg  (2752x1536 sheet, 2026-10-24: the "Expanding Ring projectile",
        a ring burst and a magic spark)
Output: Projectiles/HaloRing.png  (a short piece of the ring's BAND, THICKNESS px across and LENGTH px long. The band runs
        along the picture's x axis: BOTTOM edge = the ring's outer edge. HaloRing.cs draws it many times along the ring, so the
        ring can be any size and have a gap, which one stretched picture could not do.)
The band is the cross-section of the big ring (dark outline, gold, blue, gold, outline) taken along a horizontal line through
its left edge, then repeated along the length. The spikes and the glow of the design are left out (the game draws caps and light).
Run from the project folder:  python3 tools/make_halo_ring.py      (needs ImageMagick and numpy)
"""
import subprocess

import numpy as np

SRC = "DesignRefs/Gemini_Generated_Image_sm4f30sm4f30sm4f.jpg"
OUT = "Projectiles/HaloRing.png"
ROW_Y = 720                 # a line through the ring's left edge in the original picture
BAND_X0, BAND_W = 275, 185  # where the band starts and how wide it is (outer outline to inner outline)
THICKNESS = 28              # final pixels across the band
LENGTH = 16                 # final pixels along the band
COLOURS = 24


def main():
    # take 6 neighbouring rows so a single noisy row does not matter, then average them
    raw = subprocess.run(
        ["magick", SRC, "-crop", f"{BAND_W}x6+{BAND_X0}+{ROW_Y - 3}", "+repage", "-depth", "8", "rgb:-"],
        check=True, capture_output=True).stdout
    rows = np.frombuffer(raw, dtype=np.uint8).reshape(6, BAND_W, 3).astype(np.float32).mean(axis=0)
    # shrink the band to THICKNESS pixels with a box filter
    edges = np.linspace(0, BAND_W, THICKNESS + 1)
    prof = np.zeros((THICKNESS, 3), dtype=np.float32)
    for i in range(THICKNESS):
        a, b = edges[i], edges[i + 1]
        prof[i] = rows[int(a):max(int(np.ceil(b)), int(a) + 1)].mean(axis=0)
    prof = prof[::-1]   # the picture's bottom row is the ring's outer edge (see HaloRing.PreDraw)
    # picture: THICKNESS rows (outer edge at the bottom) by LENGTH columns, each row one colour of the profile
    img = np.repeat(prof[:, None, :], LENGTH, axis=1).astype(np.uint8)
    ppm = b"P6\n%d %d\n255\n" % (LENGTH, THICKNESS) + img.tobytes()
    subprocess.run(["magick", "ppm:-", "-colors", str(COLOURS), "-define", "png:color-type=6", OUT],
                   input=ppm, check=True)
    print("wrote", OUT, f"{LENGTH}x{THICKNESS}")


if __name__ == "__main__":
    main()
