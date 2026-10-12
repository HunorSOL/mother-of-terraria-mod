"""Generates the PLACEHOLDER sprite sheet for the Heavens Dragon (the owner has no design yet).

Output: NPCs/Heavens/HeavensDragon.png  (64 wide, 4 frames stacked vertically, 44 tall each)
Frames: 0 = walk A, 1 = walk B, 2 = wind-up (crouched, mouth glowing), 3 = dash (stretched, legs tucked).
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
    # wings: folded gold shapes on the back; swept back in the dash
    if kind == 3:
        fill(img, [(20, 18), (6, 14), (12, 20), (4, 22), (16, 24)], GOLD)
    else:
        lift = 3 if kind == 1 else 0
        fill(img, [(24, 18 + drop), (28, 4 + drop - lift), (36, 7 + drop - lift), (42, 17 + drop)], GOLD)
        fill(img, [(28, 16 + drop), (31, 8 + drop - lift), (36, 10 + drop - lift), (38, 16 + drop)], LGOLD)
    # legs
    if kind == 3:      # tucked
        rect(img, 24, 32, 28, 36, DRED)
        rect(img, 38, 32, 42, 36, DRED)
    else:
        a = 2 if kind == 1 else 0
        rect(img, 24 - a, 33 + drop, 28 - a, 42, DRED); rect(img, 22 - a, 41, 29 - a, 43, OUT_C)
        rect(img, 38 + a, 33 + drop, 42 + a, 42, DRED); rect(img, 36 + a, 41, 43 + a, 43, OUT_C)
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
