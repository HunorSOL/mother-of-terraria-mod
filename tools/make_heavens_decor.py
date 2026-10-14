"""Generates the PLACEHOLDER sprite sheet for the golden bridge posts (no owner design yet).

Output: Tiles/GoldenBridgePost.png  (1x3 tiles: an ornate golden post with a glowing orb; 16 px tiles, 2 px padding)
Replace the PNG with the owner's art of the same size (no code changes).

Run from the project folder:  python3 tools/make_heavens_decor.py      (needs ImageMagick and numpy)
"""
import math
import subprocess

import numpy as np

CLEAR = (0, 0, 0, 0)
BASE = (238, 210, 124, 255)
LIGHT = (255, 246, 200, 255)
SHADE = (196, 158, 78, 255)
DARK = (146, 108, 44, 255)
OUT = (88, 60, 20, 255)
GEM = (130, 214, 255, 255)
GEMLIGHT = (225, 245, 255, 255)


class Canvas:
    def __init__(self, tiles_w, tiles_h):
        self.w, self.h = tiles_w * 16, tiles_h * 16
        self.tw, self.th = tiles_w, tiles_h
        self.px = np.zeros((self.h, self.w, 4), dtype=np.uint8)

    def set(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.px[y, x] = c

    def rect(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.set(x, y, c)

    def poly(self, pts, c):
        for y in range(self.h):
            for x in range(self.w):
                n, hit, j = len(pts), False, len(pts) - 1
                for i in range(n):
                    xi, yi = pts[i]; xj, yj = pts[j]
                    if (yi > y + .5) != (yj > y + .5) and x + .5 < (xj - xi) * (y + .5 - yi) / (yj - yi + 1e-9) + xi:
                        hit = not hit
                    j = i
                if hit:
                    self.set(x, y, c)

    def disc(self, cx, cy, r, c):
        for y in range(self.h):
            for x in range(self.w):
                if (x + .5 - cx) ** 2 + (y + .5 - cy) ** 2 <= r * r:
                    self.set(x, y, c)

    def outline(self):
        a = self.px[..., 3] > 0
        add = []
        for y in range(self.h):
            for x in range(self.w):
                if not a[y, x]:
                    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                        nx, ny = x + dx, y + dy
                        if 0 <= nx < self.w and 0 <= ny < self.h and a[ny, nx]:
                            add.append((x, y)); break
        for x, y in add:
            self.px[y, x] = OUT

    def save(self, path):
        """Writes the padded object sheet: tile (tx, ty) goes to pixel (tx*18, ty*18)."""
        W, H = self.tw * 18 - 2, self.th * 18 - 2
        sheet = np.zeros((H, W, 4), dtype=np.uint8)
        for ty in range(self.th):
            for tx in range(self.tw):
                sheet[ty * 18:ty * 18 + 16, tx * 18:tx * 18 + 16] = self.px[ty * 16:ty * 16 + 16, tx * 16:tx * 16 + 16]
        proc = subprocess.Popen(["magick", "-size", f"{W}x{H}", "-depth", "8", "rgba:-", path], stdin=subprocess.PIPE)
        proc.communicate(sheet.tobytes())
        print(f"wrote {path}: {W}x{H}")


def bridge_post():
    c = Canvas(1, 3)
    c.rect(6, 12, 9, 47, BASE); c.rect(6, 12, 6, 47, LIGHT); c.rect(9, 12, 9, 47, SHADE)
    c.rect(2, 38, 13, 47, BASE); c.rect(2, 38, 13, 39, LIGHT); c.rect(2, 44, 13, 47, SHADE)
    c.rect(4, 11, 11, 13, SHADE)
    c.disc(8, 7, 4.5, DARK); c.disc(8, 7, 3.4, GEM); c.set(7, 6, GEMLIGHT)
    c.outline(); c.save("Tiles/GoldenBridgePost.png")


def basket():
    """The Heavenly Basket: a golden bowl on a pedestal, brimming with crystals (4x3 tiles, PLACEHOLDER)."""
    c = Canvas(4, 3)
    CRY = (170, 220, 255, 255); CRYL = (240, 250, 255, 255); CRYD = (96, 150, 226, 255)
    # crystals sticking out of the bowl
    c.poly([(20, 16), (25, 3), (30, 16)], CRY); c.poly([(25, 3), (27, 9), (25, 16)], CRYL)
    c.poly([(29, 17), (36, 0), (43, 17)], CRY); c.poly([(36, 0), (39, 8), (36, 17)], CRYL); c.poly([(36, 0), (33, 8), (36, 17)], CRYD)
    c.poly([(40, 16), (46, 5), (52, 16)], CRY); c.poly([(46, 5), (48, 10), (46, 16)], CRYL)
    # the bowl
    c.poly([(10, 16), (53, 16), (47, 33), (16, 33)], BASE)
    c.poly([(10, 16), (20, 16), (24, 33), (16, 33)], SHADE)
    c.rect(8, 14, 55, 18, LIGHT); c.rect(8, 17, 55, 18, SHADE)
    for x in range(18, 48, 6):
        c.rect(x, 21, x + 2, 30, DARK)
    # the stem and pedestal
    c.rect(26, 33, 37, 40, BASE); c.rect(26, 33, 28, 40, LIGHT); c.rect(35, 33, 37, 40, SHADE)
    c.rect(10, 41, 53, 47, BASE); c.rect(10, 41, 53, 42, LIGHT); c.rect(10, 45, 53, 47, SHADE)
    c.disc(32, 26, 3, GEM); c.set(31, 25, GEMLIGHT)
    c.outline(); c.save("Tiles/HeavenlyBasket.png")


if __name__ == "__main__":
    bridge_post()
    basket()
