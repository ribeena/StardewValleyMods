← [README](README.md)

This document helps mod authors create a content pack for Dynamic Bodies.

**See the [main README](README.md) for other info**.

## Contents
* [Introduction](#introduction)
  * [What is, and why, Dynamic Bodies?](#what-is-and-why-dynamic-bodies)
  * [What does a content pack look like?](#what-does-a-content-pack-look-like)
* [Get started](#get-started)
* [Features](#features)
  * [Body Parts](#body-parts)
  * [Hair](#hair)
  * [Shoes](#shoes)
  * [Shirt Overlays](#shirt-overlays)
* [Working with other mods](#working-with-other-mods)

## Introduction
### What is, and why, Dynamic Bodies?
Dynamic Bodies changes the way the Farmers are rendered, similar to [Fashion Sense](https://www.nexusmods.com/stardewvalley/mods/9969),
it breaks components like eyes, ears, arms, feet and bodies up into customisable parts which can be
changed by individual players and show differently for all. This mod aims to fit in with SDV so uses the cost
and friendship requirement for changing looks, and adds other similar mechanics. It's aimed to be simpler
for mod creators making vanilla-like changes, e.g. to make new arms is a few lines of json and then making the images.

In order to access Dynamic Bodies options, there are easier to use User Interfaces, with simple tools
like premade colors. As it uses a shader to render color-swapping, it allows for many extra
color swaps like two different colored eyes, 2 sets of skin tones, 5 skin tone colors, etc. Using a similar method, 
it renders hair using two colors to better match the NPCs of the game. 

There's plenty of options, [check the mod page for more](https://www.nexusmods.com/stardewvalley/mods/12893),
but why not just use Fashion Sense? I actually started trying out Fashion Sense by making some some shoes
and struggled, matching the animation frame timings to the character, it started to be easier to reanimate
the whole legs, but then that caused other problems - so this mod aimed to make some simpler adustments easier.
Fashion Sense is quite different - highly customisable visual mod, not aimed at following the 'rules of SDV',
so great if you want to make your game look more like an Anime, turn your character into an animal etc that's what you might want!
In these ways, Fashion Sense is a sister mod - same family but with different name and interests, but alas
these sisters don't work together!

### What does a content pack look like?
A content pack is a folder with a few text files and images in it. You need a`manifest.json`
(which has info like your mod name, and Dynamic Bodies as a dependency) and files decribing
rules and locations of images, such as `content.json`, `hair.json`, `shirts.json` and `boots.json`. A
folder with customisation options for all posibilities will generally look like:
```
📁 Mods/
   📁 [DB] YourModName/
      🗎 content.json
      🗎 manifest.json
      📁 assets/
         📁 arms/
            🗎 toned_Long.png
            🗎 toned_Normal.png
            🗎 toned_Short.png
            🗎 toned_Sleeveless.png
            ...
         📁 bodies/
            🗎 toned.png
            ...
         📁 ears/
            🗎 f_pokey.png
            🗎 pokey.png
            ...
         📁 faces/
            🗎 cute.png
            ...
         📁 nakedLower/
            🗎 bikini.png
            ...
         📁 nakedUpper/
            🗎 bikinibra.png
            ...
         📁 nose/
            🗎 big.png
            ...
      📁 Boots/
         🗎 boots.json
         🗎 sneakers.png
         ...
      📁 Hair/
         🗎 hair.json
         🗎 Short Messy Curls.png
         ...
      📁 Shirts/
         🗎 shirts.json
         🗎 hangingoveralls.png
         ...
```

Have a look at the existing examples on the mod page. This guide goes into more detail below.

## Get started
To start a new content pack for Dynamic Bodies;

1. Install [SMAPI](https://www.nexusmods.com/stardewvalley/mods/2400) and [Dynamic Bodies](https://www.nexusmods.com/stardewvalley/mods/12893).
2. Create an empty folder in your `Mods` folder, and name it `[DB] YourModName`. Replace
   `YourModName` with a unique name for your mod.
3. Create a `manifest.json` file with this content:
   ```js
   {
       "Name": "Your Mod Name",
       "Author": "Your Name",
       "Version": "1.0.0",
       "Description": "One or two sentences about the mod.",
       "UniqueID": "YourName.DB.YourModName",
       "UpdateKeys": [],
       "ContentPackFor": {
           "UniqueID": "ribeena.dynamicbodies"
       }
   }
   ```
4. Change the `Name`, `Author`, `Description`, and `UniqueID` values to describe your mod. (Don't
   change the `UniqueID` under `ContentPackFor`!)

That's it! You now have a pack, though it doesn't do anything yet - you need to add content and
the relevant JSON files needed.

## Features
You don't need to know or use all of these features shown below to make a content pack. However,
you may want to open your `DynamicBodies\config.json` file and change it to show debug messages;
```
{
  "freecustomisation": false,
  "debugmsgs": true
}
```

### Body Parts
This features uses the main `content.json` file to define what files are present in the
assets folder, and what part of the body they change.

A sample JSON file looks like;
```
{
  "unisex": {
    "nakedLowers": {
      "White Breifs": {
        "name": "tighty",
        "options": ["no skin", "below accessories"]
      }
    },
    "nakedUppers": {
      "White Bra": {
        "name": "bra",
        "options": ["no skin"]
      },
      "Striped Bikini": {
        "name": "striped bikini",
        "options": ["no animation"]
      }
    }
  },

  "male": {
    "arms": {
      "Toned Arms":"Toned"
    },
    "bodyStyles": {
      "Toned": "toned",
    },
    "faces": {
      "Femme":"femme",
    },
    "ears": {
      "Pokey":"pokey",
    },
    "nose": {
      "Big": "big",
      "No nose": "noseless",
    }
  },

  "female": {
    "faces": {
      "Homme":"homme",
    },
    "ears": {
      "Pokey":"pokey",
    },
    "nose": {
      "Big": "f_big",
      "No nose": "f_noseless",
    }
  }

}
```

The JSON is split into 3 parts, Unisex, Male and Female. This is because some
graphics will show fine across either gender height, while others (on the face generally)
won't work without custom graphics.

In each of the gender profiles, there are options for you to create a body part, some
can be Unisex or tied to a gender if you choose, others must be either for male/female height.

<!-- See the [`Body Parts` documentation](author-guide/body-parts.md) for more info. -->
More to come.

### Hair
You do not need to do anything special to get dual colors etc running with hair. This
feature allows you to create some basic animation to the hair, whether its an override
of a vanilla hairstyle, or a completely new one.

Under the `[DB] YourModName\Hair` folder create a `hair.json` file which may look like;
```
﻿{
  "hairStyles": {
    "6": {
      "usesUniqueLeftSprite": true,
      "isBaldStyle": false,
      "anim_frames": {
        "Walk": { 0: 0, 1:1, 2:0, 3:2 },
        "Run": { 0: 0, 1:0, 2:1, 3:1, 4:0, 5:0, 6:2, 7:2 },
        "Ride": { 0:1, 1:0, 2:2, 3:2, 4:0, 5:1 },
      }
    },
    "Short Messy Curls": {
      "usesUniqueLeftSprite": false,
      "isBaldStyle": false,
      "anim_frames": {
        "Walk": { 0: 0, 1:1, 2:0, 3:1 },
        "Run": { 0: 0, 1:0, 2:1, 3:1, 4:0, 5:0, 6:1, 7:1 },
        "Ride": { 0: 1, 1:1, 2:0, 3:0, 4:1, 5:0 },
      }
    },
  }
}
```
Under `hairStyles` is the name of the files for the hair. If using a number, Dynamic Bodies
will check if it matches an exisiting Vanilla hairstyle and replace that. Similar to making
a hair mod for Content Patcher, you can define whether you have 4 images (`"usesUniqueLeftSprite": true`)
and if the base body uses the shadowwed forehead or larger without 'bald' head (`"isBaldStyle": true`).

The hair textures need to be structured like vanilla, facing foward is the top row, facing right
the second row, and facing up is the third row. If you have set `"usesUniqueLeftSprite": true` then the
fourth row is looking left.

#### Animation Frames when walking, running or riding
The `anim_frames` is a set of number which matches the sprite in the row to a particular frame
of animation. For example `"Walk": { 0: 0, 1:1, 2:0, 3:2 },` - the walking animations have 4 frames,
and this hair has 3 different drawings horizontally showing, so on the;
1.  first frame (0) it will show the first sprite (hair down)
2.  second frame (1) it will show the second sprite (hair flick to the left)
3.  third frame (2) it will show the first sprite again
4.  fourth frame (3) it will show the third sprite (hair flick to the right)

Running has 8 frames, so you can double up if you aren't doing a lot of animating. Riding unforutnately
uses 5 frames (that's how the horse is animated), so it needs some creative license.

You can also define the animations for each direction, `"WalkLeft"`/`"WalkUp"`/`"WalkRight"`/`"WalkDown"`,
make sure to do this for all 4 directions.

#### Bigger hair!
You can make your hair a bit bigger by using a few extra options;
```
﻿{
  "hairStyles": {
    "8": {
      "usesUniqueLeftSprite": true,
      "isBaldStyle": false,
      "yOffset": -16,
      "anim_frames": {
        "Walk": { 0: 0, 1:1, 2:2, 3:1 },
        "Run": { 0: 0, 1:0, 2:1, 3:1, 4:2, 5:2, 6:1, 7:1 },
        "Ride": { 0: 1, 1:0, 2:1, 3:2, 4:1, 5:0 },
      }
    },
    "24": {
      "usesUniqueLeftSprite": false,
      "isBaldStyle": false,
      "extraWidth": 16,
      "anim_frames": {
        "Walk": { 0: 0, 1:2, 2:4, 3:6 },
        "Run": { 0: 0, 1:1, 2:2, 3:3, 4:4, 5:5, 6:6, 7:7 },
        "Ride": { 0:0, 1:1, 2:2, 3:4, 4:5, 5:6 },
      }
    },
  }
}
```
`yOffset` allows you to move the hair up if needed, and `extraWidth` let's DynmicBodies know that
your hair spirte is bigger than 16pixels, eg `"extraWidth": 8` would mean you have a 24 pixel width
hair. It will always centre it though!

### Shoes
More to come! Check the sample packs to help.

### Shirt Overlays
More to come! Check the sample packs to help. 

## Working with other mods
More to come! Check the sample packs to help. 
