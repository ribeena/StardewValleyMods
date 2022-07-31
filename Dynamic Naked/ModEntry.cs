﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

using StardewValley.Locations;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;
using StardewValley.Objects;
using System.IO;
using DynamicBodies.UI;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

using DynamicBodies.Data;
using DynamicBodies.Patches;
using DynamicBodies.Framework;

namespace DynamicBodies
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static ModEntry context;//single instance

        public static ModConfig Config;

        public static bool british_spelling = false;
        public static BritishStrings engb;

        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        internal static BootsPatched bootsPatcher;
        internal static FarmerRendererPatched farmerRendererPatcher;

        private static bool debugMode = true;

        public static IGenericModConfigMenuApi configMenu = null;

        public static float FS_pantslayer = 0.009E-05f;
        public static float hairlayer = 2.25E-05f;

        //Hard code shirt styles for now, 1019
        public static int[] shortShirts = { 1002, 1004, 1005, 1006, 1008, 1009, 1012, 1013, 1021, 1026, 1027, 1028, 1135, 1136, 1141, 1156, 1157, 1158, 1168, 1187, 1190, 1194, 1195, 1200, 1204, 1209, 1210, 1225, 1226, 1229, 1236, 1238, 1242, 1247, 1256, 1257, 1259, 1261, 1265, 1287, 1293, 1296 };
        public static int[] longShirts = { 1000, 1007, 1010, 1011, 1014, 1017, 1018, 1020, 1029, 1030, 1087, 1123, 1128, 1131, 1137, 1138, 1140, 1154, 1155, 1159, 1160, 1161, 1162, 1163, 1164, 1167, 1172, 1173, 1178, 1183, 1184, 1185, 1186, 1191, 1201, 1211, 1213, 1216, 1071, 1218, 1221, 1224, 1231, 1235, 1237, 1240, 1248, 1251, 1253, 1255, 1260, 1266, 1267, 1268, 1270, 1277, 1278, 1280, 1281, 1285, 1289, 1290, 1292, 1294 };

        //Content pack options
        public static List<ContentPackOption> bodyOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> faceOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> armOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> bodyHairOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> beardOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> nudeLOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> nudeUOptions = new List<ContentPackOption>();
        public static Dictionary<string, ContentPackOption> shoeOverrides = new Dictionary<string, ContentPackOption>();
        public static List<ShirtOverlay> shirtOverlays = new List<ShirtOverlay>();

        public const string sleeveSetting = "DB.sleeveOverride";

        public static Effect paletteSwap;
        public static Effect hairRamp;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {

            modHelper = helper;
            monitor = Monitor;

            FS_pantslayer = 0.009E-05f;

            context = this;

            //fallback fix
            context.Helper.Events.Content.AssetRequested += OnRequestAsset;
            //Load the config
            Config = context.Helper.ReadConfig<ModConfig>();
            debugMode = Config.debugmsgs;

            context.Helper.Events.Content.AssetReady += OnAssetReady;

            context.Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            context.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            context.Helper.Events.GameLoop.Saving += OnSaveRevertTextures;

            var harmony = new Harmony(ModManifest.UniqueID);

            
            //Fix up rendering of the farmer
            farmerRendererPatcher = new FarmerRendererPatched(harmony);

            //Intervene with the loading process so we can store separate textures per user
            //and add event listeners when FarmerRender is made
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), "farmerInit"),
                postfix: new HarmonyMethod(GetType(), nameof(post_Farmer_setup))
            );

            //Patch for touch events
            /*harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction), new[] { typeof(string), typeof(Vector2) }),
                prefix: new HarmonyMethod(GetType(), nameof(pre_performAction))
            );*/

            //Patch for actions on maps
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new[] { typeof(string), typeof(Farmer), typeof(Location) }),
                prefix: new HarmonyMethod(GetType(), nameof(pre_performAction))
            );
            //Fixes the destroying of left item in tailoring menu
            harmony.Patch(
                original: AccessTools.Method(typeof(TailoringMenu), nameof(TailoringMenu.SpendLeftItem)),
                postfix: new HarmonyMethod(GetType(), nameof(post_SpendLeftItem))
            );

            //Patch for tailoring menu based off an object
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction), new[] {typeof(Farmer), typeof(bool)}),
                prefix: new HarmonyMethod(GetType(), nameof(pre_checkForAction))
            );

            //Fix up rendering of boots
            bootsPatcher = new BootsPatched(harmony);


            

            helper.ConsoleCommands.Add("db_layer_get", "gets layer", delegate { context.Monitor.Log($"OK, layer is {hairlayer}.", LogLevel.Debug); });
            helper.ConsoleCommands.Add("db_layer_set", "sets layer", delegate (string command, string[] args) { hairlayer = float.Parse(args[0]); });
            helper.ConsoleCommands.Add("db_gen_png", "Saves a png of the player in mod folder.", delegate (string command, string[] args) {
                if (!File.Exists($"{context.Helper.DirectoryPath}\\farmer_sprite.png"))
                {
                    Stream stream = File.Create($"{context.Helper.DirectoryPath}\\farmer_sprite.png");
                    Farmer who = Game1.player;
                    GetFarmerBaseSprite(who).SaveAsPng(stream, 288, 672);
                    context.Monitor.Log($"OK, saved {context.Helper.DirectoryPath}\\farmer_sprite.png.", LogLevel.Debug);
                } else
                {
                    context.Monitor.Log($"Sorry, please delete {context.Helper.DirectoryPath}\\farmer_sprite.png.", LogLevel.Debug);
                }
            });

            helper.ConsoleCommands.Add("db_cache_png", "Saves a png of the cached player in mod folder.", delegate (string command, string[] args) {
                if (!File.Exists($"{context.Helper.DirectoryPath}\\debug_sprite.png"))
                {
                    Stream stream = File.Create($"{context.Helper.DirectoryPath}\\debug_sprite.png");
                    Farmer who = Game1.player;
                    PlayerBaseExtended.Get(who).cacheImage.SaveAsPng(stream, 288, 672);
                    context.Monitor.Log($"OK, saved {context.Helper.DirectoryPath}\\debug_sprite.png.", LogLevel.Debug);
                }
                else
                {
                    context.Monitor.Log($"Sorry, please delete {context.Helper.DirectoryPath}\\debug_sprite.png.", LogLevel.Debug);
                }
            });

        }

        public static string translate(string toTranslate)
        {
            //Very simple British English compatibility
            if (british_spelling) {
                if (engb.strings.ContainsKey(toTranslate))
                {
                    return engb.strings[toTranslate];
                }
            }
            return context.Helper.Translation.Get(toTranslate);
        }

        public static void post_Farmer_setup(Farmer __instance)
        {
            __instance.boots.fieldChangeEvent += delegate { FarmerRendererPatched.FieldChanged("shoes", __instance); };
            __instance.shirtItem.fieldChangeEvent += delegate { FarmerRendererPatched.FieldChanged("shirt", __instance); };
            __instance.pantsItem.fieldChangeEvent += delegate { FarmerRendererPatched.FieldChanged("pants", __instance); };
        }


        //Fix for shirts/pants as ingredients
        public static void post_SpendLeftItem(TailoringMenu __instance)
        {
            if (__instance.leftIngredientSpot.item != null && __instance.leftIngredientSpot.item is Clothing)
            {
                __instance.leftIngredientSpot.item = null;
            }
        }


        public static void debugmsg(string str, LogLevel logLevel)
        {
            if (debugMode)
            {
                context.Monitor.Log(str, LogLevel.Debug);
            }
        }

        

        public void OnRequestAsset(object sender, AssetRequestedEventArgs args)
        {
            //context.Monitor.Log($"assetrequested [{args.Name.BaseName}].", LogLevel.Debug);
            if (args.Name.BaseName.StartsWith("Characters/Farmer/farmer_base!") || args.Name.BaseName.StartsWith("Characters\\Farmer\\farmer_base"))
            {
                args.LoadFromModFile<Texture2D>("assets\\Character\\farmer_base.png", AssetLoadPriority.Low);
                debugmsg($"AssetRequested Fix: [{args.Name.BaseName}].", LogLevel.Debug);
            }

            //Allow other mods to edit the UI with content patcher
            if (args.Name.StartsWith("Mods/ribeena.dynamicbodies/assets/") && args.DataType == typeof(Texture2D))
            {
                args.LoadFromModFile<Texture2D> (args.Name.ToString().Substring("Mods/ribeena.dynamicbodies/".Length), AssetLoadPriority.Low);
            }

            if (args.Name.StartsWith("Mods/ribeena.dynamicbodies/assets/") && args.DataType == typeof(IRawTextureData))
            {
                args.LoadFromModFile<IRawTextureData>(args.Name.ToString().Substring("Mods/ribeena.dynamicbodies/".Length), AssetLoadPriority.Low);
            }

            if (args.Name.IsEquivalentTo("Maps\\townInterior")
                || args.Name.IsEquivalentTo("Mods/ribeena.dynamicbodies/assets/Character/shirts_overlay.png"))
            {
                args.Edit(EditImageAsset);
            }

            if (args.Name.IsEquivalentTo("Maps\\springobjects"))
            {
                args.Edit(bootsPatcher.PatchImage);
            }

            if (args.Name.IsEquivalentTo("Maps\\Hospital")
                || args.Name.IsEquivalentTo("Maps\\LeahHouse")
                || args.Name.IsEquivalentTo("Maps\\Trailer")
                || args.Name.IsEquivalentTo("Maps\\Trailer_big")
                || args.Name.IsEquivalentTo("Maps\\HaleyHouse")) 
            {
                args.Edit(EditMapAsset);
            }
        }

        public void EditImageAsset(IAssetData asset)
        {
            
            //Adjust the shirtoverlays for any additional tops
            if (asset.Name.IsEquivalentTo("Mods/ribeena.dynamicbodies/assets/Character/shirts_overlay.png"))
            {
                int totalShirtOverlays = shirtOverlays.Count;
                foreach (ShirtOverlay shirtOverlay in shirtOverlays)
                {
                    totalShirtOverlays += shirtOverlay.total;
                }

                //Extend the overlay image for custom shirts
                if (totalShirtOverlays > 0)
                {
                    debugmsg($"Extending shirt overlay pack for {totalShirtOverlays} overlay sets", LogLevel.Debug);
                    var editor = asset.AsImage();


                    int index = (int)(editor.Data.Height / 32) * 16;

                    int rows = (int)(totalShirtOverlays / 16);//rounds down
                    rows++;//always need at least one
                    var oldTex = editor.Data;
                    debugmsg($"Extending shirt overlay is {editor.Data.Height}", LogLevel.Debug);
                    editor.ExtendImage(editor.Data.Width, editor.Data.Height + (32 * rows));
                    debugmsg($"Extending shirt overlay changed to {editor.Data.Height}", LogLevel.Debug);


                    foreach (ShirtOverlay shirtOverlay in shirtOverlays)
                    {
                        foreach (var kvp in shirtOverlay.overlays)
                        {
                            if (kvp.Value.Count > 0)
                            {
                                Texture2D texture = shirtOverlay.contentPack.ModContent.Load<Texture2D>("Shirts\\" + kvp.Value[0]);
                                editor.PatchImage(texture, targetArea: new Rectangle((index % 16) * 8, (int)(index / 16) * 32, 8, 32));
                                shirtOverlay.SetIndex(kvp.Key, index);
                                index++;
                                if (kvp.Value.Count > 1)
                                {
                                    editor.PatchImage(texture, targetArea: new Rectangle((index % 16) * 8, (int)(index / 16) * 32, 8, 32));
                                    index++;
                                }
                            }
                        }
                    }

                }
            }

            //Add the graphic for the hospital interaction
            if (asset.Name.IsEquivalentTo("Maps\\townInterior"))
            {
                var editor = asset.AsImage();
                Texture2D buyCounter = context.Helper.ModContent.Load<Texture2D>("assets\\Maps\\townInterior_hospital.png");
                editor.PatchImage(buyCounter, targetArea: new Rectangle(320, 592, 16, 16));
                Texture2D paintCounter = context.Helper.ModContent.Load<Texture2D>("assets\\Maps\\townInterior_leah.png");
                editor.PatchImage(paintCounter, targetArea: new Rectangle(32, 640, 16, 16));
            }
        }

        public void EditMapAsset(IAssetData asset)
        {
            //Help refer to here
            //https://github.com/colinvella/tIDE/tree/master/TileMapEditor/XTile

            IAssetDataForMap mapeditor = asset.AsMap();
            xTile.Map map = mapeditor.Data;
            
            if (asset.Name.IsEquivalentTo("Maps\\Hospital"))
            {
                debugmsg($"Edit Doctor map", LogLevel.Debug);

                //Change the tile to the mirror
                xTile.Layers.Layer frontLayer = map.GetLayer("Front");
                //21 across and 38 down, sheet is 32 tiles across... 32*37+21, 1205, start at 0
                frontLayer.Tiles[3, 15].TileIndex = 1204;
                xTile.Layers.Layer buildingsLayer = map.GetLayer("Buildings");
                xTile.ObjectModel.PropertyValue dynamicBodies = new xTile.ObjectModel.PropertyValue("DynamicBodies:Doctors");
                //tile below has the action on it
                buildingsLayer.Tiles[3, 16].Properties.Add("Action", dynamicBodies);
            }
            
            if (asset.Name.IsEquivalentTo("Maps\\LeahHouse"))
            {
                debugmsg($"Edit Leah map", LogLevel.Debug);
                //Change the tile to the table
                xTile.Layers.Layer frontLayer = map.GetLayer("Front");
                xTile.Layers.Layer buildingsLayer = map.GetLayer("Buildings");
                //3 across and 53 down, sheet is 32 tiles across... 32*53+3, 1205, start at 0
                buildingsLayer.Tiles[10, 4].TileIndex = 1699;
                xTile.ObjectModel.PropertyValue dynamicBodies = new xTile.ObjectModel.PropertyValue("DynamicBodies:Leah");
                //tile below has the action on it
                buildingsLayer.Tiles[10, 4].Properties.Add("Action", dynamicBodies);

                frontLayer.Tiles[10, 3].TileIndex = 1282;
            }
            
            if (asset.Name.IsEquivalentTo("Maps\\Trailer") || asset.Name.IsEquivalentTo("Maps\\Trailer_big"))
            {
                debugmsg($"Edit Trailer map", LogLevel.Debug);
                xTile.Layers.Layer buildingsLayer = map.GetLayer("Buildings");
                xTile.ObjectModel.PropertyValue dynamicBodies = new xTile.ObjectModel.PropertyValue("DynamicBodies:Pam");
                buildingsLayer.Tiles[12, 6].Properties.Add("Action", dynamicBodies);
            }
        }

        public void OnAssetReady(object sender, AssetReadyEventArgs args)
        {
            
        }
        public static bool pre_performAction(GameLocation __instance, string action, Farmer who, Location tileLocation)
        {
            // monitor.Log($"Performed action {action}", LogLevel.Debug);
            if (action == "DynamicBodies:Doctors")
            {
                //Open the mod
                //configMenu.OpenModMenu(context.ModManifest);

                Game1.activeClickableMenu = new BodyModifier(BodyModifier.Source.Doctors);

                return true;
            }

            if (action == "DynamicBodies:Leah")
            {
                Game1.activeClickableMenu = new BodyModifier(BodyModifier.Source.Leahs);
                return true;
            }

            if (action == "DynamicBodies:Pam")
            {
                Game1.activeClickableMenu = new BodyModifier(BodyModifier.Source.Pams);
                return true;
            }

            //Override the normal tailoring menu action in Haley's House
            if(action == "Tailoring")
            {
                Game1.activeClickableMenu = new TabbedTailoringMenu();
                return false;
            }

            //Override the Wizard Shrine to include buttons for other customisation zones.
            if (action == "WizardShrine")
            {
                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:WizardTower_WizardShrine").Replace('\n', '^'), __instance.createYesNoResponses(), (Farmer who, string whichAnswer) =>
                {
                    if (whichAnswer == "Yes")
                    {
                        //default functionality
                        if (Game1.player.Money >= 500)
                        {
                            Game1.activeClickableMenu = new WizardCharacterCharacterCustomization();
                            Game1.player.Money -= 500;
                        }
                    }
                });

                return false;
            }
            return true;
        }

        //Fix for the sewing machine action
        public static bool pre_checkForAction(Object __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {
            //monitor.Log($"Performed Checking Object Action for {__instance.name}", LogLevel.Debug);
            //Do the tool action
            if (!justCheckingForActivity && who != null && who.currentLocation.isObjectAtTile(who.getTileX(), who.getTileY() - 1) && who.currentLocation.isObjectAtTile(who.getTileX(), who.getTileY() + 1) && who.currentLocation.isObjectAtTile(who.getTileX() + 1, who.getTileY()) && who.currentLocation.isObjectAtTile(who.getTileX() - 1, who.getTileY()) && !who.currentLocation.getObjectAtTile(who.getTileX(), who.getTileY() - 1).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX(), who.getTileY() + 1).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX() - 1, who.getTileY()).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX() + 1, who.getTileY()).isPassable())
            {
                return true;
            }
            //Open the new tabbed tailoring menu
            if ((bool)__instance.bigCraftable.Value)
            {
                if (!justCheckingForActivity)
                {
                    if ((int)__instance.ParentSheetIndex == 247)
                    {
                        Game1.activeClickableMenu = new TabbedTailoringMenu();
                        __result = true;
                        return false;
                    }
                }
            }
            return true;
        }

        public static Texture2D GetFarmerBaseSprite(Farmer who, string texture = "")
        {
            Texture2D bodyText2D = null;
            bool returnNew = texture == "";
            //Start modifying the base texture
            IAssetDataForImage editor = null;

            //Fix up the farmer base with options
            if ((texture.Length > 0 && texture.StartsWith("Characters\\Farmer\\farmer_") || returnNew))
            {
                string gender = "";
                if (!who.IsMale) { gender = "f_"; }

                //monitor.Log($"Edit [{pbeKeyPair.Key}] {pbeKeyPair.Value.baseStyle} image through Edit<t>", LogLevel.Debug);
                PlayerBaseExtended pbe = PlayerBaseExtended.Get(who);
                if (pbe == null)
                {
                    pbe = new PlayerBaseExtended(who, texture);
                    SetModDataDefaults(who);
                    pbe.dirty = true;

                    if (who.accessory.Value < 6 && who.accessory.Value > 0)
                    {
                        who.modData["DB.beard"] = "Vanilla's Accessory " + (who.accessory.Value + 1).ToString();
                        who.accessory.Set(0);
                    }
                }
                
                string bald = "";
                if (who.IsBaldHairStyle(who.hair.Value))
                {
                    bald = "_bald";
                }

                //Base texture needs to be redone
                if (pbe.dirtyLayers["baseTexture"])
                {
                    //Load the base texture from this mod
                    if (pbe.body.option == "Default")
                    {
                        bodyText2D = context.Helper.ModContent.Load<Texture2D>($"assets\\Character\\{gender}farmer_base.png");
                    }
                    else
                    {

                        //Otherwise load it from a content pack
                        bodyText2D = pbe.body.provider.ModContent.Load<Texture2D>($"assets\\bodies\\{gender}{pbe.body.file}.png");
                    }

                    editor = context.Helper.ModContent.GetPatchHelper(bodyText2D).AsImage();

                    if (pbe.dirtyLayers["baseTexture"]) debugmsg("base was dirty", LogLevel.Debug);

                    IRawTextureData faceText2D;
                    if (pbe.face.option == "Default")
                    {
                        faceText2D = context.Helper.ModContent.Load<IRawTextureData>($"assets\\Character\\{gender}face{bald}.png");
                    }
                    else
                    {
                        faceText2D = pbe.face.provider.ModContent.Load<IRawTextureData>($"assets\\faces\\{pbe.face.file}{bald}.png");
                    }
                    editor.PatchImage(faceText2D, new Rectangle(0, 0, faceText2D.Width, faceText2D.Height), targetArea: new Rectangle(0, 0, faceText2D.Width, faceText2D.Height), PatchMode.Overlay);




                    //Top arms
                    //editor.PatchImage(armsText2D, new Rectangle(0, 0, armsText2D.Width, armsText2D.Height-96), targetArea: new Rectangle(96, 0, armsText2D.Width, armsText2D.Height-96), PatchMode.Replace);
                    //Bottom arms
                    //editor.PatchImage(armsText2D, new Rectangle(48, 576, armsText2D.Width-48, 96), targetArea: new Rectangle(144, 576, armsText2D.Width-48, 96), PatchMode.Replace);
                    //Bath overlay
                    //editor.PatchImage(armsText2D, new Rectangle(0, 576, 48, 96), targetArea: new Rectangle(0, 576, 48, 96), PatchMode.Overlay);


                    //monitor.Log($"Edit sleeve image through Edit<t>", LogLevel.Debug);

                    IRawTextureData shoes;
                    if (pbe.shoeStyle == "None")
                    {
                        shoes = context.Helper.ModContent.Load<IRawTextureData>($"assets\\Character\\feet.png");
                        debugmsg($"Drawing feet.", LogLevel.Debug);
                    }
                    else
                    {
                        Boots equippedBoots = (Boots)who.boots;
                        if (equippedBoots == null)
                        {
                            shoes = context.Helper.ModContent.Load<IRawTextureData>($"assets\\Character\\feet.png");
                            debugmsg($"Default feet as nothing equipped found.", LogLevel.Debug);
                        }
                        else
                        {
                            if (shoeOverrides.ContainsKey(equippedBoots.Name))
                            {
                                shoes = shoeOverrides[equippedBoots.Name].contentPack.ModContent.Load<IRawTextureData>(shoeOverrides[equippedBoots.Name].file);
                                debugmsg($"Override specific shoe for [{equippedBoots.Name}].", LogLevel.Debug);
                            }
                            else
                            {
                                List<string> roughMatches = shoeOverrides.Keys.Where(key => equippedBoots.Name.StartsWith(key)).ToList();
                                if (roughMatches.Count > 0)
                                {
                                    shoes = shoeOverrides[roughMatches[0]].contentPack.ModContent.Load<IRawTextureData>(shoeOverrides[roughMatches[0]].file);
                                    debugmsg($"Override shoes group for [{equippedBoots.Name}].", LogLevel.Debug);
                                }
                                else
                                {
                                    shoes = context.Helper.ModContent.Load<IRawTextureData>($"assets\\Character\\shoes_Normal.png");
                                    debugmsg($"Default shoes for [{equippedBoots.Name}].", LogLevel.Debug);
                                }
                            }
                        }

                    }

                    editor.PatchImage(shoes, new Rectangle(0, 0, shoes.Width, shoes.Height), targetArea: new Rectangle(0, 0, shoes.Width, shoes.Height), PatchMode.Overlay);
                }

                if(pbe.dirtyLayers["arm"] || pbe.dirtyLayers["baseTexture"])
                {
                    if(editor == null)
                    {
                        editor = context.Helper.ModContent.GetPatchHelper(pbe.sourceImage).AsImage();
                        pbe.dirtyLayers["sprite"] = true;
                    }
                    
                    IRawTextureData armsText2D;
                    if (pbe.arm.option == "Default")
                    {
                        armsText2D = context.Helper.ModContent.Load<IRawTextureData>($"assets\\Character\\{gender}arm_{pbe.sleeveLength}.png");
                    }
                    else
                    {
                        armsText2D = pbe.arm.provider.ModContent.Load<IRawTextureData>($"assets\\arms\\{pbe.arm.file}_{pbe.sleeveLength}.png");
                    }
                    //editor.PatchImage(armsText2D, new Rectangle(0, 0, armsText2D.Width, armsText2D.Height), targetArea: new Rectangle(96, 0, armsText2D.Width, armsText2D.Height), PatchMode.Replace);

                    //Top row
                    editor.PatchImage(armsText2D, new Rectangle(0, 0, armsText2D.Width - 32, 32), targetArea: new Rectangle(96, 0, armsText2D.Width - 32, 32), PatchMode.Replace);
                    //remainder
                    editor.PatchImage(armsText2D, new Rectangle(0, 32, armsText2D.Width, armsText2D.Height - 32), targetArea: new Rectangle(96, 32, armsText2D.Width, armsText2D.Height - 32), PatchMode.Replace);
                }
                //Needs redrawing
                if (pbe.dirtyLayers["sprite"] || pbe.cacheImage == null)
                {
                    //Store the updated version
                    pbe.cacheImage = null;

                    pbe.cacheImage = new Texture2D(Game1.graphics.GraphicsDevice, bodyText2D.Width, bodyText2D.Height);
                    Color[] data = new Color[bodyText2D.Width * bodyText2D.Height];
                    bodyText2D.GetData(data, 0, data.Length);
                    pbe.cacheImage.SetData(data);

                    //Render any extended colours on the base
                    //pbe.cacheImage = PlayerBaseExtended.ApplyExtendedSkinColor(who.skin.Value, pbe.cacheImage);

                    pbe.dirtyLayers["sprite"] = false;
                }

                if (!returnNew)
                {
                    //Return the cached image
                    return pbe.cacheImage;
                }

            }

            

            return bodyText2D;
        }

        public static void MakePlayerDirty()
        {
            Game1.player.FarmerRenderer.MarkSpriteDirty();
            PlayerBaseExtended.Get(Game1.player).dirty = true;
        }

        

        /*********
        ** Private methods
        *********/

        private static void OnSaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            PlayerBaseExtended.extendedFarmers.Clear();
            SetModDataDefaults(Game1.player);
        }

        public static void SetModDataDefaults(Farmer who)
        {
            if (!who.modData.ContainsKey("DB.bathers"))
            {
                who.modData["DB.bathers"] = "true";
            }
            if (!who.modData.ContainsKey("DB.overallColor"))
            {
                who.modData["DB.overallColor"] = "true";
            }
		}

        private static void OnSaveRevertTextures(object sender, SavingEventArgs e)
        {
            //reset textures back so save files are clean
            foreach(Farmer who in Game1.getAllFarmers())
            {
                who.FarmerRenderer.textureName.Set("Characters\\Farmer\\" + PlayerBaseExtended.Get(who).vanilla.file);
            }

        }

        

        public static string AssignShirtLength(Clothing item, bool isMale)
        {
            //Set the shirt sleeve override
            if (item.modData.ContainsKey(sleeveSetting))
            {
                return item.modData[sleeveSetting];
            }
            else
            {
                int shirt = isMale ? 209 : 41;

                if (isMale || item.indexInTileSheetFemale.Value < 0)
                {
                    shirt = item.indexInTileSheetMale.Value;
                }
                else
                {
                    shirt = item.indexInTileSheetFemale.Value;
                }

                item.modData[sleeveSetting] = "Normal";
                //set the shirt to appropriate
                if (shortShirts.Contains(shirt + 1000) || item.GetOtherData().Contains("DB.Short"))
                {
                    item.modData[sleeveSetting] = "Short";
                }
                if (longShirts.Contains(shirt + 1000) || item.GetOtherData().Contains("DB.Long"))
                {
                    item.modData[sleeveSetting] = "Long";
                }
                if (item.GetOtherData().Contains("Sleeveless"))
                {
                    item.modData[sleeveSetting] = "Sleeveless";
                }

                debugmsg($"Shirt[{shirt}] changed to {item.modData[sleeveSetting]}.", LogLevel.Debug);
            }
            
            return item.modData[sleeveSetting];

        }

        

        public static string[] getContentPackOptions(List<ContentPackOption> options, bool male = true)
        {
            List<string> toReturn = new List<string>();
            toReturn.Add("Default");
            foreach (ContentPackOption option in options)
            {
                if (option.male && male)
                {
                    toReturn.Add(option.author + "'s " + option.name);
                    //context.Monitor.Log($"ContentPack option [{option.author + "'s " + option.name}].", LogLevel.Debug);
                } else if(option.female && !male)
                {
                    toReturn.Add(option.author + "'s " + option.name);
                }
            }
            return toReturn.ToArray();
        }

        public static ContentPackOption getContentPack(List<ContentPackOption> options, string find)
        {
            foreach (ContentPackOption option in options)
            {
                if(option.author+"'s "+option.name == find)
                {
                    return option;
                }
            }
            return null;
        }

        public void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //Try using a palette swapping shader
            // Compile via the command: mgfxc paletteSwap.fx paletteSwap.mgfx
            paletteSwap = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Effects", "paletteSwap.mgfx")));

            //Set the default palette to test for
            ModEntry.paletteSwap.Parameters["xSourcePalette"].SetValue(PlayerBaseExtended.GetBasePalette());

            hairRamp = new Effect(Game1.graphics.GraphicsDevice, File.ReadAllBytes(Path.Combine(modHelper.DirectoryPath, "Effects", "greyRamp.mgfx")));


            //Add vanilla options to the beards
            beardOptions.Add(new ContentPackOption("Accessory 1", "0", "Vanilla", null));
            beardOptions.Add(new ContentPackOption("Accessory 2", "1", "Vanilla", null));
            beardOptions.Add(new ContentPackOption("Accessory 3", "2", "Vanilla", null));
            beardOptions.Add(new ContentPackOption("Accessory 4", "3", "Vanilla", null));
            beardOptions.Add(new ContentPackOption("Accessory 5", "4", "Vanilla", null));
            beardOptions.Add(new ContentPackOption("Accessory 6", "5", "Vanilla", null));

            debugmsg($"Checking content packs for {context.Helper.ContentPacks.ModID}, {context.Helper.ContentPacks.GetOwned().Count()}", LogLevel.Debug);

            //
            int totalShirtOverlays = 0;
            //Set-up the content pack options
            foreach (IContentPack contentPack in context.Helper.ContentPacks.GetOwned())
            {
                debugmsg($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}", LogLevel.Debug);
                //Attempt to load any content packs
                if (contentPack.HasFile("content.json"))
                {
                    ContentPack data = contentPack.ReadJsonFile<ContentPack>("content.json");
                    bodyOptions.AddRange(data.GetOptions(contentPack, "bodyStyles"));
                    faceOptions.AddRange(data.GetOptions(contentPack, "faces"));
                    armOptions.AddRange(data.GetOptions(contentPack, "arms"));
                    bodyHairOptions.AddRange(data.GetOptions(contentPack, "bodyHair"));
                    beardOptions.AddRange(data.GetOptions(contentPack, "beards"));
                    nudeLOptions.AddRange(data.GetOptions(contentPack, "nakedLowers"));
                    nudeUOptions.AddRange(data.GetOptions(contentPack, "nakedUppers"));
                }

                //Animated boot styles
                if (contentPack.HasFile("Boots\\boots.json"))
                {
                    debugmsg($"Loading shoe override pack: {contentPack.Manifest.Name}", LogLevel.Debug);

                    ExtendedBoots data = contentPack.ReadJsonFile<ExtendedBoots>("Boots\\boots.json");


                    foreach (var dataKeyPair in data.overrides)
                    {
                        debugmsg($"{contentPack.Manifest.Name} added boots file for '{dataKeyPair.Key}'", LogLevel.Debug);

                        foreach (string BootType in dataKeyPair.Value)
                        {
                            ContentPackOption shoeOption = new ContentPackOption(BootType, $"Boots\\{dataKeyPair.Key}.png", contentPack.Manifest.Author, contentPack);
                            shoeOverrides[BootType] = shoeOption;
                        }
                        
                    }
                }

                //Pant overlays on shirts
                if (contentPack.HasFile("Shirts\\shirts.json"))
                {
                    debugmsg($"Loading shirt overlay pack: {contentPack.Manifest.Name}", LogLevel.Debug);

                    ShirtOverlay data = contentPack.ReadJsonFile<ShirtOverlay>("Shirts\\shirts.json");
                    totalShirtOverlays += data.overlays.Count();
                    data.contentPack = contentPack;

                    debugmsg($"{contentPack.Manifest.Name} added shirt overlays.", LogLevel.Debug);
                    shirtOverlays.Add(data);
                }
            }

            //Check for British spelling
            british_spelling = Helper.ModRegistry.IsLoaded("TheMightyAmondee.GoodbyeAmericanEnglish");
            debugmsg($"British spelling: {british_spelling}", LogLevel.Debug);
            if (british_spelling)
            {
                engb = Helper.ModContent.Load<BritishStrings>("assets\\i18n\\en-gb.json");
            }

            //////////////////////////////////////////////
            // Add support for generic mod menu
            // get Generic Mod Config Menu's API (if it's installed)
            configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return; //don't do the config menu stuff

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(Config)
            );

            // add some config options
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Free character customisation",
                getValue: () => Config.freecustomisation,
                setValue: value => Config.freecustomisation = value
            );
        }

    }

    public class MultiplayerMessage
    {
        public string message { get; set; }
        public MultiplayerMessage(string msg)
        {
            message = msg;
        }
    }

    public class ContentPackOption
    {
        public string name { get; set; }
        public string file { get; set; }
        public string author { get; set; }
        public bool male = true;
        public bool female = true;
        public List<string> metadata { get; set; }
        public IContentPack contentPack;
        public ContentPackOption(string name, string file, string author, IContentPack contentPack)
        {
            this.name = name;
            this.file = file;
            this.author = author;
            this.contentPack = contentPack;
        }

        public ContentPackOption(string name, string file, string author, IContentPack contentPack, bool gender)
        {
            this.name = name;
            this.file = file;
            this.author = author;
            this.contentPack = contentPack;
            if (gender)
            {
                female = false;
            } else
            {
                male = false;
            }
        }

        public void SetMetadata(List<string> data)
        {
            metadata = data;
        }

        public bool hasMetadata(string query)
        {
            return metadata.Contains(query);
        }

    }

}
