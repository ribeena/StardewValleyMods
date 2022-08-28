﻿← [author guide](../author-guide.md)

A patch with **`"Action": "EditData"`** edits fields and entries inside a data asset. Any number of
content packs can edit the same asset.

## Contents
* [Introduction](#introduction)
* [Icons changing colors](#icons-changing-colors)
* [Adding custom boots with JSONAssets](#adding-custom-boots-with-jsonassets)

## Introduction
After creating your [Boots folder and JSON file](../author-guide.md#shoes), that's really all you need to
do for creating custom display boots.

## Icons changing colors
The vanilla game didn't do a very good job of showing what your boots will look like on
your character - so this mod standardises all the icons into 5 colors based on the
Farmer sprite 3 colors. Refer to the [boots sprite](../../assets/Interface/springobjects_boots.png) of
this mod to see the 5 brown colors.

This means that when using the boots stainer/tailoring screens the mod generates a new
icon with the correct colors. The colors come straight from the shoes palette, and it 
calculates the additional 2 colors.

## Adding custom boots with JSONAssets
If you are adding new boots with [JSONAssets](https://www.nexusmods.com/stardewvalley/mods/1720)
there's not much you need to do - the name will fuzzy match like above, and the
new color palette you add will work automatically with the stainer.

Refer to the [boots sprite](../../assets/Interface/springobjects_boots.png) of
this mod and create your icon using those 5 colors and it should work fine.

### WHat about JSONAssets successor Dynamic Game Assets
This feature doesn't currently work with [Dynamic Game Assets](https://www.nexusmods.com/stardewvalley/mods/9365)
because Dynamic Bodies is unable to access the boots icon that is shown.