"""Builds the tile sheets for the Heavens' golden-cloud trees, saplings and stumps from the owner's tree design.

Inputs : DesignRefs/tree_tall.png, tree_round.png, tree_sapling.png, tree_stump.png
         (the design cut out of its checkerboard at its true pixel size; tree_puff.png and tree_branch.png are
          the other pieces of the design sheet, kept for later use)
Outputs: Tiles/HeavenlyTreeTall.png, HeavenlyTreeRound.png, HeavenlySapling.png, HeavenlyStump.png
         (multi-tile object sheets: 16 px tiles with 2 px padding between them; the art stands on the bottom row, centred)

If the owner supplies new art, replace the DesignRefs PNG and re-run:  python3 tools/make_tree_sheets.py
(needs ImageMagick and numpy). If a picture needs more tiles than before, also update TilesWide / TilesHigh of its
class in Tiles/HeavensObjectTile.cs; this script prints the sizes it used.
"""
import subprocess

import numpy as np

# source picture -> output sheet, in tiles (wide, high) [must match Tiles/HeavensObjectTile.cs]
OBJECTS = [
    ("DesignRefs/tree_tall.png", "Tiles/HeavenlyTreeTall.png", 7, 12),
    ("DesignRefs/tree_round.png", "Tiles/HeavenlyTreeRound.png", 6, 9),
    ("DesignRefs/tree_sapling.png", "Tiles/HeavenlySapling.png", 2, 3),
    ("DesignRefs/tree_stump.png", "Tiles/HeavenlyStump.png", 3, 2),
]


def read_rgba(path):
    w, h = map(int, subprocess.check_output(["magick", "identify", "-format", "%w %h", path]).split())
    raw = subprocess.check_output(["magick", path, "-depth", "8", "rgba:-"])
    return np.frombuffer(raw, dtype=np.uint8).reshape(h, w, 4)


def build(src, out, tiles_w, tiles_h):
    art = read_rgba(src)
    h, w, _ = art.shape
    lw, lh = tiles_w * 16, tiles_h * 16
    assert w <= lw and h <= lh, f"{src}: art {w}x{h} does not fit {lw}x{lh} ({tiles_w}x{tiles_h} tiles)"
    logical = np.zeros((lh, lw, 4), dtype=np.uint8)
    x0, y0 = (lw - w) // 2, lh - h
    logical[y0:y0 + h, x0:x0 + w] = art

    W, H = tiles_w * 18 - 2, tiles_h * 18 - 2
    sheet = np.zeros((H, W, 4), dtype=np.uint8)
    for ty in range(tiles_h):
        for tx in range(tiles_w):
            sheet[ty * 18:ty * 18 + 16, tx * 18:tx * 18 + 16] = logical[ty * 16:ty * 16 + 16, tx * 16:tx * 16 + 16]
    proc = subprocess.Popen(["magick", "-size", f"{W}x{H}", "-depth", "8", "rgba:-", out], stdin=subprocess.PIPE)
    proc.communicate(sheet.tobytes())
    print(f"wrote {out}: {W}x{H} ({tiles_w}x{tiles_h} tiles, art {w}x{h})")


if __name__ == "__main__":
    for spec in OBJECTS:
        build(*spec)
