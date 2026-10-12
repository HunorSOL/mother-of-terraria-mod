"""Generates the PLACEHOLDER sprite sheets for the Heavens ruins and bridge posts (no owner designs yet).

Outputs (each is a multi-tile object sheet: 16 px tiles with 2 px padding between them):
  Tiles/GoldenColumn.png      2x6 tiles   a tall intact golden marble column
  Tiles/BrokenColumn.png      2x3 tiles   a snapped column stump with rubble
  Tiles/RuinedArch.png        7x6 tiles   two columns and a broken arch between them
  Tiles/GoldenBridgePost.png  1x3 tiles   an ornate golden post with a glowing orb, for the golden bridges
Replace any PNG with the owner's art of the same size (no code changes).

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


def column(c, x0, width, top, bottom, broken_top=None):
    """A fluted column. x0 = left edge, width in px. broken_top: list of top y per x (jagged break) or None."""
    x1 = x0 + width - 1
    for x in range(x0, x1 + 1):
        t = top if broken_top is None else broken_top[x - x0]
        for y in range(t, bottom + 1):
            rel = (x - x0) / max(1, width - 1)
            col = LIGHT if rel < 0.2 else SHADE if rel > 0.78 else BASE
            if (x - x0) % 5 == 3:
                col = DARK if rel > 0.4 else SHADE  # flute lines
            c.set(x, y, col)
    # base and capital trims
    c.rect(x0 - 2, bottom - 7, x1 + 2, bottom - 4, BASE); c.rect(x0 - 3, bottom - 3, x1 + 3, bottom, SHADE)
    c.rect(x0 - 2, bottom - 7, x1 + 2, bottom - 7, LIGHT)
    if broken_top is None:
        c.rect(x0 - 3, top - 6, x1 + 3, top - 3, BASE); c.rect(x0 - 3, top - 6, x1 + 3, top - 6, LIGHT)
        c.rect(x0 - 2, top - 2, x1 + 2, top - 1, SHADE)


def golden_column():
    c = Canvas(2, 6)
    column(c, 6, 20, 14, 95)
    c.set(15, 4, GEM); c.set(16, 4, GEM)
    c.outline(); c.save("Tiles/GoldenColumn.png")


def broken_column():
    c = Canvas(2, 3)
    jag = [28 + ((x * 7) % 6) - (3 if x % 5 == 0 else 0) for x in range(20)]
    column(c, 6, 20, 26, 47, broken_top=jag)
    # rubble: fallen blocks on the ground to the sides
    c.rect(0, 40, 4, 47, BASE); c.rect(0, 40, 4, 41, LIGHT); c.rect(27, 42, 31, 47, SHADE); c.rect(27, 42, 31, 43, BASE)
    c.outline(); c.save("Tiles/BrokenColumn.png")


def ruined_arch():
    c = Canvas(7, 6)
    column(c, 4, 20, 22, 95)                                  # left column, intact
    jag = [58 + ((x * 5) % 7) - (4 if x % 4 == 0 else 0) for x in range(20)]
    column(c, 88, 20, 56, 95, broken_top=jag)                  # right column, snapped halfway
    # a stone lintel running from the left column's capital, snapped off raggedly before the right column
    for x in range(22, 80):
        edge = 78 - (x % 4)           # ragged broken end
        sag = int(4 * ((x - 22) / 56.0) ** 2)  # droops slightly toward the break
        if x < edge:
            for y in range(15 + sag, 25 + sag):
                rel = (y - 15 - sag) / 9.0
                c.set(x, y, LIGHT if rel < 0.2 else SHADE if rel > 0.8 else BASE)
    c.rect(48, 18, 53, 26, DARK); c.rect(49, 19, 52, 25, GEM)  # gem set into the lintel
    c.set(50, 20, GEMLIGHT)
    # rubble in the gap
    c.rect(60, 88, 70, 95, BASE); c.rect(60, 88, 70, 89, LIGHT); c.rect(72, 90, 80, 95, SHADE)
    c.outline(); c.save("Tiles/RuinedArch.png")


def bridge_post():
    c = Canvas(1, 3)
    c.rect(6, 12, 9, 47, BASE); c.rect(6, 12, 6, 47, LIGHT); c.rect(9, 12, 9, 47, SHADE)
    c.rect(2, 38, 13, 47, BASE); c.rect(2, 38, 13, 39, LIGHT); c.rect(2, 44, 13, 47, SHADE)
    c.rect(4, 11, 11, 13, SHADE)
    c.disc(8, 7, 4.5, DARK); c.disc(8, 7, 3.4, GEM); c.set(7, 6, GEMLIGHT)
    c.outline(); c.save("Tiles/GoldenBridgePost.png")


if __name__ == "__main__":
    golden_column(); broken_column(); ruined_arch(); bridge_post()
