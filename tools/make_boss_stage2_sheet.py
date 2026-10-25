#!/usr/bin/env python3
"""Builds the Mother of Terraria's STAGE 2 animation sheet from the owner's second design, plus the Starlight Lance and cut-outs.

Input : DesignRefs/boss_stage2_design.jpg  (2752x1536, 2026-10-25: the enraged boss in the middle with fiery pink-blue-gold wings,
        the Enraged Trophy and Enraged Treasure Bag on the left, the Starlight Lance and Cosmic Barrage on the right)
Output: NPCs/Bosses/MotherOfTerrariaBoss_Stage2.png   (8 frames of the SAME size as the stage 1 sheet, 420x332, so the hitbox, the
                                                        attack origins and the animation code stay valid; the boss swaps to it when stage 2 starts)
        Projectiles/StarlightLance.png                (the lance pointing straight UP, like the Celestial Spear)
        DesignRefs/boss_stage2_trophy.png, boss_stage2_bag.png, boss_stage2_barrage.png  (cut-outs for later, NOT used yet)
Same method as tools/make_boss_sheet.py (box-filter shrink, small palette, the same wing-flap warp, crisp 2x enlargement); the picture is
fitted into the stage 1 picture size so she keeps her size.
Run from the project folder:  python3 tools/make_boss_stage2_sheet.py      (needs ImageMagick and numpy)
"""
import subprocess
import sys

import numpy as np

sys.path.insert(0, "tools")
import make_boss_sheet as mb   # noqa: E402  (reuses the stage 1 helpers and the animation warp)

SRC = "DesignRefs/boss_stage2_design.jpg"
OUT = "NPCs/Bosses/MotherOfTerrariaBoss_Stage2.png"
WORK_W = 400                    # the cut is found at this width, then fitted into the stage 1 picture size
FIT_W, FIT_H = 210, 130         # the stage 1 picture size in art pixels (frame 420x332 = (210x(130+2*PAD)) x 2)
COLOURS = 60
LANCE_LENGTH = 104


def cut_boss():
    scale = WORK_W / 2752.0
    H = int(round(1536 * scale))
    args = [SRC, "-fill", "#0b111f",
            "-draw", "rectangle 0,640 345,1536",         # trophy and bag (bottom left)
            "-draw", "rectangle 0,1405 610,1536",        # their labels
            "-draw", "rectangle 2300,600 2752,1536",     # lance and barrage (bottom right)
            "-filter", "Box", "-resize", f"{WORK_W}x{H}!"]
    rgb = mb.run_rgb(args, WORK_W, H)
    dark = rgb.max(axis=2) < mb.BG_MAX
    bg = np.zeros_like(dark)
    stack = [(y, x) for y in range(H) for x in (0, WORK_W - 1)] + [(y, x) for x in range(WORK_W) for y in (0, H - 1)]
    while stack:
        y, x = stack.pop()
        if 0 <= y < H and 0 <= x < WORK_W and dark[y, x] and not bg[y, x]:
            bg[y, x] = True
            stack += [(y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)]
    solid = ~bg
    labels, n = mb.components(solid)
    sizes = np.bincount(labels.ravel(), minlength=n + 1)
    keep = np.zeros(n + 1, dtype=bool)
    keep[1:] = sizes[1:] >= 60       # drop sparkles and loose streaks, keep the boss
    solid = keep[labels]
    ys, xs = np.nonzero(solid)
    y0, y1, x0, x1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    print(f"cut found at {WORK_W}px wide: {x1 - x0}x{y1 - y0} (aspect {(x1 - x0) / (y1 - y0):.2f}; stage 1 is {FIT_W / FIT_H:.2f})")
    rgba = np.zeros((y1 - y0, x1 - x0, 4), dtype=np.uint8)
    rgba[..., :3] = rgb[y0:y1, x0:x1]
    rgba[..., 3] = np.where(solid[y0:y1, x0:x1], 255, 0)
    mb.write_rgba(rgba, "/tmp/boss2_cut.png")
    # fit into FIT_W x FIT_H (keeping the proportions, centred, feet on the bottom) with a box filter, then a small palette
    subprocess.check_call(["magick", "/tmp/boss2_cut.png", "-filter", "Box", "-resize", f"{FIT_W}x{FIT_H}",
                           "-background", "none", "-gravity", "south", "-extent", f"{FIT_W}x{FIT_H}", "/tmp/boss2_fit.png"])
    fit = mb.read_rgba("/tmp/boss2_fit.png")
    alpha = fit[..., 3].copy()
    subprocess.check_call(["magick", "/tmp/boss2_fit.png", "-alpha", "off", "-colors", str(COLOURS), "+dither", "/tmp/boss2_q.png"])
    q = mb.read_rgba("/tmp/boss2_q.png")
    q[..., 3] = np.where(alpha > 100, 255, 0)
    return q


def cut_items():
    boxes = {"boss_stage2_trophy": (70, 650, 280, 360), "boss_stage2_bag": (100, 1160, 250, 230),
             "boss_stage2_lance": (2300, 640, 400, 360), "boss_stage2_barrage": (2310, 1100, 380, 330)}
    for name, (x, y, w, h) in boxes.items():
        subprocess.check_call(["magick", SRC, "-crop", f"{w}x{h}+{x}+{y}", "+repage", "-fuzz", "14%", "-fill", "none",
                               "-draw", "color 2,2 floodfill", f"DesignRefs/{name}.png"])
    subprocess.check_call([
        "magick", "DesignRefs/boss_stage2_lance.png", "-background", "none", "-rotate", "-45", "-trim", "+repage",
        "-filter", "Box", "-resize", f"x{LANCE_LENGTH}", "-channel", "A", "-threshold", "45%", "+channel",
        "-colors", "32", "+dither", "-trim", "+repage", "Projectiles/StarlightLance.png"])
    # the design has loose sparks around the lance: keep only the lance itself (the biggest piece and what touches it within 3 px)
    lance = mb.read_rgba("Projectiles/StarlightLance.png")
    solid = lance[..., 3] > 0
    labels, n = mb.components(solid)
    sizes = np.bincount(labels.ravel(), minlength=n + 1)
    sizes[0] = 0
    main = labels == sizes.argmax()
    near = main.copy()
    for _ in range(3):
        grown = near.copy()
        grown[1:] |= near[:-1]
        grown[:-1] |= near[1:]
        grown[:, 1:] |= near[:, :-1]
        grown[:, :-1] |= near[:, 1:]
        near = grown
    lance[..., 3] = np.where(solid & near & (sizes[labels] >= 6), 255, 0)
    mb.write_rgba(lance, "Projectiles/StarlightLance.png")
    subprocess.check_call(["magick", "Projectiles/StarlightLance.png", "-trim", "+repage", "Projectiles/StarlightLance.png"])


def main():
    base = cut_boss()
    frames = [mb.make_frame(base, f) for f in range(mb.FRAMES)]
    frames = [np.repeat(np.repeat(fr, mb.UPSCALE, axis=0), mb.UPSCALE, axis=1) for fr in frames]
    mb.write_rgba(np.concatenate(frames, axis=0), OUT)
    print(f"wrote {OUT}: frame {frames[0].shape[1]}x{frames[0].shape[0]} x {mb.FRAMES} frames")
    cut_items()
    print(subprocess.check_output(["magick", "identify", "Projectiles/StarlightLance.png"]).decode())


if __name__ == "__main__":
    main()
