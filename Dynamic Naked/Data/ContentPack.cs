﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace DynamicBodies
{
    internal class ContentPack
    {
        public struct characterSettings
        {
            public Dictionary<string, string> arms { get; set; }
            public Dictionary<string, string> hairBodys { get; set; }
            public Dictionary<string, string> hairFaces { get; set; }
            public Dictionary<string, string> nakedUppers { get; set; }
            public Dictionary<string, string> nakedLowers { get; set; }
            public Dictionary<string, string> bodyStyles { get; set; }
            public Dictionary<string, string> bodyHair { get; set; }
            public Dictionary<string, string> beards { get; set; }
        }
        public characterSettings unisex { get; set; }
        public characterSettings male { get; set; }
        public characterSettings female { get; set; }

        public ContentPack()
        {
        }

        public List<ContentPackOption> GetOptions(IContentPack contentPack, string prop)
        {
            switch (prop)
            {
                case "bodyStyles":
                    return GetOptions_bodyStyles(contentPack);
                case "arms":
                    return GetOptions_arms(contentPack);
                case "bodyHair":
                    return GetOptions_bodyHair(contentPack);
                case "beards":
                    return GetOptions_beards(contentPack);
                case "nakedUppers":
                    return GetOptions_nakedUppers(contentPack);
                case "nakedLowers":
                    return GetOptions_nakedLowers(contentPack);
                default:
                    return null;
            }
        }

        private List<ContentPackOption> GetOptions_bodyStyles(IContentPack contentPack)
        {
            List<ContentPackOption> options = new List<ContentPackOption>();

            if (unisex.bodyStyles != null && unisex.bodyStyles.Count > 0)
            {
                //record all the body options
                foreach (var dataKeyPair in unisex.bodyStyles)
                {
                    if (contentPack.HasFile($"assets\\bodies\\{dataKeyPair.Value}.png")
                        && contentPack.HasFile($"assets\\bodies\\{dataKeyPair.Value}_bald.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption bodyOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack);
                        options.Add(bodyOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added unisex body file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.monitor.Log($"{contentPack.Manifest.Name} is missing a unisex body file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (male.bodyStyles != null && male.bodyStyles.Count > 0)
            {
                //record all the body options
                foreach (var dataKeyPair in male.bodyStyles)
                {
                    if (contentPack.HasFile($"assets\\bodies\\{dataKeyPair.Value}.png")
                        && contentPack.HasFile($"assets\\bodies\\{dataKeyPair.Value}_bald.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption bodyOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, true);
                        options.Add(bodyOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added male body file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.monitor.Log($"{contentPack.Manifest.Name} is missing a male body file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (female.bodyStyles != null && female.bodyStyles.Count > 0)
            {
                //record all the body options
                foreach (var dataKeyPair in female.bodyStyles)
                {
                    if (contentPack.HasFile($"assets\\bodies\\{dataKeyPair.Value}.png")
                        && contentPack.HasFile($"assets\\bodies\\{dataKeyPair.Value}_bald.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption bodyOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, false);
                        options.Add(bodyOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added female body file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.monitor.Log($"{contentPack.Manifest.Name} is missing a female body file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            return options;
        }

        private List<ContentPackOption> GetOptions_arms(IContentPack contentPack)
        {
            List<ContentPackOption> options = new List<ContentPackOption>();

            if (unisex.arms != null && unisex.arms.Count > 0)
            {
                //record all the arm options
                foreach (var dataKeyPair in unisex.arms)
                {
                    if (contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Sleeveless.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Normal.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Short.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Long.png"))
                    {
                        ContentPackOption armOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack);
                        options.Add(armOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added unisex arms file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a unisex arms file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (male.arms != null && male.arms.Count > 0)
            {
                //record all the arm options
                foreach (var dataKeyPair in male.arms)
                {
                    if (contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Sleeveless.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Normal.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Short.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Long.png"))
                    {
                        ContentPackOption armOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, true);
                        options.Add(armOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added male arms file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a male arms file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (female.arms != null && female.arms.Count > 0)
            {
                //record all the arm options
                foreach (var dataKeyPair in female.arms)
                {
                    if (contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Sleeveless.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Normal.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Short.png")
                        && contentPack.HasFile($"assets\\arms\\{dataKeyPair.Value}_Long.png"))
                    {
                        ContentPackOption armOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, false);
                        options.Add(armOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added female arms file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a female arms file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            return options;
        }

        private List<ContentPackOption> GetOptions_bodyHair(IContentPack contentPack)
        {
            List<ContentPackOption> options = new List<ContentPackOption>();

            if (unisex.bodyHair != null && unisex.bodyHair.Count > 0)
            {
                //record all the arm options
                foreach (var dataKeyPair in unisex.bodyHair)
                {
                    if (contentPack.HasFile($"assets\\bodyHair\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption bodyHairOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack);
                        options.Add(bodyHairOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added unisex body hair file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a unisex body hair file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (male.bodyHair != null && male.bodyHair.Count > 0)
            {
                //record all the arm options
                foreach (var dataKeyPair in male.bodyHair)
                {
                    if (contentPack.HasFile($"assets\\bodyHair\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption bodyHairOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, true);
                        options.Add(bodyHairOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added male body hair file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a male body hair file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (female.bodyHair != null && female.bodyHair.Count > 0)
            {
                //record all the arm options
                foreach (var dataKeyPair in female.bodyHair)
                {
                    if (contentPack.HasFile($"assets\\bodyHair\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption bodyHairOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, false);
                        options.Add(bodyHairOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added female body hair file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a female body hair file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            return options;
        }

        private List<ContentPackOption> GetOptions_beards(IContentPack contentPack)
        {
            List<ContentPackOption> options = new List<ContentPackOption>();
            if (unisex.beards != null && unisex.beards.Count > 0)
            {
                foreach (var dataKeyPair in unisex.beards)
                {
                    if (contentPack.HasFile($"assets\\beards\\{dataKeyPair.Value}.png"))
                    {
                        ContentPackOption beardOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack);
                        options.Add(beardOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added unisex beard file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a unisex beard file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (male.beards != null && male.beards.Count > 0)
            {
                foreach (var dataKeyPair in male.beards)
                {
                    if (contentPack.HasFile($"assets\\beards\\{dataKeyPair.Value}.png"))
                    {
                        ContentPackOption beardOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, true);
                        options.Add(beardOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added male beard file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a male beard file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (female.beards != null && female.beards.Count > 0)
            {
                foreach (var dataKeyPair in female.beards)
                {
                    if (contentPack.HasFile($"assets\\beards\\{dataKeyPair.Value}.png"))
                    {
                        ContentPackOption beardOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, false);
                        options.Add(beardOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added female beard file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a female beard file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }
            return options;
        }

        private List<ContentPackOption> GetOptions_nakedLowers(IContentPack contentPack)
        {
            List<ContentPackOption> options = new List<ContentPackOption>();
            if (unisex.nakedLowers != null && unisex.nakedLowers.Count > 0)
            {

                //record all the options
                foreach (var dataKeyPair in unisex.nakedLowers)
                {
                    if (contentPack.HasFile($"assets\\nakedLower\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption nakedOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack);
                        options.Add(nakedOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added unisex naked lower file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a unisex naked lower file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (male.nakedLowers != null && male.nakedLowers.Count > 0)
            {

                //record all the arm options
                foreach (var dataKeyPair in male.nakedLowers)
                {
                    if (contentPack.HasFile($"assets\\nakedLower\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption nakedOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, true);
                        options.Add(nakedOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added male naked overlay file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a male naked lower file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (female.nakedLowers != null && female.nakedLowers.Count > 0)
            {

                //record all the arm options
                foreach (var dataKeyPair in female.nakedLowers)
                {
                    if (contentPack.HasFile($"assets\\nakedLower\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption nakedOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, false);
                        options.Add(nakedOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added female naked overlay file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a female naked lower file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }
            return options;
        }


        private List<ContentPackOption> GetOptions_nakedUppers(IContentPack contentPack)
        {
            List<ContentPackOption> options = new List<ContentPackOption>();
            if (unisex.nakedUppers != null && unisex.nakedUppers.Count > 0)
            {

                //record all the options
                foreach (var dataKeyPair in unisex.nakedUppers)
                {
                    if (contentPack.HasFile($"assets\\nakedUpper\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption nakedOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack);
                        options.Add(nakedOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added unisex naked lower file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a unisex naked lower file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (male.nakedUppers != null && male.nakedUppers.Count > 0)
            {

                //record all the arm options
                foreach (var dataKeyPair in male.nakedUppers)
                {
                    if (contentPack.HasFile($"assets\\nakedUpper\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption nakedOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, true);
                        options.Add(nakedOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added male naked overlay file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a male naked lower file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }

            if (female.nakedUppers != null && female.nakedUppers.Count > 0)
            {

                //record all the arm options
                foreach (var dataKeyPair in female.nakedUppers)
                {
                    if (contentPack.HasFile($"assets\\nakedUpper\\{dataKeyPair.Value}.png"))
                    {
                        //TODO check the images are right size
                        //Texture2D image = contentPack.LoadAsset<Texture2D>("image.png");
                        ContentPackOption nakedOption = new ContentPackOption(dataKeyPair.Key, dataKeyPair.Value, contentPack.Manifest.Author, contentPack, false);
                        options.Add(nakedOption);
                        ModEntry.debugmsg($"{contentPack.Manifest.Name} added female naked overlay file for '{dataKeyPair.Value}'", LogLevel.Debug);

                    }
                    else
                    {
                        ModEntry.context.Monitor.Log($"{contentPack.Manifest.Name} is missing a female naked lower file for '{dataKeyPair.Value}'", LogLevel.Debug);
                    }
                }
            }
            return options;
        }
    }
}
