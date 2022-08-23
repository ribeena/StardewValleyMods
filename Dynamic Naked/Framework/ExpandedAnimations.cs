using System;
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
        public static Rectangle getFrameRectangle(Farmer who, ExtendedHair.hairSettings settings, bool doesFlipped = true)
        {
            Rectangle frameRectangle = new Rectangle(0, 0, 16, 32);

            FarmerSprite sprite = who.FarmerSprite;

            string anim_name = "standing";
            switch (sprite.CurrentSingleAnimation)
            {
                case FarmerSprite.carryWalkUp:
                case FarmerSprite.walkUp:
                    anim_name = "WalkUp";
                    break;
                case FarmerSprite.carryWalkDown:
                case FarmerSprite.walkDown:
                    anim_name = "WalkDown";
                    break;
                case FarmerSprite.carryWalkRight:
                case FarmerSprite.walkRight:
                    anim_name = "WalkRight";
                    break;
                case FarmerSprite.carryWalkLeft:
                case FarmerSprite.walkLeft:
                    anim_name = "WalkLeft";
                    break;
                case FarmerSprite.carryRunDown:
                case FarmerSprite.runDown:
                    anim_name = "RunDown";
                    break;
                case FarmerSprite.carryRunUp:
                case FarmerSprite.runUp:
                    anim_name = "RunUp";
                    break;
                case FarmerSprite.carryRunRight:
                case FarmerSprite.runRight:
                    anim_name = "RunRight";
                    break;
                case FarmerSprite.carryRunLeft:
                case FarmerSprite.runLeft:
                    anim_name = "RunLeft";
                    break;
            }

            if(who.FacingDirection == 1 || who.FacingDirection == 3)
            {
                frameRectangle.Y = 32;
            }

            if(who.FacingDirection == 2)
            {
                frameRectangle.Y = 64;
            }

            if(!doesFlipped && who.FacingDirection == 3)
            {
                frameRectangle.Y = 96;
            }

            bool moving = false;
            if (who.CanMove && who.isMoving())
            {
                moving = true;
            }

            if (moving)
            {
                int totalAnimationFrames = sprite.currentAnimation.Count;

                if (settings.anim_frames[anim_name].frameMatch.Count == totalAnimationFrames)
                {
                    frameRectangle.X = settings.anim_frames[anim_name].frameMatch[sprite.currentFrame] * 16;
                } else
                {
                    ModEntry.debugmsg($"The hair animation expected [{totalAnimationFrames}] for '{anim_name}'", LogLevel.Debug);
                }
            }

            return frameRectangle;
        }
    }
}
