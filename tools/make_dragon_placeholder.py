"""Generates the PLACEHOLDER sprite sheet for the Heavens Dragon (the owner has no design yet).

Output: NPCs/Heavens/HeavensDragon.png  (64 wide, 4 frames stacked vertically, 44 tall each)
Frames: 0 = flying, wings up, 1 = flying, wings down, 2 = wind-up (mouth glowing), 3 = dash (stretched, wings swept back).
Legs are always tucked: it is a flier.
The dragon faces RIGHT. Replace the PNG with the owner's real frames later (keep the frame count or update
Main.npcFrameCount and FindFrame in HeavensDragon.cs).

Run from the project folder:  python3 tools/make_dragon_placeholder.py     (needs ImageMagick and numpy)
"""
import subprocess

import numpy as np

W, H, FRAMES = 64, 44, 4
OUT = "NPCs/Heavens/HeavensDragon.png"

CLEAR = (0, 0, 0, 0)
RED = (196, 52, 36, 255)
DRED = (140, 30, 28, 255)
LRED = (236, 96, 60, 255)
GOLD = (240, 196, 70, 255)
LGOLD = (255, 232, 140, 255)
BELLY = (250, 214, 120, 255)
EYE = (240, 248, 255, 255)
GLOW = (255, 150, 40, 255)
OUT_C = (46, 18, 14, 255)


def inside(poly, x, y):
    n, hit, j = len(poly), False, len(poly) - 1
    for i in range(n):
        xi, yi = poly[i]
        xj, yj = poly[j]
        if (yi > y) != (yj > y) and x < (xj - xi) * (y - yi) / (yj - yi + 1e-9) + xi:
            hit = not hit
        j = i
    return hit


def fill(img, poly, color):
    for y in range(H):
        for x in range(W):
            if inside(poly, x + 0.5, y + 0.5):
                img[y][x] = color


def rect(img, x0, y0, x1, y1, color):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if 0 <= x < W and 0 <= y < H:
                img[y][x] = color


def outline(img):
    add = []
    for y in range(H):
        for x in range(W):
            if img[y][x][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < W and 0 <= ny < H and img[ny][nx][3] and img[ny][nx] != OUT_C:
                        add.append((x, y))
                        break
    for x, y in add:
        img[y][x] = OUT_C


def frame(kind):
    img = [[CLEAR] * W for _ in range(H)]
    drop = 3 if kind == 2 else 0            # crouch for the wind-up
    stretch = 4 if kind == 3 else 0         # lean forward in the dash
    # tail (left), body, neck/head (right)
    fill(img, [(2, 24 + drop), (20, 20 + drop), (22, 30 + drop), (4, 27 + drop)], DRED)
    fill(img, [(18, 18 + drop), (44 + stretch, 17 + drop), (46 + stretch, 31 + drop), (20, 33 + drop)], RED)
    fill(img, [(22, 29 + drop), (44 + stretch, 29 + drop), (45 + stretch, 33 + drop), (22, 34 + drop)], BELLY)
    head_y = 8 + drop + (-3 if kind == 2 else 0) + (4 if kind == 3 else 0)
    fill(img, [(42 + stretch, 18 + drop), (50 + stretch, head_y + 2), (62, head_y + 5), (62, head_y + 11), (48 + stretch, head_y + 14), (44 + stretch, 26 + drop)], LRED)
    fill(img, [(48 + stretch, head_y + 2), (51 + stretch, head_y - 3), (53 + stretch, head_y + 2)], GOLD)  # horn
    rect(img, 52 + stretch, head_y + 5, 54 + stretch, head_y + 6, EYE)
    if kind == 2:  # mouth glow = the tell
        rect(img, 56, head_y + 8, 61, head_y + 10, GLOW)
        rect(img, 58, head_y + 9, 60, head_y + 9, LGOLD)
    # wings: big gold shapes. Up on frame 0, down on frame 1, raised for the wind-up, swept back in the dash.
    if kind == 3:
        fill(img, [(20, 18), (4, 12), (12, 20), (2, 22), (16, 25)], GOLD)
        fill(img, [(18, 19), (8, 15), (12, 21), (6, 22)], LGOLD)
    elif kind == 1:   # wings down
        fill(img, [(24, 22 + drop), (20, 34), (32, 38), (44, 24 + drop)], GOLD)
        fill(img, [(26, 24 + drop), (24, 33), (31, 35), (40, 25 + drop)], LGOLD)
    else:             # wings up (frames 0 and 2)
        fill(img, [(24, 18 + drop), (22, 2), (34, 0), (44, 17 + drop)], GOLD)
        fill(img, [(27, 16 + drop), (26, 5), (33, 4), (40, 16 + drop)], LGOLD)
    # legs: always tucked up against the belly
    rect(img, 26, 32 + drop, 29, 35 + drop, DRED)
    rect(img, 37, 32 + drop, 40, 35 + drop, DRED)
    outline(img)
    return np.array(img, dtype=np.uint8)


def main():
    sheet = np.concatenate([frame(k) for k in (0, 1, 2, 3)], axis=0)
    h, w, _ = sheet.shape
    proc = subprocess.Popen(["magick", "-size", f"{w}x{h}", "-depth", "8", "rgba:-", OUT], stdin=subprocess.PIPE)
    proc.communicate(sheet.tobytes())
    print(f"wrote {OUT}: {w}x{h}")


if __name__ == "__main__":
    main()
