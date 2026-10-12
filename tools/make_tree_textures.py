"""Builds the three textures of the Heavenly Tree (a real, tile-based Terraria tree) from the owner's tree design.

Inputs : tools/templates/tree_trunk_template.png     tModLoader ExampleMod's trunk sheet (176x264, 22 px cells): it decides
                                                     which cell is used for which part of the trunk (roots, plain, branch joins, top)
         tools/templates/tree_branches_template.png  its branch sheet (84x126: a left and a right branch, 3 variants)
         DesignRefs/tree_tall.png, tree_round.png    the owner's two canopies (cut out at their true pixel size)
Outputs: Tiles/Trees/HeavenlyTree.png            trunk sheet: golden bark, dark-brown outline
         Tiles/Trees/HeavenlyTree_Branches.png   branches: golden bark with the owner's pastel cloud foliage at the tips
         Tiles/Trees/HeavenlyTree_Tops.png       3 crown frames (the owner's canopies) with a short bark stub at the bottom

The game draws the crown with its bottom-centre on top of the trunk and sways it in the wind, so the crown frame size
(CROWN_W x CROWN_H below) must match HeavenlyTree.cs. If the owner supplies new art, replace the DesignRefs PNGs
(or paint the output sheets by hand) and re-run:  python3 tools/make_tree_textures.py   (needs ImageMagick and numpy)
"""
import subprocess

import numpy as np

TRUNK_T = "tools/templates/tree_trunk_template.png"
BRANCH_T = "tools/templates/tree_branches_template.png"
TALL = "DesignRefs/tree_tall.png"
ROUND = "DesignRefs/tree_round.png"
OUT_TRUNK = "Tiles/Trees/HeavenlyTree.png"
OUT_BRANCH = "Tiles/Trees/HeavenlyTree_Branches.png"
OUT_TOPS = "Tiles/Trees/HeavenlyTree_Tops.png"

CROWN_W, CROWN_H = 104, 84          # one crown frame; frames are CROWN_W + 2 apart in the sheet
BARK_OUTLINE = (92, 54, 30)
LEAF_OUTLINE = (96, 126, 196)
BARK = [(150, 100, 44), (198, 146, 68), (198, 146, 68), (232, 188, 104), (198, 146, 68), (250, 226, 152), (198, 146, 68)]


def read_rgba(path):
    w, h = map(int, subprocess.check_output(["magick", "identify", "-format", "%w %h", path]).split())
    raw = subprocess.check_output(["magick", path, "-depth", "8", "rgba:-"])
    return np.frombuffer(raw, dtype=np.uint8).reshape(h, w, 4).copy()


def write_rgba(arr, path):
    h, w, _ = arr.shape
    proc = subprocess.Popen(["magick", "-size", f"{w}x{h}", "-depth", "8", "rgba:-", path], stdin=subprocess.PIPE)
    proc.communicate(arr.tobytes())
    assert proc.returncode == 0
    print(f"wrote {path}: {w}x{h}")


def bark(x, y, width=20):
    """Golden bark: vertical grain, lighter on the right, darker on the left. Repeats every 8 px vertically, so
    the trunk pieces (which overlap by 4 px) join without a visible seam."""
    streak = BARK[(x * 5 + (1 if (y % 8) >= 6 and x % 3 == 0 else 0)) % len(BARK)]
    shade = -22 if x < 3 else 14 if x > width - 5 else 0
    return tuple(max(0, min(255, c + shade)) for c in streak)


def foliage_pattern():
    """The interior of the owner's tall canopy, used to fill the leaf blobs on the branches."""
    art = read_rgba(TALL)
    return art[22:58, 14:86, :3]


def foliage(pattern, x, y):
    h, w, _ = pattern.shape
    px = x % (2 * w)
    py = y % (2 * h)
    px = px if px < w else 2 * w - 1 - px     # mirror-tile so the pattern has no hard seams
    py = py if py < h else 2 * h - 1 - py
    return tuple(int(v) for v in pattern[py, px])


def is_outline_pixel(rgb):
    return tuple(rgb) == (0, 0, 0) or tuple(rgb) == (50, 50, 50)


def build_trunk():
    t = read_rgba(TRUNK_T)
    out = np.zeros_like(t)
    H, W, _ = t.shape
    for y in range(H):
        for x in range(W):
            if t[y, x, 3] == 0:
                continue
            rgb = tuple(int(v) for v in t[y, x, :3])
            cx, cy = x % 22, y % 22
            if rgb == (255, 255, 255):
                out[y, x] = (*bark(cx, cy), 255)
            elif is_outline_pixel(rgb):
                out[y, x] = (*BARK_OUTLINE, 255)
            # the template's pink guide colour (padding between cells) stays transparent
    write_rgba(out, OUT_TRUNK)


def build_branches():
    t = read_rgba(BRANCH_T)
    H, W, _ = t.shape
    pattern = foliage_pattern()
    out = np.zeros_like(t)
    leaf = (t[..., :3] == 204).all(axis=2) & (t[..., 3] > 0)
    for y in range(H):
        for x in range(W):
            if t[y, x, 3] == 0:
                continue
            rgb = tuple(int(v) for v in t[y, x, :3])
            if rgb == (204, 204, 204):
                out[y, x] = (*foliage(pattern, x, y), 255)
            elif rgb == (255, 255, 255):
                out[y, x] = (*bark(x % 22, y % 22), 255)
            elif is_outline_pixel(rgb):
                near_leaf = any(0 <= y + dy < H and 0 <= x + dx < W and leaf[y + dy, x + dx]
                                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)))
                out[y, x] = (*(LEAF_OUTLINE if near_leaf else BARK_OUTLINE), 255)
    write_rgba(out, OUT_BRANCH)


def paste(canvas, art, x0, y0, mirror=False):
    if mirror:
        art = art[:, ::-1]
    h, w, _ = art.shape
    for y in range(h):
        for x in range(w):
            if art[y, x, 3] > 0 and 0 <= y0 + y < canvas.shape[0] and 0 <= x0 + x < canvas.shape[1]:
                canvas[y0 + y, x0 + x] = art[y, x]


def build_tops():
    tall = read_rgba(TALL)[0:69]     # the crown of the tall tree (above its side branches)
    rnd = read_rgba(ROUND)[0:55]     # the crown of the round tree (above its side puffs)
    sheet = np.zeros((CROWN_H + 2, 3 * (CROWN_W + 2), 4), dtype=np.uint8)
    frames = [(tall, False, 0), (rnd, False, 4), (tall, True, 0)]
    for i, (art, mirror, lift) in enumerate(frames):
        frame = np.zeros((CROWN_H, CROWN_W, 4), dtype=np.uint8)
        h, w, _ = art.shape
        paste(frame, art, (CROWN_W - w) // 2, lift, mirror)
        # a short bark stub at the bottom centre: this is where the crown sits on the trunk
        for y in range(CROWN_H - 20, CROWN_H):
            for x in range(CROWN_W // 2 - 8, CROWN_W // 2 + 8):
                edge = x in (CROWN_W // 2 - 8, CROWN_W // 2 + 7)
                frame[y, x] = (*(BARK_OUTLINE if edge else bark(x - (CROWN_W // 2 - 8), y)), 255)
        sheet[0:CROWN_H, i * (CROWN_W + 2):i * (CROWN_W + 2) + CROWN_W] = frame
    write_rgba(sheet, OUT_TOPS)


if __name__ == "__main__":
    build_trunk()
    build_branches()
    build_tops()
