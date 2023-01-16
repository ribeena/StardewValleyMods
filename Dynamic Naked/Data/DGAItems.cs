using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicBodies.Data
{
    public class DGAItems
    {
        static Dictionary<string, DGAShirt> shirts = new Dictionary<string, DGAShirt>();
        static Dictionary<string, DGAPants> pants = new Dictionary<string, DGAPants>();

        public static DGAPants GetDGAPants(string dgaid)
        {
            if (!pants.ContainsKey(dgaid)) return null;
            return pants[dgaid];
        }

        public static void AddDGAPants(DGAPants pantsToAdd)
        {
            pants[pantsToAdd.ID] = pantsToAdd;
        }

        public static DGAShirt GetDGAShirt(string dgaid)
        {
            if (!shirts.ContainsKey(dgaid)) return null;
            return shirts[dgaid];
        }

        public static void AddDGAShirt(DGAShirt shirtToAdd)
        {
            shirts[shirtToAdd.ID] = shirtToAdd;
        }

        public static string GetTextureLocationVariant(string location, string bodystyle)
        {
            if (location.Contains("."))
            {
                string[] textureparts = location.Split(".");
                textureparts[textureparts.Length - 2] = textureparts[textureparts.Length - 2] + "_" + bodystyle;
                return string.Join(".", textureparts);
            }
            else
            {
                ModEntry.monitor.Log("Issue with " + location, LogLevel.Alert);

                return location;
            }
        }

        public static string SanitizeDGA(string location)
        {
            if(location.Contains("@"))
            {
                ModEntry.monitor.Log("DGA animation is not supported. First frame will be used only. Please contact content pack author and advise to fix.", LogLevel.Alert);

                return location.Split("@")[0];
            }
            return location;
        }

        public KeyValuePair<string,int[]> GetFrames(string dgatexture)
        {
            KeyValuePair<string, int[]> frames;
            if (dgatexture.Contains(","))
            {
                string location = "";
                List<int> frameindexs = new List<int>();

                string[] framesStrings = dgatexture.Split(",");
                for(int i = 0; i < framesStrings.Length; i++)
                {
                    string[] frameIndex = framesStrings[i].Split(":");
                    location = frameIndex[0];
                    string[] amounts = frameIndex[1].Split("@");
                    int amount = int.Parse(amounts[1]);
                    
                    if (amounts[0].Contains("..."))
                    {
                        int start = int.Parse(amounts[0].Split("...")[0]);
                        int end = int.Parse(amounts[0].Split("...")[1]);
                        for(int j = start; j < end; j++)
                        {
                            for(int k = 0; k < amount; k++) frameindexs.Add(j);
                        }
                    }
                    else
                    {
                        int index = int.Parse(amounts[0]);
                        for (int j = 0; j < amount; j++)
                        {
                            frameindexs.Add(index);
                        }
                    }
                }
                frames = new KeyValuePair<string, int[]>(location, frameindexs.ToArray());
            } else
            {
                if (dgatexture.Contains(":"))
                {
                    //Default single frame specified
                    frames = new KeyValuePair<string, int[]>(dgatexture.Split(":")[0], new int[] { int.Parse(dgatexture.Split(":")[1]) });
                }
                else
                {
                    //Default no frame specified
                    frames = new KeyValuePair<string, int[]>(dgatexture, new int[] { 0 });
                }
            }
            return frames;
        }
    }
    public class DGAShirt
    {
        public string ID = string.Empty;
        IContentPack contentPack;
        string texture;
        string male_color;
        string male_overlay;

        string female_texture;
        string female_color;
        string female_overlay;

        public string metadata;

        public DGAShirt(string id, IContentPack contentPack, string texture, string male_color = "", string male_overlay = "", string female_texture = "", string female_color = "", string female_overlay = "", string metadata = "")
        {
            ID = id;
            this.contentPack = contentPack;
            this.texture = texture;
            this.male_color = male_color;
            this.male_overlay = male_overlay;
            this.female_texture = female_texture;
            if (female_texture == "") this.female_texture = texture;
            this.female_color = female_color;
            if (female_color == "" && male_color != "") this.female_color = male_color;
            this.female_overlay = female_overlay;
            if (female_overlay == "" && male_overlay != "") this.female_color = male_overlay;
            this.metadata = metadata;
        }

        public Texture2D GetTexture(bool isMale, bool isColor = false, bool isOverlay = false, string bodystyle = "")
        {
            string location = texture;
            if (isColor) { location = male_color; }
            if (isOverlay) { location = male_overlay; }
            if (!isMale) { 
                location = female_texture;
                if (isColor) { location = female_color; }
                if (isOverlay) { location = female_overlay; }
            }

            if (location == "") return null;
            
            if (bodystyle != "")
            {
                string bodystyle_location = DGAItems.GetTextureLocationVariant(location, bodystyle);
                if (contentPack.HasFile(bodystyle_location))
                {
                    return contentPack.ModContent.Load<Texture2D>(bodystyle_location.Split(":")[0]);
                }
            }
            return contentPack.ModContent.Load<Texture2D>(location.Split(":")[0]);
        }

        public int GetIndex(bool isMale, bool isColor = false, bool isOverlay = false, string bodystyle = "")
        {
            string location = texture;
            if (isColor) { location = male_color; }
            if (isOverlay) { location = male_overlay; }
            if (!isMale)
            {
                location = female_texture;
                if (isColor) { location = female_color; }
                if (isOverlay) { location = female_overlay; }
            }

            if (bodystyle != "")
            {
                string bodystyle_location = DGAItems.GetTextureLocationVariant(location, bodystyle);
                if (contentPack.HasFile(bodystyle_location))
                {
                    if(bodystyle_location.Contains(":")) return int.Parse(bodystyle_location.Split(":")[1]);
                    return 0;
                }
            }
            if (location.Contains(":")) return int.Parse(location.Split(":")[1]);
            return 0;
        }
    }

    public class DGAPants
    {
        public string ID = string.Empty;
        IContentPack contentPack;
        string texture;

        string metadata = string.Empty;

        public DGAPants(string id, IContentPack contentPack, string texture, string metadata = "")
        {
            ID = id;
            this.contentPack = contentPack;
            this.texture = texture;
            this.metadata = metadata;
        }

        public Texture2D GetTexture(string bodystyle = "")
        {
            if(bodystyle != "")
            {
                string bodystyle_texture = DGAItems.GetTextureLocationVariant(texture, bodystyle);
                if (contentPack.HasFile(bodystyle_texture))
                {
                    return contentPack.ModContent.Load<Texture2D>(bodystyle_texture);
                }
            }
            return contentPack.ModContent.Load<Texture2D>(texture);
        }

        public int GetIndex(string bodystyle = "")
        {
            string location = texture;
            
            if (bodystyle != "")
            {
                string bodystyle_location = DGAItems.GetTextureLocationVariant(location, bodystyle);
                if (contentPack.HasFile(bodystyle_location))
                {
                    if (bodystyle_location.Contains(":")) return int.Parse(bodystyle_location.Split(":")[1]);
                    return 0;
                }
            }
            if (location.Contains(":")) return int.Parse(location.Split(":")[1]);
            return 0;
        }
    }
}
