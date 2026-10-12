"""Builds the Heavens Dragon animation sheet from the owner's design.

Input : DesignRefs/dragon_base.png   (the design cut out and scaled to 202 px wide, facing RIGHT)
Output: NPCs/Heavens/HeavensDragon.png  (8 frames stacked vertically)

The body ripples like a swimming serpent (a wave runs from the head to the tail) and the big wings flap.
It is made by gently bending the single picture (a smooth "warp", so there are no seams), as a stand-in for
hand-drawn animation. If the owner supplies real frames, replace the PNG (same frame size, 8 frames, or update
Main.npcFrameCount and the constants in HeavensDragon.cs).

Run from the project folder:  python3 tools/make_dragon_sheet.py      (needs ImageMagick and numpy)
Prints the head position, which HeavensDragon.cs uses to line the hitbox up with the head.
"""
import math
import subprocess

import numpy as np

SRC = "DesignRefs/dragon_base.png"
OUT = "NPCs/Heavens/HeavensDragon.png"
FRAMES = 8
PAD = 6  # empty rows above and below, so the flapping wings and the wave never get cut off


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


def make_frame(base, frame):
    bh, bw, _ = base.shape
    H = bh + 2 * PAD
    canvas = np.zeros((H, bw, 4), dtype=np.uint8)
    canvas[PAD:PAD + bh] = base
    phase = 2 * math.pi * frame / FRAMES

    yy, xx = np.mgrid[0:H, 0:bw].astype(float)
    dist = (bw - 1) - xx                                   # distance from the head (head is on the right)
    amp = 2.5 * clip01((dist - 20) / 110)                  # the wave grows toward the tail
    dy = amp * np.sin(phase - 0.07 * dist)

    wing_x = clip01(1 - np.abs(xx - 0.62 * bw) / (0.13 * bw))
    wing_y = clip01((0.72 * bh + PAD - yy) / (0.45 * bh))  # strongest at the wing tips, zero at the wing root
    dy -= 5.0 * math.sin(phase) * wing_x * wing_y

    src_x = np.rint(xx).astype(int)
    src_y = np.rint(yy - dy).astype(int)
    inside = (src_y >= 0) & (src_y < H)
    out = np.zeros_like(canvas)
    out[inside] = canvas[src_y[inside], src_x[inside]]
    return out


def main():
    base = read_rgba(SRC)
    bh, bw, _ = base.shape
    frames = [make_frame(base, f) for f in range(FRAMES)]
    write_rgba(np.concatenate(frames, axis=0), OUT)
    fh = bh + 2 * PAD
    cols = np.nonzero((base[..., 3] > 127).any(axis=0))[0]
    right = cols.max()
    ys = np.nonzero((base[:, right - 25:right + 1, 3] > 127).any(axis=1))[0]
    print(f"wrote {OUT}: {bw}x{fh * FRAMES} ({FRAMES} frames of {bw}x{fh})")
    print(f"nose tip x={right}, head rows y={ys.min() + PAD}..{ys.max() + PAD} (frame coordinates)")


if __name__ == "__main__":
    main()
