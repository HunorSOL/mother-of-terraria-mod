"""Generates the PLACEHOLDER animated sprite sheets for the Heavens' shimmer puddles (no owner design yet).

Outputs (each is an ANIMATED 1-tile-high object: 8 frames stacked vertically, 18 px per frame, 16 px tiles with 2 px padding):
  Tiles/ShimmerPuddleSmall.png   2 tiles wide
  Tiles/ShimmerPuddleLarge.png   4 tiles wide
A flat iridescent pool whose colours drift around the rainbow in pastel shades, with a few twinkling sparkles.
It is only decoration: the tile is not solid and not a liquid, so nothing can fall through it.
Replace a PNG with the owner's art of the same size (8 frames) to change the look.

Run from the project folder:  python3 tools/make_shimmer_puddles.py      (needs ImageMagick and numpy)
"""
import colorsys
import math
import subprocess

import numpy as np

FRAMES = 8
FRAME_H = 18  # 16 px of tile + 2 px padding


def frame(tiles_w, f):
    w = tiles_w * 16
    img = np.zeros((16, w, 4), dtype=np.uint8)
    cx, cy = (w - 1) / 2.0, 11.5
    rx, ry = w / 2.0 - 1.5, 3.6
    for y in range(16):
        for x in range(w):
            d = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2
            if d > 1.0:
                continue
            # drifting pastel rainbow: the hue moves along the puddle and around with the frame number
            hue = (x * 0.012 + y * 0.03 + f / FRAMES) % 1.0
            sat = 0.42 + 0.12 * math.sin(x * 0.4 + f * 0.8)
            val = 0.92
            r, g, b = colorsys.hsv_to_rgb(hue, sat, val)
            if d > 0.78:                       # soft darker rim
                r, g, b = r * 0.72, g * 0.72, b * 0.8
            elif y < cy - 1.2:                 # lighter highlight along the far edge
                r, g, b = min(1, r * 1.1 + 0.08), min(1, g * 1.1 + 0.08), min(1, b * 1.1 + 0.1)
            img[y, x] = (int(r * 255), int(g * 255), int(b * 255), 255)
    # twinkling sparkles
    for k in range(max(2, tiles_w)):
        sx = int((k * 37 + f * 11) % max(1, w - 6)) + 3
        sy = 10 + (k + f) % 3
        if img[sy, sx, 3]:
            img[sy, sx] = (255, 255, 255, 255)
            if f % 2 == 0 and img[sy, sx + 1, 3]:
                img[sy, sx + 1] = (255, 255, 240, 255)
    return img


def build(tiles_w, path):
    w = tiles_w * 16
    W = tiles_w * 18 - 2
    sheet = np.zeros((FRAMES * FRAME_H, W, 4), dtype=np.uint8)
    for f in range(FRAMES):
        art = frame(tiles_w, f)
        for tx in range(tiles_w):
            sheet[f * FRAME_H:f * FRAME_H + 16, tx * 18:tx * 18 + 16] = art[:, tx * 16:tx * 16 + 16]
    proc = subprocess.Popen(["magick", "-size", f"{W}x{FRAMES * FRAME_H}", "-depth", "8", "rgba:-", path], stdin=subprocess.PIPE)
    proc.communicate(sheet.tobytes())
    print(f"wrote {path}: {W}x{FRAMES * FRAME_H} ({tiles_w} tiles wide, {FRAMES} frames)")


if __name__ == "__main__":
    build(2, "Tiles/ShimmerPuddleSmall.png")
    build(4, "Tiles/ShimmerPuddleLarge.png")
