"""Builds the golden statue's tile sheet from the owner's design.

Input : DesignRefs/statue_base.png  (128x110: the design cut out of its checkerboard, at its true pixel size of 64x55, scaled 2x)
Output: Tiles/GoldenStatue.png      (142x124: an 8x7-tile object, 16 px tiles with 2 px padding between them)

The statue stands on the bottom row, centred. If the owner supplies a new design, put the cut-out PNG at
DesignRefs/statue_base.png (up to 128x112) and re-run:  python3 tools/make_statue_sheet.py   (needs ImageMagick and numpy)
If the size changes, also update GoldenStatue's TilesWide / TilesHigh in Tiles/HeavensObjectTile.cs.
"""
import subprocess

import numpy as np

SRC = "DesignRefs/statue_base.png"
OUT = "Tiles/GoldenStatue.png"
TILES_W, TILES_H = 8, 7


def read_rgba(path):
    w, h = map(int, subprocess.check_output(["magick", "identify", "-format", "%w %h", path]).split())
    raw = subprocess.check_output(["magick", path, "-depth", "8", "rgba:-"])
    return np.frombuffer(raw, dtype=np.uint8).reshape(h, w, 4)


def main():
    art = read_rgba(SRC)
    h, w, _ = art.shape
    lw, lh = TILES_W * 16, TILES_H * 16
    assert w <= lw and h <= lh, f"art {w}x{h} does not fit {lw}x{lh}"
    logical = np.zeros((lh, lw, 4), dtype=np.uint8)
    x0, y0 = (lw - w) // 2, lh - h          # centred, standing on the bottom row
    logical[y0:y0 + h, x0:x0 + w] = art

    W, H = TILES_W * 18 - 2, TILES_H * 18 - 2
    sheet = np.zeros((H, W, 4), dtype=np.uint8)
    for ty in range(TILES_H):
        for tx in range(TILES_W):
            sheet[ty * 18:ty * 18 + 16, tx * 18:tx * 18 + 16] = logical[ty * 16:ty * 16 + 16, tx * 16:tx * 16 + 16]
    proc = subprocess.Popen(["magick", "-size", f"{W}x{H}", "-depth", "8", "rgba:-", OUT], stdin=subprocess.PIPE)
    proc.communicate(sheet.tobytes())
    print(f"wrote {OUT}: {W}x{H}")


if __name__ == "__main__":
    main()
