#!/usr/bin/env python3
"""Makes the PLACEHOLDER sprite for the Mother of Terraria (NPCs/Bosses/MotherOfTerrariaBoss.png, 160x160).
A golden robed figure with two pale-blue wings and a blue gem. Replace with the owner's real design later.
Needs ImageMagick. Run from the project folder: python3 tools/make_boss_placeholder.py"""
import subprocess

out = "NPCs/Bosses/MotherOfTerrariaBoss.png"
cmd = [
    "convert", "-size", "160x160", "xc:none",
    "-stroke", "#7a5a10", "-strokewidth", "2",
    "-fill", "#bfe4ff", "-draw", "polygon 80,60 4,10 14,70 30,110 80,100",
    "-fill", "#bfe4ff", "-draw", "polygon 80,60 156,10 146,70 130,110 80,100",
    "-fill", "#f2c230", "-draw", "polygon 80,50 112,150 48,150",
    "-fill", "#ffe27a", "-draw", "circle 80,40 80,62",
    "-fill", "#3fa0ff", "-stroke", "#1b4f8a", "-draw", "polygon 80,78 88,92 80,106 72,92",
    out,
]
subprocess.run(cmd, check=True)
print("wrote", out)
