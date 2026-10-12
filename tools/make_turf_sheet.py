"""Builds the Heavens turf tile sheet: luminous golden grass over a pale body.

Input : tools/templates/block_sheet_template.png  (tModLoader ExampleMod's standard block sheet, 288x270: 16x15 frames
        of 16 px with 2 px padding. It decides which frame shows for which neighbours.)
Output: Tiles/HeavensTurf.png

The template's light fill is repainted as golden turf, its dark edge lines as deep amber, and the top edge of every
frame that has one gets a bright strip so the grass looks lit. Unused cells stay transparent.

Run from the project folder:  python3 tools/make_turf_sheet.py      (needs ImageMagick and numpy)
"""
import subprocess

import numpy as np

SRC = "tools/templates/block_sheet_template.png"
OUT = "Tiles/HeavensTurf.png"
CELL = 18  # 16 px frame + 2 px padding

FILL_BASE = (236, 194, 72)
FILL_LIGHT = (252, 224, 118)
FILL_DARK = (208, 160, 50)
SPARKLE = (255, 246, 190)
OUTLINES = {  # template outline colours -> deep amber shades
    (18, 18, 46): (122, 78, 14),
    (45, 46, 77): (168, 116, 28),
    (69, 71, 94): (198, 148, 48),
    (114, 81, 56): (140, 92, 20),
    (151, 107, 75): (178, 124, 34),
}
TEMPLATE_FILL = {(234, 241, 240), (174, 192, 192)}
TOP_GLOW = (255, 238, 150)


def read_rgba(path):
    w, h = map(int, subprocess.check_output(["magick", "identify", "-format", "%w %h", path]).split())
    raw = subprocess.check_output(["magick", path, "-depth", "8", "rgba:-"])
    return np.frombuffer(raw, dtype=np.uint8).reshape(h, w, 4).copy()


def noise(x, y):
    """A small repeatable hash, so the speckle looks the same every time."""
    n = (x * 374761393 + y * 668265263) & 0xFFFFFFFF
    n = ((n ^ (n >> 13)) * 1274126177) & 0xFFFFFFFF
    return (n ^ (n >> 16)) % 100


def main():
    t = read_rgba(SRC)
    H, W, _ = t.shape
    out = np.zeros_like(t)
    for cy in range(H // CELL):
        for cx in range(W // CELL):
            y0, x0 = cy * CELL, cx * CELL
            cell = t[y0:y0 + 16, x0:x0 + 16]
            rgb = cell[..., :3]
            # an unused cell is one solid dark colour in the template: leave it transparent
            if (rgb == rgb[0, 0]).all() and tuple(rgb[0, 0]) == (18, 18, 46):
                continue
            has_top_edge = sum(tuple(rgb[0, x]) in OUTLINES for x in range(16)) >= 12
            for y in range(16):
                for x in range(16):
                    c = tuple(int(v) for v in cell[y, x, :3])
                    if c in OUTLINES:
                        # small dark squares in the middle of a frame are only template markers: paint them as fill
                        edge = x in (0, 15) or y in (0, 15) or x in (1, 14) or y in (1, 14)
                        if not edge and 4 <= x <= 11 and 4 <= y <= 11:
                            c = (234, 241, 240)
                        else:
                            out[y0 + y, x0 + x] = (*OUTLINES[c], 255)
                            continue
                    if c in TEMPLATE_FILL:
                        n = noise(x0 + x, y0 + y)
                        col = FILL_BASE
                        if n < 16:
                            col = FILL_LIGHT
                        elif n < 30:
                            col = FILL_DARK
                        elif n > 97:
                            col = SPARKLE
                        if has_top_edge and 1 <= y <= 2:
                            col = TOP_GLOW if y == 1 else FILL_LIGHT
                        out[y0 + y, x0 + x] = (*col, 255)
    proc = subprocess.Popen(["magick", "-size", f"{W}x{H}", "-depth", "8", "rgba:-", OUT], stdin=subprocess.PIPE)
    proc.communicate(out.tobytes())
    print(f"wrote {OUT}: {W}x{H}")


if __name__ == "__main__":
    main()
