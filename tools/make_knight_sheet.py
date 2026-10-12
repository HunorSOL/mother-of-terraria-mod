"""Builds the Heavenly Knight animation sheet from the single knight picture.

Input : DesignRefs/knight_base.png   (65x82, the owner's design cut out and scaled)
Output: NPCs/Heavens/HeavenlyKnight.png  (65 wide, 7 frames stacked vertically, 82 tall each)

Frame 0 is the standing pose. Frames 1-6 are a walk cycle made by gently bending the picture
(a smooth "warp", so there are no seams): the legs step in turn, the body bobs, the feet stay planted,
the cape sways and the wings flap. This is a stand-in for hand-drawn animation: if the owner supplies
real animation frames, just replace the output PNG (keep 7 frames of the same size, or update
Main.npcFrameCount and the frame logic in HeavenlyKnight.cs).

Run from the project folder:  python3 tools/make_knight_sheet.py
Needs ImageMagick (the `magick` command) and numpy.
"""
import math
import subprocess

import numpy as np

SRC = "DesignRefs/knight_base.png"
OUT = "NPCs/Heavens/HeavenlyKnight.png"
FRAMES = 7
W, H = 65, 82


def read_rgba(path):
    w, h = map(int, subprocess.check_output(["magick", "identify", "-format", "%w %h", path]).split())
    raw = subprocess.check_output(["magick", path, "-depth", "8", "rgba:-"])
    return np.frombuffer(raw, dtype=np.uint8).reshape(h, w, 4).copy()


def write_rgba(arr, path):
    h, w, _ = arr.shape
    proc = subprocess.Popen(["magick", "-size", f"{w}x{h}", "-depth", "8", "rgba:-", path], stdin=subprocess.PIPE)
    proc.communicate(arr.tobytes())
    assert proc.returncode == 0


def clip01(a):
    return np.clip(a, 0.0, 1.0)


def displacement(phase):
    """How far each pixel of the standing picture moves (dx, dy) for a given point in the walk cycle.
    Every part has a 'weight' that is 1 where it should move fully and fades to 0 where it joins the rest,
    so the picture bends smoothly instead of tearing."""
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    dx = np.zeros((H, W))
    dy = np.zeros((H, W))
    if phase is None:
        return dx, dy

    # Feet: the legs step sideways and lift in turn. Weight grows toward the feet (hips stay put).
    foot = clip01((yy - 62) / 10)
    left_leg = clip01((xx - 18) / 4) * clip01((40 - xx) / 4)
    right_leg = clip01((xx - 41) / 4) * clip01((59 - xx) / 3)
    step_l, step_r = 3 * math.sin(phase), 3 * math.sin(phase + math.pi)
    lift_l = 2 * max(0.0, math.cos(phase))
    lift_r = 2 * max(0.0, math.cos(phase + math.pi))
    dx += foot * (step_l * left_leg + step_r * right_leg)
    dy -= foot * (lift_l * left_leg + lift_r * right_leg)

    # Body bob: the upper body rises a pixel on every other step; the feet stay planted on the ground.
    bob = -1.0 if math.sin(2 * phase + 0.6) > 0 else 0.0
    dy += bob * clip01((76 - yy) / 14)

    # Cape: the outer edges sway, more toward the hem.
    sway = 2.5 * math.sin(phase + 1.2)
    cape_l = clip01((24 - xx) / 12) * clip01((yy - 44) / 26)
    cape_r = clip01((xx - 54) / 8) * clip01((yy - 50) / 16)
    dx += sway * cape_l - sway * cape_r

    # Wings flap up and down together; the weight fades out toward the helmet and the shoulders.
    flap = 2 * math.sin(phase)
    wing_l = clip01((27 - xx) / 8) * clip01((40 - yy) / 14)
    wing_r = clip01((xx - 56) / 6) * clip01((38 - yy) / 14)
    dy -= flap * (wing_l + wing_r)
    return dx, dy


def make_frame(base, frame):
    phase = None if frame == 0 else 2 * math.pi * (frame - 1) / 6 + math.pi / 6
    dx, dy = displacement(phase)
    yy, xx = np.mgrid[0:H, 0:W]
    # For each output pixel, look up where it came from in the standing picture.
    src_x = np.rint(xx - dx).astype(int)
    src_y = np.rint(yy - dy).astype(int)
    inside = (src_x >= 0) & (src_x < W) & (src_y >= 0) & (src_y < H)
    out = np.zeros_like(base)
    out[inside] = base[src_y[inside], src_x[inside]]
    return out


def main():
    base = read_rgba(SRC)
    assert base.shape[:2] == (H, W), f"expected {W}x{H}, got {base.shape}"
    sheet = np.concatenate([make_frame(base, f) for f in range(FRAMES)], axis=0)
    write_rgba(sheet, OUT)
    print(f"wrote {OUT}: {W}x{H * FRAMES} ({FRAMES} frames)")


if __name__ == "__main__":
    main()
