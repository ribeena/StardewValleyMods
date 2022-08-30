← [author guide](../author-guide.md)

## Contents
* [Introduction](#introduction)
* [The Body](#Body)
* [Naked Upper Overlays](#naked-upper-overlays)
* [Naked Lower Overlays](#naked-lower-overlays)

## Introduction
After creating your [content JSON file](../author-guide.md#body-parts), you'll need to add
entries for each of the options.

Sometimes looking at the [examples](https://www.nexusmods.com/stardewvalley/mods/12893?tab=files#file-container-optional-files) make it easier!

## Body
The main body file only includes the torso and leg graphics of the character. These
define the body when no shirt or pants are worn, or likewise shorts etc. It doesn't
include the feet either, to create those graphics, you'll want to make
[boots](/shoes.md) that overlay onto this.

When making you picture it's good to start off by copying the
[default one from this mod](../../asset/Character/farmer_base.png). The image is the full size
288x672 but only the first 96 pixels in width will be use, as the rest is
overwritten by subsequent features (arms, and facial features).

Once a file is made, add an entry to the JSON file;

```
...
  "male": {
    ...
    "bodyStyles": {
        "Toned": "toned",
    },
...
```
Above in the `male` section, a 'toned.png' file has been added. The folder will look like;
```
📁 Mods/
   📁 [DB] YourModName/
      🗎 content.json
      🗎 manifest.json
      📁 assets/
         📁 bodies/
            🗎 toned.png
            ...
         ...
```

## Naked Upper Overlays
More to come.

## Naked Lower Overlays
More to come.