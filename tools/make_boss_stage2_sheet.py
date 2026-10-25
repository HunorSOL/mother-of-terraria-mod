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
WORK_W = 1000                   # the background is found at this width; the picture itself is shrunk once, straight from the original
POCKET_MIN = 30                 # an enclosed dark area at least this big (working pixels) is background (a gap between feathers), not shadow
FRINGE_MAX = 85                 # dark pixels touching the background and darker than this are the soft edge of the background
HAZE_MAX = 100                  # fairly dark navy haze (not only the plain background) can be background if it forms a thick blob
HAZE_SHAVE = 3                  # pixels shaved off all round to tell thick blobs from thin outlines
HAZE_MIN = 25                   # a blob this big (working pixels) after shaving counts
MIN_PIECE = 300                 # loose pieces smaller than this (sparkles, streaks) are dropped
FIT_W, FIT_H = 210, 130         # the stage 1 picture size in art pixels (frame 420x332 = (210x(130+2*PAD)) x 2)
COLOURS = 64
LANCE_LENGTH = 104


def cut_boss():
    # 1. Find what is background at a medium size (fast), blanking out the side items first.
    scale = WORK_W / 2752.0
    H = int(round(1536 * scale))
    args = [SRC, "-fill", "#0b111f",
            "-draw", "rectangle 0,640 345,1536",         # trophy and bag (bottom left)
            "-draw", "rectangle 0,1405 610,1536",        # their labels
            "-draw", "rectangle 2300,600 2752,1536",     # lance and barrage (bottom right)
            "-filter", "Box", "-resize", f"{WORK_W}x{H}!"]
    rgb = mb.run_rgb(args, WORK_W, H)
    bright = rgb.max(axis=2)
    dark = bright < mb.BG_MAX
    # Background = dark areas touching the picture's border, AND the big enclosed dark pockets (the gaps between feathers and flames
    # that the border fill cannot reach; they were the black spots). Small dark areas inside the boss (shadows, eyes) stay.
    labels, n = mb.components(dark)
    sizes = np.bincount(labels.ravel(), minlength=n + 1)
    border = np.unique(np.concatenate([labels[0], labels[-1], labels[:, 0], labels[:, -1]]))
    is_bg = np.zeros(n + 1, dtype=bool)
    is_bg[border] = True
    is_bg |= sizes >= POCKET_MIN
    is_bg[0] = False
    bg = is_bg[labels]
    # a thin dark fringe left by the picture's soft edges: dark pixels right next to background are background too
    near = np.zeros_like(bg)
    near[1:] |= bg[:-1]
    near[:-1] |= bg[1:]
    near[:, 1:] |= bg[:, :-1]
    near[:, :-1] |= bg[:, 1:]
    bg |= near & (bright < FRINGE_MAX)
    # Dark navy haze that is a little brighter than the plain background (behind the halo and hair) is background too, but the boss's own
    # dark outlines are not: outlines are thin, the haze is a thick blob. So take the fairly dark pixels, shave off 3 pixels all round
    # (thin outlines vanish, thick blobs survive), keep the blobs of real size, and grow them back to their old edge.
    hazy = bright < HAZE_MAX
    core = hazy.copy()
    for _ in range(HAZE_SHAVE):
        shaved = core.copy()
        shaved[1:] &= core[:-1]
        shaved[:-1] &= core[1:]
        shaved[:, 1:] &= core[:, :-1]
        shaved[:, :-1] &= core[:, 1:]
        core = shaved
    labels, n = mb.components(core)
    sizes = np.bincount(labels.ravel(), minlength=n + 1)
    big = np.zeros(n + 1, dtype=bool)
    big[1:] = sizes[1:] >= HAZE_MIN
    core = big[labels]
    for _ in range(HAZE_SHAVE + 1):
        grown = core.copy()
        grown[1:] |= core[:-1]
        grown[:-1] |= core[1:]
        grown[:, 1:] |= core[:, :-1]
        grown[:, :-1] |= core[:, 1:]
        core = grown & hazy
    bg |= core
    solid = ~bg
    # drop loose sparkles and streaks, keep the boss
    labels, n = mb.components(solid)
    sizes = np.bincount(labels.ravel(), minlength=n + 1)
    keep = np.zeros(n + 1, dtype=bool)
    keep[1:] = sizes[1:] >= MIN_PIECE
    solid = keep[labels]
    ys, xs = np.nonzero(solid)
    y0, y1, x0, x1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    print(f"cut found: {x1 - x0}x{y1 - y0} at {WORK_W}px wide (aspect {(x1 - x0) / (y1 - y0):.2f}; stage 1 is {FIT_W / FIT_H:.2f})")

    # 2. Use that mask on the ORIGINAL picture and shrink it ONCE (a single box filter, so the pixels stay sharp).
    full = mb.run_rgb([SRC], 2752, 1536)
    xi = np.minimum(np.arange(2752) * WORK_W // 2752, WORK_W - 1)
    yi = np.minimum(np.arange(1536) * H // 1536, H - 1)
    full_mask = solid[np.ix_(yi, xi)]
    f = 2752.0 / WORK_W
    cx0, cy0, cx1, cy1 = int(x0 * f), int(y0 * f), min(2752, int(x1 * f) + 1), min(1536, int(y1 * f) + 1)
    rgba = np.zeros((cy1 - cy0, cx1 - cx0, 4), dtype=np.uint8)
    rgba[..., :3] = full[cy0:cy1, cx0:cx1]
    rgba[..., 3] = np.where(full_mask[cy0:cy1, cx0:cx1], 255, 0)
    mb.write_rgba(rgba, "/tmp/boss2_cut.png")
    subprocess.check_call(["magick", "/tmp/boss2_cut.png", "-filter", "Box", "-resize", f"{FIT_W}x{FIT_H}",
                           "-background", "none", "-gravity", "south", "-extent", f"{FIT_W}x{FIT_H}", "/tmp/boss2_fit.png"])
    fit = mb.read_rgba("/tmp/boss2_fit.png")
    alpha = fit[..., 3].copy()
    # 3. A small palette (Terraria look), no dithering; the edge is cut hard (opaque or clear).
    subprocess.check_call(["magick", "/tmp/boss2_fit.png", "-alpha", "off", "-colors", str(COLOURS), "+dither", "/tmp/boss2_q.png"])
    q = mb.read_rgba("/tmp/boss2_q.png")
    q[..., 3] = np.where(alpha > 128, 255, 0)
    # where the face is in the fitted picture (the design's face centre, carried through the same crop and shrink)
    cw, ch = cx1 - cx0, cy1 - cy0
    k = min(FIT_W / cw, FIT_H / ch)
    face_x = (FACE_X - cx0) * k + (FIT_W - cw * k) / 2
    face_y = (FACE_Y - cy0) * k + (FIT_H - ch * k)
    paint_face(q, int(round(face_x)), int(round(face_y)))
    return q


# THE ANGRY FACE (owner, 2026-10-25: her second phase face is angry on purpose and the eyes and face must show). At this size the whole face
# is about 8x7 art pixels, so the shrink turns it into a pale blob; the features are painted by hand here instead: glowing white eyes,
# brows that slant down toward the nose, a shaded nose and a frowning mouth. Each row: (dy, {dx: colour letter}).
SKIN = (246, 213, 186)
SHADE = (222, 170, 150)
BROW = (70, 38, 125)
EYE = (255, 255, 255)
GLOW = (196, 240, 255)
MOUTH = (150, 70, 110)
FACE = [
    # dy, features
    (-4, {-4: "B", -3: "B", 3: "B", 4: "B"}),                 # outer ends of the brows, raised
    (-3, {-2: "B", -1: "B", 1: "B", 2: "B"}),                 # inner ends lower: the angry V
    (-2, {-4: "g", -3: "E", -2: "E", -1: "S", 1: "S", 2: "E", 3: "E", 4: "g"}),   # the glowing eyes
    (-1, {-3: "S", -2: "S", -1: "S", 0: "N", 1: "S", 2: "S", 3: "S"}),             # nose
    (0, {-2: "S", -1: "S", 0: "N", 1: "S", 2: "S"}),
    (1, {-2: "S", -1: "M", 0: "M", 1: "M", 2: "S"}),          # the mouth
    (2, {-2: "M", -1: "S", 0: "S", 1: "S", 2: "M"}),          # its corners turn down: a frown
]
FACE_COLOURS = {"S": SKIN, "N": SHADE, "B": BROW, "E": EYE, "g": GLOW, "M": MOUTH}
FACE_X, FACE_Y = 1375, 490      # the face's centre in the original design picture


def paint_face(img, cx, cy):
    for dy, row in FACE:
        for dx, letter in row.items():
            x, y = cx + dx, cy + dy
            if 0 <= y < img.shape[0] and 0 <= x < img.shape[1] and img[y, x, 3] > 0:
                img[y, x, :3] = FACE_COLOURS[letter]


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
