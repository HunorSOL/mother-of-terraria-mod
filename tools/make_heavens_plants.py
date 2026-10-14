"""Generates the PLACEHOLDER sprites for the Heavens' flowers and hanging vines (no owner design yet).

Outputs (16 px tiles, 2 px padding): Tiles/HeavenlyFlowerBlue.png (1x1), HeavenlyFlowerGold.png (1x2),
HeavenlyFlowerPink.png (1x1), HeavenlyVineShort.png (1x2), HeavenlyVineLong.png (1x4).
Replace any PNG with the owner's art of the same size (no code changes).

Run from the project folder:  python3 tools/make_heavens_plants.py      (needs ImageMagick and numpy)
"""
from make_heavens_decor import Canvas

STEM = (118, 190, 120, 255)
STEM_LIGHT = (176, 226, 150, 255)
STEM_DARK = (62, 128, 96, 255)
OUT = (40, 84, 70, 255)
BLUE = (140, 196, 255, 255)
BLUE_LIGHT = (230, 244, 255, 255)
BLUE_DARK = (82, 130, 214, 255)
GOLD = (255, 214, 90, 255)
GOLD_LIGHT = (255, 244, 176, 255)
GOLD_DARK = (214, 156, 40, 255)
PINK = (255, 178, 214, 255)
PINK_LIGHT = (255, 228, 240, 255)
PINK_DARK = (214, 112, 170, 255)
GLOW_BUD = (170, 230, 255, 255)


def finish(c, name):
    # outline in dark green-teal instead of the default so plants do not look like metal
    import make_heavens_decor as d
    d.OUT = OUT
    c.outline()
    c.save(f"Tiles/{name}.png")


def flower_blue():
    c = Canvas(1, 1)
    c.rect(8, 8, 8, 15, STEM); c.set(7, 12, STEM_LIGHT); c.set(6, 11, STEM_LIGHT); c.set(9, 13, STEM_DARK); c.set(10, 12, STEM_DARK)
    c.disc(8, 5, 4, BLUE); c.disc(8, 5, 2, BLUE_LIGHT); c.set(8, 5, GOLD_LIGHT)
    c.set(4, 5, BLUE_DARK); c.set(11, 6, BLUE_DARK); c.set(8, 9, BLUE_DARK)
    finish(c, "HeavenlyFlowerBlue")


def flower_gold():
    c = Canvas(1, 2)
    c.rect(8, 12, 8, 31, STEM); c.rect(9, 18, 9, 31, STEM_DARK)
    for (x, y) in ((6, 26), (5, 25), (4, 24), (10, 22), (11, 21), (12, 20)):
        c.set(x, y, STEM_LIGHT)
    for dx, dy in ((0, -4), (4, 0), (-4, 0), (0, 4), (3, 3), (-3, 3), (3, -3), (-3, -3)):
        c.disc(8 + dx * 0.9, 8 + dy * 0.9, 2.3, GOLD)
    c.disc(8, 8, 2.6, GOLD_DARK); c.disc(8, 8, 1.6, GOLD_LIGHT)
    finish(c, "HeavenlyFlowerGold")


def flower_pink():
    c = Canvas(1, 1)
    for x in (4, 8, 12):
        c.rect(x, 9 if x == 8 else 11, x, 15, STEM)
    for (x, y, r) in ((4, 8, 2.6), (8, 5, 3.2), (12, 8, 2.6)):
        c.disc(x, y, r, PINK); c.set(x, y, PINK_LIGHT); c.set(x + 1, y + 1, PINK_DARK)
    finish(c, "HeavenlyFlowerPink")


def vine(tiles, name):
    c = Canvas(1, tiles)
    h = tiles * 16
    x = 8
    for y in range(0, h - 4):
        if y % 7 == 3:
            x += 1 if (y // 7) % 2 == 0 else -1
        c.set(x, y, STEM); c.set(x + 1, y, STEM_DARK)
        if y % 9 == 5:
            c.set(x - 1, y, STEM_LIGHT); c.set(x - 2, y + 1, STEM_LIGHT)
        if y % 11 == 8:
            c.set(x + 2, y, STEM_LIGHT); c.set(x + 3, y + 1, STEM_LIGHT)
    c.disc(x + 0.5, h - 4, 2.6, BLUE_DARK); c.disc(x + 0.5, h - 4, 1.6, GLOW_BUD)
    finish(c, name)


if __name__ == "__main__":
    flower_blue(); flower_gold(); flower_pink()
    vine(2, "HeavenlyVineShort"); vine(4, "HeavenlyVineLong")
