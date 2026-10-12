"""Generates the PLACEHOLDER animated sprite sheets for the Heavens' shimmer puddles (no owner design yet).

Outputs (each is an ANIMATED 1-tile-high ground piece: 8 frames stacked vertically, 18 px per frame, 16 px tiles with 2 px padding):
  Tiles/ShimmerPuddleSmall.png   2 tiles wide
  Tiles/ShimmerPuddleLarge.png   4 tiles wide
Each piece REPLACES a stretch of the turf surface, so the pool is set into the ground: an iridescent pool whose pastel
colours drift around the rainbow, level with the grass, with a pale cloud-coloured body below it so it blends into
the island. Players walk on top of it like the rest of the ground (the tile is a solid-top, not a liquid).
Replace a PNG with the owner's art of the same size (8 frames) to change the look.

Run from the project folder:  python3 tools/make_shimmer_puddles.py      (needs ImageMagick and numpy)
"""
import colorsys
import math
import subprocess

import numpy as np

FRAMES = 8
FRAME_H = 18  # 16 px of tile + 2 px padding


BODY_TOP = (238, 246, 255)
BODY_BOT = (204, 222, 247)
SHADOW = (166, 188, 222)
RIM = (234, 202, 96)


def frame(tiles_w, f):
    w = tiles_w * 16
    img = np.zeros((16, w, 4), dtype=np.uint8)
    cx, cy = (w - 1) / 2.0, 3.0
    rx, ry = w / 2.0 - 1.0, 5.0
    for y in range(16):
        for x in range(w):
            d = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2
            if d <= 1.0:
                # the pool: pastel rainbow that drifts along the surface and around with the frame number
                hue = (x * 0.012 + y * 0.03 + f / FRAMES) % 1.0
                sat = 0.42 + 0.12 * math.sin(x * 0.4 + f * 0.8)
                r, g, b = colorsys.hsv_to_rgb(hue, sat, 0.93)
                if d > 0.8:                       # darker edge of the pool, so it looks set into the ground
                    r, g, b = r * 0.74, g * 0.74, b * 0.82
                elif y <= 1:                      # bright surface line
                    r, g, b = min(1, r * 1.08 + 0.1), min(1, g * 1.08 + 0.1), min(1, b * 1.08 + 0.12)
                col = (int(r * 255), int(g * 255), int(b * 255))
            elif d <= 1.35:
                col = SHADOW                      # shadowed lip around the pool
            elif y == 0:
                col = RIM                         # the golden turf line either side of the pool
            else:
                t = y / 15.0                      # pale cloud-coloured ground
                col = tuple(int(BODY_TOP[i] * (1 - t) + BODY_BOT[i] * t) for i in range(3))
            img[y, x] = (col[0], col[1], col[2], 255)
    # twinkling sparkles on the pool
    for k in range(max(2, tiles_w)):
        sx = int((k * 37 + f * 11) % max(1, w - 8)) + 4
        sy = 1 + (k + f) % 4
        if ((sx - cx) / rx) ** 2 + ((sy - cy) / ry) ** 2 < 0.7:
            img[sy, sx] = (255, 255, 255, 255)
            if f % 2 == 0:
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
