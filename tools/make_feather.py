#!/usr/bin/env python3
"""Makes the PLACEHOLDER feather sprite for the Mother of Terraria's Feather Storm (Projectiles/GoldenFeather.png, 16x44).
A golden quill with pale-gold vanes and a blue tip, pointing UP (the projectile code turns it to face its travel direction).
Not from the owner's design (the design sheet has no feather): replace with real art later, same size or update
GoldenFeather.cs. Needs ImageMagick. Run from the project folder: python3 tools/make_feather.py"""
import subprocess

subprocess.check_call([
    "magick", "-size", "16x44", "xc:none", "-filter", "point",
    "-stroke", "#8a6410", "-strokewidth", "1",
    "-fill", "#ffe9a0", "-draw", "polygon 8,1 14,16 13,34 8,43 3,34 2,16",
    "-stroke", "none", "-fill", "#7fc8ff", "-draw", "polygon 8,1 11,10 8,14 5,10",
    "-fill", "#f2c230", "-draw", "rectangle 7,6 8,43",
    "-fill", "#fff6d0", "-draw", "polygon 4,18 7,16 7,30 4,28 ",
    "-colors", "12", "+dither", "Projectiles/GoldenFeather.png"])
print("wrote Projectiles/GoldenFeather.png")
