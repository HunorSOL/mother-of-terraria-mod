"""Makes the Heavens Crystal item icon from the owner's design (DesignRefs/crystal_design.jpg, the big crystal on the left).

Output: Items/HeavensCrystal.png (22x32, transparent). Terraria-style: no glow, 1 px dark outline, a small colour palette.
Run from the project folder:  python3 tools/make_crystal_icon.py      (needs ImageMagick and numpy)
"""
import subprocess
import sys

import numpy as np

SRC = "DesignRefs/crystal_design.jpg"
OUT = "Items/HeavensCrystal.png"
CROP = (800, 1100, 260, 200)      # w, h, x, y in the 2752x1536 original: the big crystal and a little margin
W, H = 22, 32
OUTLINE = (34, 28, 92)
# Palette picked from the design: shadow blue to bright white, with a warm gold-ish accent.
PALETTE = [
    (58, 78, 170), (92, 140, 224), (132, 188, 244), (176, 220, 252),
    (222, 240, 255), (255, 255, 255), (236, 220, 186), (170, 150, 214),
]


def read_rgb(args):
    raw = subprocess.run(["magick", SRC, *args, "-depth", "8", "rgb:-"], capture_output=True, check=True).stdout
    return raw


def main():
    cw, ch, cx, cy = CROP
    raw = read_rgb(["-crop", f"{cw}x{ch}+{cx}+{cy}", "+repage"])
    img = np.frombuffer(raw, dtype=np.uint8).reshape(ch, cw, 3).astype(float)

    # The crystal is bright; the outline and background are dark and the glow outside is dimmer than the facets.
    lum = img.max(axis=2)
    mask = lum > float(sys.argv[1]) if len(sys.argv) > 1 else lum > 150
    # Keep the biggest blob only (drops sparkles), then fill holes.
    from collections import deque
    lab = np.zeros(mask.shape, int)
    best, best_n, n = 0, 0, 0
    for y, x in zip(*np.nonzero(mask)):
        if lab[y, x]:
            continue
        n += 1
        q = deque([(y, x)]); lab[y, x] = n; cnt = 0
        while q:
            cy2, cx2 = q.popleft(); cnt += 1
            for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                ny, nx = cy2 + dy, cx2 + dx
                if 0 <= ny < ch and 0 <= nx < cw and mask[ny, nx] and not lab[ny, nx]:
                    lab[ny, nx] = n; q.append((ny, nx))
        if cnt > best_n:
            best, best_n = n, cnt
    mask = lab == best
    ys, xs = np.nonzero(mask)
    y0, y1, x0, x1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    crop = img[y0:y1, x0:x1]
    m = mask[y0:y1, x0:x1].astype(float)

    # Shrink to fit inside (W-2) x (H-2) so there is room for the outline, keeping the shape's proportions.
    inner_w, inner_h = W - 2, H - 2
    scale = min(inner_w / crop.shape[1], inner_h / crop.shape[0])
    tw, th = max(1, round(crop.shape[1] * scale)), max(1, round(crop.shape[0] * scale))
    rgba = np.dstack([crop, m * 255]).astype(np.uint8)
    p = subprocess.run(["magick", "-size", f"{crop.shape[1]}x{crop.shape[0]}", "-depth", "8", "rgba:-",
                        "-filter", "Box", "-resize", f"{tw}x{th}!", "-depth", "8", "rgba:-"],
                       input=rgba.tobytes(), capture_output=True, check=True).stdout
    small = np.frombuffer(p, dtype=np.uint8).reshape(th, tw, 4)

    out = np.zeros((H, W, 4), dtype=np.uint8)
    ox, oy = (W - tw) // 2, (H - th) // 2
    pal = np.array(PALETTE, dtype=float)
    body = np.zeros((H, W), bool)
    for y in range(th):
        for x in range(tw):
            if small[y, x, 3] >= 128:
                c = small[y, x, :3].astype(float)
                c = np.clip((c - 140) * 1.6 + 128, 0, 255)      # a bit more contrast, like Terraria items
                out[oy + y, ox + x] = (*pal[np.argmin(((pal - c) ** 2).sum(axis=1))].astype(np.uint8), 255)
                body[oy + y, ox + x] = True
    # 1 px outline around the body.
    for y in range(H):
        for x in range(W):
            if not body[y, x] and any(0 <= y + dy < H and 0 <= x + dx < W and body[y + dy, x + dx]
                                      for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                out[y, x] = (*OUTLINE, 255)
    subprocess.run(["magick", "-size", f"{W}x{H}", "-depth", "8", "rgba:-", OUT], input=out.tobytes(), check=True)
    print("wrote", OUT, f"({W}x{H}), crystal {tw}x{th}")


main()
