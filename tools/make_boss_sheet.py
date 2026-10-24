#!/usr/bin/env python3
"""Builds the Mother of Terraria animation sheet from the owner's design, and cuts out the design's side items.

Input : DesignRefs/boss_design.jpg   (2752x1536 mockup: the boss in the middle, trophy + treasure bag, Celestial Spear
                                      and Gold Crystal Star around her, on a dark grid background)
Output: NPCs/Bosses/MotherOfTerrariaBoss.png   (8 frames stacked vertically; wings flap, hair and robe sway)
        DesignRefs/boss_trophy.png, boss_bag.png, boss_spear.png, boss_star.png  (cut-outs for later, NOT used yet)

"Terraria-like": the picture is shrunk with a box filter to a small size and reduced to a small palette, like
tools/make_crystal_icon.py. The animation is a smooth warp of the single picture (a stand-in for hand-drawn frames).
If the owner supplies real frames, replace the PNG (same frame size, 8 frames, or update the constants in
MotherOfTerrariaBoss.cs).

Run from the project folder:  python3 tools/make_boss_sheet.py      (needs ImageMagick and numpy)
Prints the frame size and the body width, which MotherOfTerrariaBoss.cs uses for the hitbox.
"""
import math
import subprocess

import numpy as np

SRC = "DesignRefs/boss_design.jpg"
OUT = "NPCs/Bosses/MotherOfTerrariaBoss.png"
FRAMES = 8
WIDTH = 240          # width of the picture before enlarging (one "art pixel" is 1 pixel at this size)
UPSCALE = 2          # every art pixel is then made 2x2 real pixels (crisp, no blur): the in-game frame is picture width * 2 (about 420 px)
PAD = 18             # empty rows above and below so flapping wings are never cut off
COLOURS = 56         # palette size
BG_MAX = 58          # a pixel whose brightest channel is below this is "background" (if it touches the edge)


def run_rgb(args, w, h):
    raw = subprocess.check_output(["magick"] + args + ["-depth", "8", "rgb:-"])
    return np.frombuffer(raw, dtype=np.uint8).reshape(h, w, 3).copy()


def read_rgba(path):
    w, h = map(int, subprocess.check_output(["magick", "identify", "-format", "%w %h", path]).split())
    raw = subprocess.check_output(["magick", path, "-depth", "8", "rgba:-"])
    return np.frombuffer(raw, dtype=np.uint8).reshape(h, w, 4).copy()


def write_rgba(arr, path):
    h, w, _ = arr.shape
    proc = subprocess.Popen(["magick", "-size", f"{w}x{h}", "-depth", "8", "rgba:-", path], stdin=subprocess.PIPE)
    proc.communicate(arr.tobytes())
    assert proc.returncode == 0


def components(mask):
    """Connected pieces (8-neighbour) of a boolean mask. Returns an int label array and the number of labels."""
    h, w = mask.shape
    labels = np.zeros((h, w), dtype=np.int32)
    n = 0
    for y in range(h):
        for x in range(w):
            if mask[y, x] and labels[y, x] == 0:
                n += 1
                stack = [(y, x)]
                labels[y, x] = n
                while stack:
                    cy, cx = stack.pop()
                    for dy in (-1, 0, 1):
                        for dx in (-1, 0, 1):
                            ny, nx = cy + dy, cx + dx
                            if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and labels[ny, nx] == 0:
                                labels[ny, nx] = n
                                stack.append((ny, nx))
    return labels, n


def cut_boss():
    # 1. Blank out the side items (they are not part of the boss), then shrink with a box filter.
    scale = WIDTH / 2752.0
    H = int(round(1536 * scale))
    args = [SRC, "-fill", "#0b111f",
            "-draw", "rectangle 0,690 420,1536",        # trophy and bag (bottom left)
            "-draw", "rectangle 2265,640 2752,1536",    # spear and star (bottom right)
            "-filter", "Box", "-resize", f"{WIDTH}x{H}!"]
    rgb = run_rgb(args, WIDTH, H)
    bright = rgb.max(axis=2)
    dark = bright < BG_MAX

    # 2. Background = dark pixels connected to the picture's border.
    bg = np.zeros_like(dark)
    stack = [(y, x) for y in range(H) for x in (0, WIDTH - 1)] + [(y, x) for x in range(WIDTH) for y in (0, H - 1)]
    while stack:
        y, x = stack.pop()
        if 0 <= y < H and 0 <= x < WIDTH and dark[y, x] and not bg[y, x]:
            bg[y, x] = True
            stack += [(y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)]
    solid = ~bg

    # 3. Drop the tiny floating sparkles; keep the boss, the halo stars and anything else of real size.
    labels, n = components(solid)
    sizes = np.bincount(labels.ravel(), minlength=n + 1)
    keep = np.zeros(n + 1, dtype=bool)
    keep[1:] = sizes[1:] >= 40
    solid = keep[labels]

    ys, xs = np.nonzero(solid)
    y0, y1, x0, x1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    rgba = np.zeros((y1 - y0, x1 - x0, 4), dtype=np.uint8)
    rgba[..., :3] = rgb[y0:y1, x0:x1]
    rgba[..., 3] = np.where(solid[y0:y1, x0:x1], 255, 0)
    write_rgba(rgba, "/tmp/boss_cut.png")
    # 4. Small palette (a Terraria sprite uses few colours); no dithering.
    subprocess.check_call(["magick", "/tmp/boss_cut.png", "-alpha", "off", "-colors", str(COLOURS), "+dither", "/tmp/boss_q.png"])
    q = read_rgba("/tmp/boss_q.png")
    q[..., 3] = rgba[..., 3]
    return q


def make_frame(base, f):
    bh, bw, _ = base.shape
    H = bh + 2 * PAD
    phase = 2 * math.pi * f / FRAMES
    yy, xx = np.mgrid[0:H, 0:bw].astype(float)
    cx = (bw - 1) / 2.0
    body_half = 0.17 * bw
    side = np.clip((np.abs(xx - cx) - body_half) / (bw / 2 - body_half), 0, 1)   # 0 at the body, 1 at the wing tips
    upper = np.clip((0.62 * bh + PAD - yy) / (0.5 * bh), 0, 1)                    # the big upper wings
    lower = np.clip((yy - (0.45 * bh + PAD)) / (0.3 * bh), 0, 1) * np.clip((0.95 * bh + PAD - yy) / (0.2 * bh), 0, 1)

    dy = -15.0 * math.sin(phase) * side ** 1.4 * (0.35 + 0.65 * upper)            # upper wings beat up and down
    dy += 6.0 * math.sin(phase + 0.9) * side ** 1.2 * lower                       # lower wings follow a little later
    # Hair and robe sway gently: more the lower down, only near the body.
    near = np.clip(1 - side * 1.6, 0, 1)
    dx = 1.6 * np.sin(phase - yy * 0.045) * np.clip((yy - 0.35 * bh - PAD) / (0.6 * bh), 0, 1) * near
    dy += 1.5 * math.sin(phase * 2) * near                                        # the body breathes

    src_x = np.rint(xx - dx).astype(int)
    src_y = np.rint(yy - PAD - dy).astype(int)
    ok = (src_x >= 0) & (src_x < bw) & (src_y >= 0) & (src_y < bh)
    out = np.zeros((H, bw, 4), dtype=np.uint8)
    out[ok] = base[src_y[ok], src_x[ok]]
    return out


def cut_items():
    # name: (x, y, width, height) in the original picture
    boxes = {
        "boss_trophy": (80, 700, 270, 300),
        "boss_bag": (100, 1160, 240, 250),
        "boss_spear": (2300, 650, 400, 345),
        "boss_star": (2250, 1130, 450, 275),
    }
    for name, (x, y, w, h) in boxes.items():
        subprocess.check_call([
            "magick", SRC, "-crop", f"{w}x{h}+{x}+{y}", "+repage", "-fuzz", "14%", "-fill", "none",
            "-draw", "color 2,2 floodfill", f"DesignRefs/{name}.png"])


def main():
    base = cut_boss()
    frames = [make_frame(base, f) for f in range(FRAMES)]
    frames = [np.repeat(np.repeat(fr, UPSCALE, axis=0), UPSCALE, axis=1) for fr in frames]  # crisp enlargement
    sheet = np.concatenate(frames, axis=0)
    write_rgba(sheet, OUT)
    fh, fw = frames[0].shape[:2]
    print(f"wrote {OUT}: frame {fw}x{fh}, {FRAMES} frames, picture {base.shape[1]}x{base.shape[0]}")
    cut_items()
    print("wrote DesignRefs/boss_trophy.png, boss_bag.png, boss_spear.png, boss_star.png")


if __name__ == "__main__":
    main()
