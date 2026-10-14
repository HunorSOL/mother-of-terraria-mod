"""Makes the Heavenly Basket tile art from the owner's design (DesignRefs/basket_design.jpg, the big basket in the middle).

Outputs (8x9 tiles, 16 px frames with 2 px padding = 142x160, the standard multi-tile object layout):
  Tiles/HeavenlyBasket.png            the basket, with its inner light DIMMED (the empty look)
  Tiles/HeavenlyBasket_Glow.png       only the inner light, drawn on top (additive) in proportion to how full the Basket is
  Tiles/HeavenlyBasket_Highlight.png  the hover outline (required by TileID.Sets.HasOutlines)
Also prints the bowl-opening centre in sprite pixels (used by the glow orb in Tiles/HeavensObjectTile.cs).

Run from the project folder:  python3 tools/make_basket_sprite.py [threshold]      (needs ImageMagick and numpy)
"""
import subprocess
import sys

import numpy as np

SRC = "DesignRefs/basket_design.jpg"
CROP = (940, 250, 875, 930)       # x, y, w, h in the 2752x1536 original: the middle basket with a little margin
TILES_W, TILES_H = 8, 9
W, H = TILES_W * 16, TILES_H * 16  # 128 x 144 pixels of art
WORK_W = 256                       # the cut-out is computed at this width, then shrunk with a box filter
# Opening of the bowl (where the light is), as fractions of the finished sprite, found by eye on the design:
OPEN_CX, OPEN_CY, OPEN_RX, OPEN_RY = 0.51, 0.55, 0.27, 0.07
DIM = 0.45                         # how much of its brightness the opening keeps when the Basket is empty


def run_rgb(args, w, h, channels=3):
    raw = subprocess.run(["magick", *args, "-depth", "8", "rgb:-" if channels == 3 else "rgba:-"], capture_output=True, check=True).stdout
    return np.frombuffer(raw, dtype=np.uint8).reshape(h, w, channels)


def dilate(m, r):
    out = m.copy()
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            if dx * dx + dy * dy <= r * r + 1:
                out |= np.roll(np.roll(m, dy, 0), dx, 1)
    return out


def components(mask):
    """Labels 4-connected groups of True pixels. Returns (labels, count)."""
    from collections import deque
    lab = np.zeros(mask.shape, int)
    n = 0
    for y, x in zip(*np.nonzero(mask)):
        if lab[y, x]:
            continue
        n += 1
        q = deque([(y, x)]); lab[y, x] = n
        while q:
            cy, cx = q.popleft()
            for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                ny, nx = cy + dy, cx + dx
                if 0 <= ny < mask.shape[0] and 0 <= nx < mask.shape[1] and mask[ny, nx] and not lab[ny, nx]:
                    lab[ny, nx] = n; q.append((ny, nx))
    return lab, n


def main():
    thr = float(sys.argv[1]) if len(sys.argv) > 1 else 105
    cx, cy, cw, ch = CROP
    wh = round(ch * WORK_W / cw)
    img = run_rgb([SRC, "-crop", f"{cw}x{ch}+{cx}+{cy}", "+repage", "-filter", "Box", "-resize", f"{WORK_W}x{wh}!"], WORK_W, wh).astype(float)

    bright = img.max(axis=2) > thr
    lab, n = components(dilate(bright, 2))
    sizes = np.bincount(lab.ravel())[1:]
    body = dilate(lab == (np.argmax(sizes) + 1), 0) & bright | (lab == (np.argmax(sizes) + 1)) & bright
    mask = dilate(body, 5)  # one art pixel of outline all round
    # Fill small holes only (the handle's big opening must stay see-through).
    holes, hn = components(~mask)
    for k in range(1, hn + 1):
        size = (holes == k).sum()
        touches_edge = (holes == k)[0].any() or (holes == k)[-1].any() or (holes == k)[:, 0].any() or (holes == k)[:, -1].any()
        if not touches_edge and size < 600:
            mask |= holes == k

    ys, xs = np.nonzero(mask)
    y0, y1, x0, x1 = ys.min(), ys.max() + 1, xs.min(), xs.max() + 1
    img, mask = img[y0:y1, x0:x1], mask[y0:y1, x0:x1]

    # Shrink to fit inside W x H, standing on the bottom edge and centred.
    scale = min(W / img.shape[1], H / img.shape[0])
    tw, th = round(img.shape[1] * scale), round(img.shape[0] * scale)
    rgba = np.dstack([img, mask * 255.0]).astype(np.uint8)
    small = run_rgb(["-size", f"{img.shape[1]}x{img.shape[0]}", "-depth", "8", "rgba:-", "-filter", "Box", "-resize", f"{tw}x{th}!"],
                    tw, th, 4) if False else None
    p = subprocess.run(["magick", "-size", f"{img.shape[1]}x{img.shape[0]}", "-depth", "8", "rgba:-", "-filter", "Box",
                        "-resize", f"{tw}x{th}!", "-depth", "8", "rgba:-"], input=rgba.tobytes(), capture_output=True, check=True).stdout
    small = np.frombuffer(p, dtype=np.uint8).reshape(th, tw, 4)

    art = np.zeros((H, W, 4), dtype=np.uint8)
    ox, oy = (W - tw) // 2, H - th
    art[oy:oy + th, ox:ox + tw] = small
    art[..., 3] = np.where(art[..., 3] >= 128, 255, 0)
    art[art[..., 3] == 0] = 0

    # The light in the bowl's opening: a soft ellipse. Dim it in the base art, keep it for the glow overlay.
    ccx, ccy = ox + OPEN_CX * tw, oy + OPEN_CY * th
    yy, xx = np.mgrid[0:H, 0:W]
    d = np.sqrt(((xx + .5 - ccx) / (OPEN_RX * tw)) ** 2 + ((yy + .5 - ccy) / (OPEN_RY * th)) ** 2)
    weight = np.clip(1.15 - d, 0, 1) * (art[..., 3] > 0)
    glow = np.zeros((H, W, 4), dtype=np.uint8)
    glow[..., 0] = (190 * weight).astype(np.uint8)
    glow[..., 1] = (235 * weight).astype(np.uint8)
    glow[..., 2] = (255 * weight).astype(np.uint8)
    glow[..., 3] = (255 * weight).astype(np.uint8)
    base = art.copy()
    dim = 1 - (1 - DIM) * weight
    base[..., :3] = (art[..., :3] * dim[..., None]).astype(np.uint8)

    # Hover outline: the outermost pixels of the sprite in bright gold.
    a = art[..., 3] > 0
    hl = np.zeros((H, W, 4), dtype=np.uint8)
    for y in range(H):
        for x in range(W):
            if a[y, x] and any(not (0 <= x + dx < W and 0 <= y + dy < H and a[y + dy, x + dx]) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                hl[y, x] = (255, 240, 120, 255)

    for name, px in (("HeavenlyBasket", base), ("HeavenlyBasket_Glow", glow), ("HeavenlyBasket_Highlight", hl)):
        save_padded(px, f"Tiles/{name}.png")
    print(f"opening centre in sprite pixels: x={ccx:.0f} y={ccy:.0f} (sprite {W}x{H})")
    # a flat preview (not used by the game)
    subprocess.run(["magick", "-size", f"{W}x{H}", "-depth", "8", "rgba:-", "/tmp/basket_flat.png"], input=base.tobytes(), check=True)


def save_padded(px, path):
    """Writes the padded object sheet: tile (tx, ty) goes to pixel (tx*18, ty*18)."""
    SW, SH = TILES_W * 18 - 2, TILES_H * 18 - 2
    sheet = np.zeros((SH, SW, 4), dtype=np.uint8)
    for ty in range(TILES_H):
        for tx in range(TILES_W):
            sheet[ty * 18:ty * 18 + 16, tx * 18:tx * 18 + 16] = px[ty * 16:ty * 16 + 16, tx * 16:tx * 16 + 16]
    subprocess.run(["magick", "-size", f"{SW}x{SH}", "-depth", "8", "rgba:-", path], input=sheet.tobytes(), check=True)
    print(f"wrote {path}: {SW}x{SH}")


main()
