#!/usr/bin/env python3
"""Makes the PLACEHOLDER feather sprite for the Mother of Terraria's Feather Storm (Projectiles/GoldenFeather.png, 22x70).
A long pointed feather in the colours of the owner's wings: gold at the base fading to blue at the tip, a dark indigo
outline, a pale quill down the middle and a little sparkle. It points UP (the projectile code turns it to face its travel).
Not from the owner's design (the design sheet has no loose feather): replace with real art later, same size or update
GoldenFeather.cs. (The first version, a flat gold leaf, was called ugly by the owner on 2026-10-24.)
Needs ImageMagick and numpy. Run from the project folder: python3 tools/make_feather.py"""
import subprocess

import numpy as np

W, H = 22, 70
OUTLINE = np.array([38, 22, 110])
GOLD = np.array([255, 205, 60])
GOLD_LIGHT = np.array([255, 240, 150])
BLUE = np.array([70, 140, 255])
BLUE_LIGHT = np.array([140, 205, 255])
QUILL = np.array([255, 250, 215])

img = np.zeros((H, W, 4), dtype=np.uint8)
cx = (W - 1) / 2.0
for y in range(H):
    t = y / (H - 1.0)                       # 0 at the tip (top), 1 at the base
    # Half-width: a pointed leaf, widest about 60% of the way down, then narrowing to the quill.
    half = 9.5 * np.sin(np.pi * min(1.0, t * 1.25)) ** 0.8 if t < 0.8 else 9.5 * np.sin(np.pi * 1.0 * 0.8 * 1.25) ** 0.8 * (1 - (t - 0.8) / 0.2) + 0.6
    half = max(half, 0.6)
    for x in range(W):
        d = abs(x - cx)
        if d <= half:
            edge = half - d
            # Like the wings: bands of blue at the tip, then gold (no muddy grey in between).
            if t < 0.22:
                body, light = BLUE, BLUE_LIGHT
            elif t < 0.5:
                body, light = np.array([95, 165, 255]), np.array([170, 220, 255])
            else:
                body, light = GOLD, GOLD_LIGHT
            colour = light if (x < cx and edge > 1.2) else body    # a lit left half
            if d < 0.8:
                colour = QUILL                                      # the quill
            if edge < 1.0 and half > 1.5:
                colour = OUTLINE                                    # the outline
            elif half <= 1.5:
                colour = np.array([224, 160, 32])                   # the bare quill at the base
            img[y, x, :3] = colour
            img[y, x, 3] = 255
# a few pale barbs cut diagonally across the vane
for y in range(14, 58, 5):
    for x in range(W):
        d = abs(x - cx)
        if 1.2 < d < 7 and img[y, x, 3] and tuple(img[y, x, :3]) != tuple(OUTLINE):
            yy = y + int((d) * 0.6)
            if yy < H and img[yy, x, 3] and tuple(img[yy, x, :3]) != tuple(OUTLINE):
                img[yy, x, :3] = (img[yy, x, :3] * 0.86).astype(np.uint8)
# a sparkle near the tip
img[16, int(cx) + 3, :] = (255, 255, 255, 255)
img[15, int(cx) + 3, :] = (255, 255, 255, 255)

p = subprocess.Popen(["magick", "-size", f"{W}x{H}", "-depth", "8", "rgba:-", "Projectiles/GoldenFeather.png"], stdin=subprocess.PIPE)
p.communicate(img.tobytes())
print("wrote Projectiles/GoldenFeather.png")
