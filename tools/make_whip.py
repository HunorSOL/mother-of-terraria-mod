"""Makes the whip sprite for the Mother of Terraria's Heavenly Whip attack (NPCs/Bosses/HeavenlyWhip.png) from the owner's design.

Input : DesignRefs/whip design.jpg  (2752x1536, 2026-10-26: the held whip, the big "Extended / Attack Swing" whip and two impact bursts)
Output: NPCs/Bosses/HeavenlyWhip.png  (the extended whip alone, with its star tip, no glow; shrunk with a box filter to HEIGHT px
        and a small palette for the Terraria look)
Found by colour like make_feather.py (gold, blue, white and the dark navy outline); the sparkles are separate small pieces and are left out.
Run from the project folder:  python3 tools/make_whip.py      (needs ImageMagick and numpy)
"""
import subprocess

import numpy as np

SRC = "DesignRefs/whip design.jpg"
OUT = "NPCs/Bosses/HeavenlyWhip.png"
HEIGHT = 130         # final height in pixels
COLOURS = 48
CROP = "1500x1350+700+100"      # the extended whip's area in the original picture
WORK_W, WORK_H = 469, 422       # the cut-out is found at this reduced size (fast), 1500x1350 / 3.2


def components(mask):
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


def dilate(mask, r):
    out = mask.copy()
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            if dx * dx + dy * dy <= r * r:
                out |= np.roll(np.roll(mask, dy, axis=0), dx, axis=1)
    return out


raw = subprocess.check_output(["magick", SRC, "-crop", CROP, "+repage", "-filter", "Box", "-resize", f"{WORK_W}x{WORK_H}!", "-depth", "8", "rgb:-"])
a = np.frombuffer(raw, dtype=np.uint8).reshape(WORK_H, WORK_W, 3).astype(int)
r, g, b = a[..., 0], a[..., 1], a[..., 2]

gold = (r > 150) & (r - b > 60)
blue = (b - g > 40) & (b > 90) & (b - r > 40)
white = (r > 215) & (g > 215) & (b > 190) & (b < 250)
navy = (b > 50) & (r < 45) & (g < 70) & (b - g > 20)

# The feather itself = the biggest connected piece of gold/blue/white (the sparkles are separate small pieces).
labels, n = components(gold | blue | white)
sizes = np.bincount(labels.ravel(), minlength=n + 1)
sizes[0] = 0
core = labels == sizes.argmax()
core = dilate(core, 2)                       # close the tiny gaps inside the quill and between the colour bands
# Fill holes: anything not reachable from the picture's border without crossing the core is inside the feather.
outside = np.zeros_like(core)
stack = [(y, x) for y in range(WORK_H) for x in (0, WORK_W - 1)] + [(y, x) for x in range(WORK_W) for y in (0, WORK_H - 1)]
while stack:
    y, x = stack.pop()
    if 0 <= y < WORK_H and 0 <= x < WORK_W and not core[y, x] and not outside[y, x]:
        outside[y, x] = True
        stack += [(y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)]
body = ~outside
# The dark navy outline lies just outside the coloured part: take navy pixels within 3 px of the body.
outline = navy & dilate(body, 3)
mask = body | outline

rgba = np.zeros((WORK_H, WORK_W, 4), dtype=np.uint8)
rgba[..., :3] = a.astype(np.uint8)
rgba[..., 3] = np.where(mask, 255, 0)
p = subprocess.Popen(["magick", "-size", f"{WORK_W}x{WORK_H}", "-depth", "8", "rgba:-", "/tmp/whip_cut.png"], stdin=subprocess.PIPE)
p.communicate(rgba.tobytes())

subprocess.check_call([
    "magick", "/tmp/whip_cut.png", "-background", "none", "-trim", "+repage",
    "-filter", "Box", "-resize", f"x{HEIGHT}", "-channel", "A", "-threshold", "50%", "+channel",
    "-colors", str(COLOURS), "+dither", "-trim", "+repage", OUT])
print(subprocess.check_output(["magick", "identify", OUT]).decode())
