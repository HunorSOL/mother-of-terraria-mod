Shape templates for tModLoader tile sheets.

These three images come from the official tModLoader ExampleMod
(https://github.com/tModLoader/tModLoader, ExampleMod/Content/Tiles). tModLoader is released under the MIT license.
They define WHERE the pieces of a standard block, a tree trunk and tree branches sit in their sheets; the scripts in
tools/ keep those shapes and repaint them with the Heavens' own colours and art:
  block_sheet_template.png        ->  Tiles/HeavensTurf.png              (tools/make_turf_sheet.py)
  tree_trunk_template.png         ->  Tiles/Trees/HeavenlyTree.png       (tools/make_tree_textures.py)
  tree_branches_template.png      ->  Tiles/Trees/HeavenlyTree_Branches.png (tools/make_tree_textures.py)
This folder is not part of the built mod (build.txt ignores tools\*).
