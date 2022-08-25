﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using Netcode;

using DynamicBodies.Data;
using DynamicBodies.Framework;
using StardewValley.Objects;

namespace DynamicBodies.Framework
{
    internal class ExpandedAnimations
    {
        public static Rectangle getFrameRectangle(Farmer who, ExtendedHair.hairSettings settings, int height = 32, int width = 16, int facingDirection = -1)
        {
            Rectangle frameRectangle = new Rectangle(0, 0, width, height);

            if(facingDirection < 0)
            {
                facingDirection = who.FacingDirection;
            }

            FarmerSprite sprite = who.FarmerSprite;

            string anim_name = "Standing";
            string anim_group = "Standing";

            //down run 0, walk 1
            //left right, run 6, walk 7
            //up run 12, walk 13

            switch (sprite.CurrentSingleAnimation)
            {
                case 1:
                    anim_name = "WalkUp";
                    anim_group = "Walk";
                    break;
                case 13:
                    anim_name = "WalkDown";
                    anim_group = "Walk";
                    break;
                case 7:
                    if (who.FacingDirection == 1)
                    {
                        anim_name = "WalkRight";
                    } else
                    {
                        anim_name = "WalkLeft";
                    }
                    anim_group = "Walk";
                    break;
                case 0:
                    anim_name = "RunDown";
                    anim_group = "Run";
                    break;
                case 12:
                    anim_name = "RunUp";
                    anim_group = "Run";
                    break;
                case 6:
                    if (who.FacingDirection == 1)
                    {
                        anim_name = "RunRight";
                    }
                    else
                    {
                        anim_name = "RunLeft";
                    }
                    anim_group = "Run";
                    break;
                case 107:
                    anim_name = "RideDown";
                    anim_group = "Ride";
                    break;
                case 113:
                    anim_name = "RideUp";
                    anim_group = "Ride";
                    break;
                case 106:
                    if (who.FacingDirection == 1)
                    {
                        anim_name = "RideRight";
                    }
                    else
                    {
                        anim_name = "RideLeft";
                    }
                    anim_group = "Ride";
                    break;
            }

            if(facingDirection == 1 || facingDirection == 3)
            {
                frameRectangle.Y = height;
            }

            if(facingDirection == 0)
            {
                frameRectangle.Y = height*2;
            }

            if(settings.usesUniqueLeftSprite && facingDirection == 3)
            {
                frameRectangle.Y = height*3;
            }

            bool moving = false;
            if (who.CanMove && who.isMoving())
            {
                moving = true;
            }

            //Handle run/walk/ride aniamtions
            if (moving)
            {
                int totalAnimationFrames = sprite.currentAnimation.Count;
                int index = sprite.currentAnimationIndex;

                if (anim_name.StartsWith("Ride") && who.mount != null)
                {
                    totalAnimationFrames = who.mount.Sprite.currentAnimation.Count;
                    index = who.mount.Sprite.currentAnimationIndex;
                }

                if (settings.anim_frames.ContainsKey(anim_name))
                {

                    if (settings.anim_frames[anim_name].Count == totalAnimationFrames)
                    {
                        frameRectangle.X = settings.anim_frames[anim_name][index] * width;
                    }
                    else
                    {
                        ModEntry.debugmsg($"The hair animation expected [{totalAnimationFrames}] for '{anim_name}'", LogLevel.Debug);
                    }
                } else if (settings.anim_frames.ContainsKey(anim_group))
                {
                    if (settings.anim_frames[anim_group].Count == totalAnimationFrames)
                    {
                        frameRectangle.X = settings.anim_frames[anim_group][index] * width;
                    }
                    else
                    {
                        ModEntry.debugmsg($"The hair animation expected [{totalAnimationFrames}] for '{anim_group}'", LogLevel.Debug);
                    }
                } else
                {
                    ModEntry.debugmsg($"The hair animation didn't find frame matches for '{anim_name}' or '{anim_group}'", LogLevel.Debug);
                }
            }

            return frameRectangle;
        }
    }
}
