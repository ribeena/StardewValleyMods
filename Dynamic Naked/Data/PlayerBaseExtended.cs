using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

using DynamicBodies.Patches;

namespace DynamicBodies.Data
{
    public class PlayerBaseExtended
    {
        public static Dictionary<string, PlayerBaseExtended> extendedFarmers = new Dictionary<string, PlayerBaseExtended>();

        public float initialLayerDepth = 0f;
        public bool overrideCheck = false;
        public Texture2D cacheImage;
        public int shirt { get; set; }
        public int shirtOverlayIndex = -1;
        public int pants { get; set; }
        public int shoes { get; set; }
        public uint hair;
        public uint dhair;
        //Draw rendering
        public string sleeveLength { get; set; }
        public string shoeStyle { get; set; }
        public BodyPartStyle vanilla;
        public BodyPartStyle body;
        public BodyPartStyle face;
        public BodyPartStyle arm;
        public BodyPartStyle beard;
        public BodyPartStyle bodyHair;
        public BodyPartStyle nakedUpper;
        public BodyPartStyle nakedLower;

        //Overlays/Accessories rendering
        public Dictionary<int, Texture2D> hairTextures = new Dictionary<int, Texture2D>();
        public Texture2D lowerSkinOverlay = null;
        public string lowerSkinOverlayName = "Default";
        public bool dirty { get; set; }
        public int firstFrame = 1;
        public bool recalcVanillaRecolor { get; set; }
        public PlayerBaseExtended(Farmer who) : this(who, who.FarmerRenderer.textureName.Value) { }
        public PlayerBaseExtended(Farmer who, string baseTexture)
        {
            body = new BodyPartStyle("body");

            vanilla = new BodyPartStyle("body");
            SetVanillaFile(baseTexture);

            face = new BodyPartStyle("face");
            arm = new BodyPartStyle("arm");
            beard = new BodyPartStyle("beard");
            bodyHair = new BodyPartStyle("bodyHair");
            nakedLower = new BodyPartStyle("nakedLower");
            nakedUpper = new BodyPartStyle("nakedUpper");

            shirt = who.shirt.Value;
            pants = who.pants.Value;
            shoes = who.shoes.Value;
            hair = (uint)who.hairColor;
            dhair = 0;
            sleeveLength = "Normal";
            shoeStyle = "Normal";

            recalcVanillaRecolor = true;

            dirty = true;

            string whoID = GetKey(who);
            extendedFarmers[whoID] = this;
        }

        public static PlayerBaseExtended Get(Farmer who)
        {
            if (extendedFarmers.ContainsKey(GetKey(who)))
            {
                return extendedFarmers[GetKey(who)];
            }
            return null;
        }

        public static PlayerBaseExtended Get(string whoID)
        {
            if (extendedFarmers.ContainsKey(whoID))
            {
                return extendedFarmers[whoID];
            }
            return null;
        }

        public static string GetKey(Farmer who)
        {
            if (who.isFakeEventActor)
            {
                return who.Name + "_fake";
            }
            return who.Name;
        }

        public void DefaultOptions(Farmer who)
        {
            body.SetDefault(who);
            SetVanillaFile(who.FarmerRenderer.textureName.Value);
            body.file = vanilla.file;

            face.SetDefault(who);
            arm.SetDefault(who);
            beard.SetDefault(who);
            bodyHair.SetDefault(who);
            nakedLower.SetDefault(who);
            nakedUpper.SetDefault(who);
        }

        public void SetModData(Farmer who, string key, string value)
        {
            bool change = false;
            if (who.modData.ContainsKey(key))
            {
                if(who.modData[key] != value)
                {
                    change = true;
                }
            }
            who.modData[key] = value;
            if (change)
            {
                UpdateTextures(who);
            }
        }

        public void SetVanillaFile(string file)
        {
            vanilla.file = file.Split("\\").Last();
            if (vanilla.file == "farmer_girl_base") { vanilla.file = "farmer_base"; }
            if (vanilla.file == "farmer_girl_base_bald") { vanilla.file = "farmer_base_bald"; }
        }

        public void UpdateTextures(Farmer who)
        {
            PlayerBaseExtended pbe = PlayerBaseExtended.Get(who);

            //Check for custom body
            if (!pbe.body.OptionMatchesModData(who))
            {
                pbe.body.SetOptionFromModData(who, ModEntry.bodyOptions);
                pbe.dirty = true;
            }

            //Check for custom face
            if (!pbe.face.OptionMatchesModData(who))
            {
                pbe.face.SetOptionFromModData(who, ModEntry.faceOptions);
                pbe.dirty = true;
            }

            //Check for custom arms
            if (!pbe.arm.OptionMatchesModData(who))
            {
                pbe.arm.SetOptionFromModData(who, ModEntry.armOptions);
                pbe.dirty = true;
            }

            //Check for beard
            if (!pbe.beard.OptionMatchesModData(who))
            {
                pbe.beard.SetOptionFromModData(who, ModEntry.beardOptions);
                pbe.dirty = true;
            }

            //Check for naked overlay
            if (!pbe.nakedLower.OptionMatchesModData(who))
            {
                pbe.nakedLower.SetOptionFromModData(who, ModEntry.nudeLOptions);
                pbe.dirty = true;
            }

            if (!pbe.nakedUpper.OptionMatchesModData(who))
            {
                pbe.nakedUpper.SetOptionFromModData(who, ModEntry.nudeUOptions);
                pbe.dirty = true;
            }
        }

        public Texture2D GetHairTexture(Farmer who, int hair_style, Texture2D source_texture, Rectangle rect)
        {

            CheckHairTextures(who); //Redraw if needed

            //there's a cached image
            if (hairTextures.ContainsKey(hair_style) && hairTextures[hair_style] != null)
            {
                return hairTextures[hair_style];
            }

            hairTextures[hair_style] = RenderHair(who, source_texture, rect);
            return hairTextures[hair_style];
        }

        public Texture2D GetBodyHairTexture(Farmer who)
        {
            CheckHairTextures(who); //Redraw if needed

            if (bodyHair.texture == null)
            {
                Texture2D bodyHairText2D = bodyHair.provider.ModContent.Load<Texture2D>($"assets\\bodyhair\\{bodyHair.file}.png");
                Rectangle rect = new Rectangle(0, 0, bodyHairText2D.Width, bodyHairText2D.Height);
                bodyHair.texture = new Texture2D(Game1.graphics.GraphicsDevice, bodyHairText2D.Width, bodyHairText2D.Height);
                Color[] data = new Color[bodyHairText2D.Width * bodyHairText2D.Height];
                bodyHairText2D.GetData(data, 0, data.Length);
                bodyHair.texture.SetData(data);
                bodyHair.texture = RenderHair(who, bodyHair.texture, rect);
            }
            return bodyHair.texture;
        }

        public Texture2D GetBeardTexture(Farmer who, int beard_style, Texture2D source_texture, Rectangle rect)
        {
            CheckHairTextures(who); //Redraw if needed

            //there's a cached image
            if (beard.textures.ContainsKey(beard_style.ToString()))
            {
                return beard.textures[beard_style.ToString()];
            }

            beard.textures[beard_style.ToString()] = RenderHair(who, source_texture, rect);
            return beard.textures[beard_style.ToString()];
        }

        public Texture2D GetBeardTexture(Farmer who)
        {
            CheckHairTextures(who); //Redraw if needed

            if (!beard.textures.ContainsKey(beard.option) && beard.option != "Default")
            {
                //Build a new one
                Texture2D beardText2D = beard.provider.ModContent.Load<Texture2D>($"assets\\beards\\{beard.file}.png");
                Rectangle rect = new Rectangle(0, 0, beardText2D.Width, beardText2D.Height);
                beard.textures[beard.option] = new Texture2D(Game1.graphics.GraphicsDevice, beardText2D.Width, beardText2D.Height);
                Color[] data = new Color[beardText2D.Width * beardText2D.Height];
                beardText2D.GetData(data, 0, data.Length);
                beard.textures[beard.option].SetData(data);
                beard.textures[beard.option] = RenderHair(who, beard.textures[beard.option], rect);
            }
            return beard.textures[beard.option];
        }

        public Texture2D RenderHair(Farmer who, Texture2D source_texture, Rectangle rect)
        {
            Texture2D hairText2D = null;
            //Need to render a new texture
            hairText2D = new Texture2D(Game1.graphics.GraphicsDevice, rect.Width, rect.Height);
            Color[] HairData = new Color[hairText2D.Width * hairText2D.Height];
            source_texture.GetData<Color>(source_texture.LevelCount - 1, rect, HairData, 0, hairText2D.Width * hairText2D.Height);

            //monitor.Log($"Building new hair texture", LogLevel.Debug);

            //Colours to replace
            Color hairdark = new Color(57, 57, 57);//default, generally dark
            if (who.modData.ContainsKey("DB.darkHair"))
            {
                hairdark = new Color(uint.Parse(who.modData["DB.darkHair"]));
            }
            Color hairdarker = Color.Lerp(hairdark, Color.Black, 0.25f);
            Color hair = who.hairstyleColor.Value;
            Color hairLight = Color.Lerp(hair, Color.White, 0.25f);
            Color hairThreshold = new Color(57, 57, 57, 99);//36% or 99 alpha is the shadow overlay onto skin
            Color hairUpper = new Color(240, 240, 240);
            Color hairRange = new Color(hairUpper.R - hairThreshold.R, hairUpper.G - hairThreshold.G, hairUpper.B - hairThreshold.B);

            for (int i = 0; i < HairData.Length; i++)
            {
                Color hairpixel = HairData[i];
                //Check if the pixel is more solid than the hair-shadow colour
                bool changeit = hairpixel.A > hairThreshold.A;
                //Check if it's transparent that it is grey, then change it
                if (!changeit && hairpixel.A > 0)
                {
                    changeit = hairpixel.R == hairpixel.G && hairpixel.G == hairpixel.B;
                }
                if (changeit)
                {
                    byte alpha = hairpixel.A;
                    //Currently only uses the red chanel - ignores tinting
                    if (hairpixel.R >= hairThreshold.R || hairpixel.R < hairUpper.R)
                    {
                        float perc = (float)(hairpixel.R - hairThreshold.R) / (float)hairRange.R;
                        HairData[i] = Color.Lerp(hairdark, hair, perc);
                    }

                    if (hairpixel.R < hairThreshold.R)
                    {
                        float perc = (float)(hairpixel.R) / (float)hairThreshold.R;
                        HairData[i] = Color.Lerp(hairdarker, hairdark, perc);
                    }

                    if (hairpixel.R >= hairUpper.R)
                    {
                        float perc = (float)(hairpixel.R - hairUpper.R) / (float)(byte.MaxValue - hairUpper.R);
                        HairData[i] = Color.Lerp(hair, hairLight, perc);
                    }
                    HairData[i].A = alpha;
                }
            }
            hairText2D.SetData<Color>(HairData);
            return hairText2D;
        }

        public void CheckHairTextures(Farmer who)
        {
            //Check for hair colour
            if (hair != (uint)who.hairColor)
            {
                hair = (uint)who.hairColor;
                ResetHairTextures();
            }

            //Check for dark hair colour
            if (who.modData.ContainsKey("DB.darkHair"))
            {
                if (dhair != uint.Parse(who.modData["DB.darkHair"]))
                {
                    dhair = uint.Parse(who.modData["DB.darkHair"]);
                    ResetHairTextures();
                }
            }
        }

        public void ResetHairTextures()
        {
            ModEntry.debugmsg($"Reset hair textures", LogLevel.Debug);

            hairTextures.Clear();
            beard.Clear();
            bodyHair.Clear();
        }

        public Texture2D GetNakedUpperTexture(int skin)
        {
            if (dirty)
            {
                nakedUpper.texture = null;
            }

            if (nakedUpper.texture == null && nakedUpper.option != "Default")
            {
                Texture2D texture = nakedUpper.provider.ModContent.Load<Texture2D>($"assets\\nakedUpper\\{nakedUpper.file}.png");
                //recalculate the skin colours on the overlay
                nakedUpper.texture = ApplySkinColor(skin, texture);
            }
            if (nakedUpper.option == "Default")
            {
                return null;
            }
            return nakedUpper.texture;
        }

        public Texture2D GetNakedLowerTexture(int skin)
        {
            if (dirty)
            {
                nakedLower.texture = null;
            }

            if (nakedLower.texture == null && nakedLower.option != "Default")
            {
                Texture2D texture = nakedLower.provider.ModContent.Load<Texture2D>($"assets\\nakedLower\\{nakedLower.file}.png");
                
                if (nakedLower.CheckForOption("no skin"))
                {
                    //Don't calculate new colours, it doesn't have them
                    nakedLower.texture = texture;
                }
                else
                {
                    //recalculate the skin colours on the overlay
                    nakedLower.texture = ApplySkinColor(skin, texture);
                }
            }
            if (nakedLower.option == "Default")
            {
                return null;
            }
            return nakedLower.texture;
        }

        private static Texture2D ApplySkinColor(int skin, Texture2D source_texture)
        {
            Texture2D texture = null;
            //Need to render a new texture
            texture = new Texture2D(Game1.graphics.GraphicsDevice, source_texture.Width, source_texture.Height);

            //Calculate the skin colours
            int which = skin;
            Texture2D skinColors = Game1.content.Load<Texture2D>("Characters/Farmer/skinColors");
            Texture2D glandColors = Game1.content.Load<Texture2D>("Mods/ribeena.dynamicbodies/assets/Character/extendedSkinColors.png");

            Color[] skinColorsData = new Color[skinColors.Width * skinColors.Height];
            if (which < 0) which = skinColors.Height - 1;
            if (which > skinColors.Height - 1) which = 0;
            skinColors.GetData(skinColorsData);

            Color[] glandColorsData = new Color[glandColors.Width * glandColors.Height];
            if (which < 0) which = glandColors.Height - 1;
            if (which > glandColors.Height - 1) which = 0;
            glandColors.GetData(glandColorsData);

            //Colours to replace
            Color darkest_old = skinColorsData[0], medium_old = skinColorsData[1], lightest_old = skinColorsData[2];
            Color glandDark_old = glandColorsData[0], glandLight_old = glandColorsData[1];

            //Store what the colours are
            Color darkest = skinColorsData[which * 3 % (skinColors.Height * 3)];
            Color medium = skinColorsData[which * 3 % (skinColors.Height * 3) + 1];
            Color lightest = skinColorsData[which * 3 % (skinColors.Height * 3) + 2];
            Color glandDark = glandColorsData[which * 2 % (glandColors.Height * 2)];
            Color glandLight = glandColorsData[which * 2 % (glandColors.Height * 2) + 1];

            Color[] data = new Color[texture.Width * texture.Height];
            source_texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Equals(darkest_old))
                {
                    data[i] = darkest;
                }
                else if (data[i].Equals(medium_old))
                {
                    data[i] = medium;
                }
                else if (data[i].Equals(lightest_old))
                {
                    data[i] = lightest;
                }
                else if (data[i].Equals(glandDark_old))
                {
                    data[i] = glandDark;
                }
                else if (data[i].Equals(glandLight_old))
                {
                    data[i] = glandLight;
                }

            }
            texture.SetData<Color>(data);
            return texture;
        }

        public static Texture2D ApplyExtendedSkinColor(int skin, Texture2D source_texture)
        {
            Texture2D texture = null;
            //Need to render a new texture
            texture = new Texture2D(Game1.graphics.GraphicsDevice, source_texture.Width, source_texture.Height);

            //Calculate the skin colours
            int which = skin;
            Texture2D glandColors = Game1.content.Load<Texture2D>("Mods/ribeena.dynamicbodies/assets/Character/extendedSkinColors.png");

            Color[] glandColorsData = new Color[glandColors.Width * glandColors.Height];
            if (which < 0) which = glandColors.Height - 1;
            if (which > glandColors.Height - 1) which = 0;
            glandColors.GetData(glandColorsData);

            //Colours to replace
            Color glandDark_old = glandColorsData[0], glandLight_old = glandColorsData[1];

            //Store what the colours are
            Color glandDark = glandColorsData[which * 2 % (glandColors.Height * 2)];
            Color glandLight = glandColorsData[which * 2 % (glandColors.Height * 2) + 1];

            Color[] data = new Color[texture.Width * texture.Height];
            source_texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Equals(glandDark_old))
                {
                    data[i] = glandDark;
                }
                else if (data[i].Equals(glandLight_old))
                {
                    data[i] = glandLight;
                }

            }
            texture.SetData<Color>(data);
            return texture;
        }

    }

    public class BodyPartStyle
    {
        public string name;
        public string option;
        public string file;
        public IContentPack provider;
        public Texture2D texture;
        public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        public List<string> metadata;
        public bool fullAnimation = true;
        public BodyPartStyle(string n)
        {
            name = n;
            option = "Default";
            file = "";
            textures = new Dictionary<string, Texture2D>();
        }

        public string GetOptionModData(Farmer who)
        {
            if (who == null) { return "Default"; }
            return (who.modData.ContainsKey("DB." + name) ? who.modData["DB." + name] : "Default");
        }

        public bool OptionMatchesModData(Farmer who)
        {
            return option == (who.modData.ContainsKey("DB." + name) ? who.modData["DB." + name] : "Default");
        }

        public bool CheckForOption(string option)
        {
            if (metadata == null) return false;
            return metadata.Contains(option);
        }

        public void SetOptionFromModData(Farmer who, List<ContentPackOption> options)
        {
            option = (who.modData.ContainsKey("DB." + name) ? who.modData["DB." + name] : "Default");
            //Clear the texture loaded as we are setting a new value
            if (texture != null) texture.Dispose();
            texture = null;

            if (option == "Default")
            {
                provider = null;
                file = null;
                metadata = null;
            }
            else
            {
                ContentPackOption choice = ModEntry.getContentPack(options, who.modData["DB." + name]);
                if (choice == null)
                {
                    //Option not installed
                    option = "Default";
                    provider = null;
                    metadata = null;
                }
                else
                {
                    provider = choice.contentPack;
                    file = choice.file;
                    metadata = choice.metadata;
                    if(metadata != null && metadata.Contains("no animation"))
                    {
                        fullAnimation = false;
                    } else
                    {
                        fullAnimation = true;
                    }
                }
            }
        }

        public void SetDefault(Farmer who)
        {
            who.modData["DB." + name] = "Default";
            provider = null;
            file = null;
        }

        public void Clear()
        {
            texture = null;
            textures.Clear();
        }
    }
}
