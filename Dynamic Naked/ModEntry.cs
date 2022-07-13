using HarmonyLib;
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

namespace DynamicBodies
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static ModEntry context;//single instance

        //private static ModConfig Config;

        public static bool british_spelling = false;
        public static BritishStrings engb;

        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        internal static BootsPatched bootsPatcher;

        private static bool debugMode = false;

        public static IGenericModConfigMenuApi configMenu = null;

        public static float FS_pantslayer = 0.009E-05f;
        public static float hairlayer = 2.25E-05f;

        //Hard code shirt styles for now, 1019
        public static int[] shortShirts = { 1002, 1004, 1005, 1006, 1008, 1009, 1012, 1013, 1021, 1026, 1027, 1028, 1135, 1136, 1141, 1156, 1157, 1158, 1168, 1187, 1190, 1194, 1195, 1200, 1204, 1209, 1210, 1225, 1226, 1229, 1236, 1238, 1242, 1247, 1256, 1257, 1259, 1261, 1265, 1287, 1293, 1296 };
        public static int[] longShirts = { 1000, 1007, 1010, 1011, 1014, 1017, 1018, 1020, 1029, 1030, 1087, 1123, 1128, 1131, 1137, 1138, 1140, 1154, 1155, 1159, 1160, 1161, 1162, 1163, 1164, 1167, 1172, 1173, 1178, 1183, 1184, 1185, 1186, 1191, 1201, 1211, 1213, 1216, 1071, 1218, 1221, 1224, 1231, 1235, 1237, 1240, 1248, 1251, 1253, 1255, 1260, 1266, 1267, 1268, 1270, 1277, 1278, 1280, 1281, 1285, 1289, 1290, 1292, 1294 };

        //Content pack options
        public static List<ContentPackOption> bodyOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> armOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> bodyHairOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> beardOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> nudeLOptions = new List<ContentPackOption>();
        public static List<ContentPackOption> nudeUOptions = new List<ContentPackOption>();
        public static Dictionary<string, ContentPackOption> shoeOverrides = new Dictionary<string, ContentPackOption>();
        public static List<ShirtOverlay> shirtOverlays = new List<ShirtOverlay>();

        public const string sleeveSetting = "DB.sleeveOverride";

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
            //Config = context.Helper.ReadConfig<ModConfig>();
            context.Helper.Events.Content.AssetReady += OnAssetReady;

            context.Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            context.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            context.Helper.Events.GameLoop.Saving += OnSaveRevertTextures;

            var harmony = new Harmony(ModManifest.UniqueID);

            //Draw the Hair, beards and naked overlay
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories), new Type[] { typeof(SpriteBatch), typeof(int), typeof(Farmer), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(float), typeof(Color), typeof(float) }),
                prefix: new HarmonyMethod(GetType(), nameof(pre_DrawHairAndAccesories))
            );
            //Intervene with the loading process so we can store separate textures per user
            harmony.Patch(
                original: AccessTools.Constructor(typeof(FarmerRenderer), new[] { typeof(string), typeof(Farmer) }),
                postfix: new HarmonyMethod(GetType(), nameof(FarmerRenderer_setup))
            );
            //Intervene when texture has changed to ensure lcmo is in place
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), "textureChanged"),
                prefix: new HarmonyMethod(GetType(), nameof(pre_TextureChanged))
            );
            //Event for base texture changed
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.MarkSpriteDirty)),
                prefix: new HarmonyMethod(GetType(), nameof(pre_MarkSpriteDirty))
            );
            //Calculate any dirty flags for recaching the image
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), new[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) }),
                prefix: new HarmonyMethod(GetType(), nameof(pre_Draw))
            );
            //Draw new mini portraits
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.drawMiniPortrat), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(int), typeof(Farmer) }),
                prefix: new HarmonyMethod(GetType(), nameof(pre_drawMiniPortrat))
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


            harmony.CreateReversePatcher(AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions", new[] { typeof(Farmer) }), new HarmonyMethod(GetType(), nameof(ExecuteRecolorActionsReversePatch))).Patch();


            helper.ConsoleCommands.Add("db_layer_get", "gets layer", delegate { context.Monitor.Log($"OK, layer is {hairlayer}.", LogLevel.Debug); });
            helper.ConsoleCommands.Add("db_layer_set", "sets layer", delegate (string command, string[] args) { hairlayer = float.Parse(args[0]); });
            helper.ConsoleCommands.Add("db_save_png", "Saves a png of the player in mod folder.", delegate (string command, string[] args) {
                if (!File.Exists($"{context.Helper.DirectoryPath}\\debug_sprite.png"))
                {
                    Stream stream = File.Create($"{context.Helper.DirectoryPath}\\debug_sprite.png");

                    GetFarmerBaseSprite(Game1.player, "Characters\\Farmer\\farmer_base").SaveAsPng(stream, 288, 672);
                    context.Monitor.Log($"OK, saved {context.Helper.DirectoryPath}\\debug_sprite.png.", LogLevel.Debug);
                } else
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


        //Adjust the base texture before rendering
        private static void FarmerRenderer_setup(FarmerRenderer __instance, ref LocalizedContentManager ___farmerTextureManager, string textureName, Farmer farmer)
        {
            debugmsg($"LCMO in farmerRenderer constructor for {farmer.Name}/{farmer.UniqueMultiplayerID}", LogLevel.Debug);
            //Add a wrapping layer around the texture manager for the farmerrenderer
            LocalizedContentManagerOverride lcmo = new LocalizedContentManagerOverride(___farmerTextureManager.ServiceProvider, ___farmerTextureManager.RootDirectory);
            ___farmerTextureManager = (LocalizedContentManager)lcmo.CreateTemporary(context, farmer);
        }

        public static void pre_MarkSpriteDirty(FarmerRenderer __instance, LocalizedContentManager ___farmerTextureManager)
        {
            LocalizedContentManagerOverride lcmo = ___farmerTextureManager as LocalizedContentManagerOverride;
            if (lcmo != null)
            {
                PlayerBaseExtended pbe = PlayerBaseExtended.Get(lcmo.who);
                if (pbe != null)
                {
                    pbe.dirty = true;
                    pbe.UpdateTextures(lcmo.who);
                }
            }
        }

        public static bool pre_TextureChanged(FarmerRenderer __instance, ref Texture2D ___baseTexture, NetString ___textureName, LocalizedContentManager ___farmerTextureManager)
        {
            LocalizedContentManagerOverride lcmo = ___farmerTextureManager as LocalizedContentManagerOverride;
            if (lcmo != null)
            {
                PlayerBaseExtended pbe = PlayerBaseExtended.Get(lcmo.who);
                //if (___baseTexture != null) ___baseTexture.Dispose();
                //___baseTexture = null;
                ___baseTexture = pbe.cacheImage;
            } else
            {
                //if (___baseTexture != null) ___baseTexture.Dispose();
                ___baseTexture = null;
                debugmsg($"LCMO wasn't loaded - repatching loading", LogLevel.Debug);
            }

            if (___baseTexture == null)
            {
                //Vanilla code fallback
                Texture2D source_texture = ___farmerTextureManager.Load<Texture2D>(___textureName.Value);
                ___baseTexture = new Texture2D(Game1.graphics.GraphicsDevice, source_texture.Width, source_texture.Height);
                Color[] data = new Color[source_texture.Width * source_texture.Height];
                source_texture.GetData(data, 0, data.Length);
                ___baseTexture.SetData(data);
            }

            return false;
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
            if (args.Name.StartsWith("Mods/ribeena.dynamicbodies/assets/"))
            {
                args.LoadFromModFile<Texture2D>(args.Name.ToString().Substring("Mods/ribeena.dynamicbodies/".Length), AssetLoadPriority.Low);
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

        public static Texture2D GetFarmerBaseSprite(Farmer who, string texture)
        {
            Texture2D bodyText2D = null;
            //Fix up the farmer base with options
            if (texture.Length > 0 && texture.StartsWith("Characters\\Farmer\\farmer_"))
            {

                string gender = "";
                if (texture.StartsWith("Characters\\Farmer\\farmer_girl")) { gender = "f_"; }

                //monitor.Log($"Edit [{pbeKeyPair.Key}] {pbeKeyPair.Value.baseStyle} image through Edit<t>", LogLevel.Debug);
                PlayerBaseExtended pbe = PlayerBaseExtended.Get(who);
                if (pbe == null)
                {
                    pbe = new PlayerBaseExtended(who, texture, -2, who.GetPantsIndex(), -2);
                    SetModDataDefaults(who);
                    pbe.dirty = true;

                    if (who.accessory.Value < 6 && who.accessory.Value > 0)
                    {
                        who.modData["DB.beard"] = "Vanilla's Accessory " + (who.accessory.Value + 1).ToString();
                        who.accessory.Set(0);
                    }
                } else
                {
                    if(pbe.cacheImage == null)
                    {
                        pbe.dirty = true;
                    }
                }

                //flagged for redrawing
                if (pbe.dirty)
                {

                    //Load the base texture from this mod
                    if (pbe.body.option == "Default")
                    {
                        bodyText2D = context.Helper.ModContent.Load<Texture2D>($"assets\\Character\\{gender}{pbe.vanilla.file}.png");
                    }
                    else
                    {
                        string bald = "";
                        if (pbe.vanilla.file.EndsWith("_bald"))
                        {
                            bald = "_bald";
                        }
                        //Otherwise load it from a content pack
                        bodyText2D = pbe.body.provider.ModContent.Load<Texture2D>($"assets\\bodies\\{gender}{pbe.body.file}{bald}.png");
                    }

                    var editor = context.Helper.ModContent.GetPatchHelper(bodyText2D).AsImage();

                    Texture2D armsText2D;
                    if (pbe.arm.option == "Default")
                    {
                        armsText2D = context.Helper.ModContent.Load<Texture2D>($"assets\\Character\\{gender}arm_{pbe.sleeveLength}.png");
                    }
                    else
                    {
                        armsText2D = pbe.arm.provider.ModContent.Load<Texture2D>($"assets\\arms\\{pbe.arm.file}_{pbe.sleeveLength}.png");
                    }
                    editor.PatchImage(armsText2D, new Rectangle(0, 0, armsText2D.Width, armsText2D.Height), targetArea: new Rectangle(96, 0, armsText2D.Width, armsText2D.Height), PatchMode.Replace);

                    //Top arms
                    //editor.PatchImage(armsText2D, new Rectangle(0, 0, armsText2D.Width, armsText2D.Height-96), targetArea: new Rectangle(96, 0, armsText2D.Width, armsText2D.Height-96), PatchMode.Replace);
                    //Bottom arms
                    //editor.PatchImage(armsText2D, new Rectangle(48, 576, armsText2D.Width-48, 96), targetArea: new Rectangle(144, 576, armsText2D.Width-48, 96), PatchMode.Replace);
                    //Bath overlay
                    //editor.PatchImage(armsText2D, new Rectangle(0, 576, 48, 96), targetArea: new Rectangle(0, 576, 48, 96), PatchMode.Overlay);


                    //monitor.Log($"Edit sleeve image through Edit<t>", LogLevel.Debug);

                    Texture2D shoes;
                    if (pbe.shoeStyle == "None")
                    {
                        shoes = context.Helper.ModContent.Load<Texture2D>($"assets\\Character\\feet.png");
                        debugmsg($"Drawing feet.", LogLevel.Debug);
                    } else
                    {
                        Boots equippedBoots = (Boots)who.boots;
                        if (equippedBoots == null)
                        {
                            shoes = context.Helper.ModContent.Load<Texture2D>($"assets\\Character\\shoes_Normal.png");
                            debugmsg($"Default shoes as nothing equipped found.", LogLevel.Debug);
                        }
                        else
                        {
                            if (shoeOverrides.ContainsKey(equippedBoots.Name))
                            {
                                shoes = shoeOverrides[equippedBoots.Name].contentPack.ModContent.Load<Texture2D>(shoeOverrides[equippedBoots.Name].file);
                                debugmsg($"Override specific shoe for [{equippedBoots.Name}].", LogLevel.Debug);
                            }
                            else
                            {
                                List<string> roughMatches = shoeOverrides.Keys.Where(key => equippedBoots.Name.StartsWith(key)).ToList();
                                if (roughMatches.Count > 0)
                                {
                                    shoes = shoeOverrides[roughMatches[0]].contentPack.ModContent.Load<Texture2D>(shoeOverrides[roughMatches[0]].file);
                                    debugmsg($"Override shoes group for [{equippedBoots.Name}].", LogLevel.Debug);
                                }
                                else
                                {
                                    shoes = context.Helper.ModContent.Load<Texture2D>($"assets\\Character\\shoes_Normal.png");
                                    debugmsg($"Default shoes for [{equippedBoots.Name}].", LogLevel.Debug);
                                }
                            }
                        }

                    }

                    editor.PatchImage(shoes, new Rectangle(0, 0, shoes.Width, shoes.Height), targetArea: new Rectangle(0, 0, shoes.Width, shoes.Height), PatchMode.Overlay);

                    //Store the updated version
                    pbe.cacheImage = null;

                    pbe.cacheImage = new Texture2D(Game1.graphics.GraphicsDevice, bodyText2D.Width, bodyText2D.Height);
                    Color[] data = new Color[bodyText2D.Width * bodyText2D.Height];
                    bodyText2D.GetData(data, 0, data.Length);
                    pbe.cacheImage.SetData(data);

                    pbe.dirty = false;
                } 

                //Return the cached image
                return pbe.cacheImage;
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

        private static bool pre_Draw(FarmerRenderer __instance, ref Vector2 ___positionOffset, ref Vector2 ___rotationAdjustment, ref bool ____sickFrame, ref bool ____shirtDirty, ref bool ____spriteDirty, ref LocalizedContentManager ___farmerTextureManager, ref Dictionary<string, Dictionary<int, List<int>>> ____recolorOffsets, ref Texture2D ___baseTexture, ref string ___textureName, SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Vector2 origin, float layerDepth, int facingDirection, Color overrideColor, float rotation, float scale, Farmer who)
        {
            if (who.isFakeEventActor && Game1.eventUp)
            {
                who = Game1.player;
            }

            //Calculate if any sprites farmer sprite sections need redrawing
            PlayerBaseExtended pbe = PlayerBaseExtended.Get(who);
            if (pbe == null)
            {
                pbe = new PlayerBaseExtended(who, __instance.textureName.Value, -2, who.GetPantsIndex(), -2);
                SetModDataDefaults(who);
                pbe.dirty = true;
                
                if (who.accessory.Value < 6 && who.accessory.Value > 0)
                {
                    who.modData["DB.beard"] = "Vanilla's Accessory " + (who.accessory.Value + 1).ToString();
                    who.accessory.Set(0);
                }
            }

            //Overriding the texture loading didn't apply during construction
            if (!pbe.overrideCheck)
            {
                LocalizedContentManagerOverride lcmo = ___farmerTextureManager as LocalizedContentManagerOverride;
                if (lcmo == null)
                {
                    lcmo = new LocalizedContentManagerOverride(___farmerTextureManager.ServiceProvider, ___farmerTextureManager.RootDirectory);
                    ___farmerTextureManager = (LocalizedContentManager)lcmo.CreateTemporary(context, who);
                }
                pbe.overrideCheck = true;
            }

            //Flat the positions to whole pixels
            position.X = (int)position.X;
            position.Y = (int)position.Y;
            ___positionOffset.Y = (int)___positionOffset.X;
            ___positionOffset.X = (int)___positionOffset.Y;

            //TODO move this dirty texture check to not run so often
            pbe.UpdateTextures(who);

            if (pbe.dirty)
            {
                //Redraw the image
                pbe.cacheImage = null;
                //Wipe the skin colour calculations
                ____recolorOffsets = null;
                //Wipe hair calculations
                pbe.ResetHairTextures();
                //Wipe naked overlays
                pbe.nakedLower.texture = null;
                pbe.nakedUpper.texture = null;
                //recalculate the recolouring parts
                __instance.MarkSpriteDirty();
            }

            //Draw the character
            bool sick_frame = currentFrame == 104 || currentFrame == 105;
            if (____sickFrame != sick_frame)
            {
                ____sickFrame = sick_frame;
                ____shirtDirty = true;
                ____spriteDirty = true;
            }

            ExecuteRecolorActionsReversePatch(__instance, who);
            AdjustedVanillaMethods.drawBase(__instance, ref ___rotationAdjustment, ref ___positionOffset, ref ___baseTexture, b, animationFrame, currentFrame, ref sourceRect, ref position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            DrawBodyHair(__instance, ___positionOffset, ___rotationAdjustment, ___baseTexture, animationFrame, sourceRect, b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);

            ///////////////////////////////
            /// Setup a new overlay drawing for upper body
            //no shirt
            bool drawNakedOverlay = who.shirtItem.Value == null;
            if (!drawNakedOverlay && who.bathingClothes.Value)
            {
                if (who.modData.ContainsKey("DB.bathers"))
                {
                    drawNakedOverlay = who.modData["DB.bathers"] == "false";
                }
            }

            if (drawNakedOverlay)
            {
                Texture2D nakedOverlayTexture = pbe.GetNakedUpperTexture(who.skin.Value);

                if (nakedOverlayTexture != null)
                {
                    Rectangle pants_rect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
                    float layerOffset = ((who.FarmerSprite.CurrentAnimationFrame.frame == 5) ? 0.00096f : 9.6E-08f);

                    if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming.Value)
                    {
                        //don't draw it in the water
                    }
                    else
                    {

                        if (FarmerRenderer.isDrawingForUI)
                        {
                            //Change the frame for UI version
                            sourceRect.X = 0;
                            sourceRect.Y = 0;
                            b.Draw(nakedOverlayTexture, position + origin + ___positionOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layerOffset);
                        }
                        else
                        {
                            b.Draw(nakedOverlayTexture, position + origin + ___positionOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layerOffset);
                        }
                    }
                }
            }


            bool drawPants = true;
            if (who.GetPantsIndex() == 14 || who.pantsItem.Value == null)
            {
                Texture2D nakedOverlayTexture = pbe.GetNakedLowerTexture(who.skin.Value);
                if (nakedOverlayTexture != null)
                {
                    drawPants = false;
                }
            }
            if (drawPants && who.bathingClothes.Value)
            {
                if (who.modData.ContainsKey("DB.bathers"))
                {
                    drawPants = who.modData["DB.bathers"] == "true";
                }
            }
            if(drawPants) AdjustedVanillaMethods.drawPants(__instance, ref ___rotationAdjustment, ref ___positionOffset, ref ___baseTexture, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            AdjustedVanillaMethods.drawEyes(__instance, ref ___rotationAdjustment, ref ___positionOffset, ref ___baseTexture, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            __instance.drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
            AdjustedVanillaMethods.drawArms(__instance, ref ___rotationAdjustment, ref ___positionOffset, ref ___baseTexture, b, animationFrame, currentFrame, sourceRect, position, origin, layerDepth, facingDirection, overrideColor, rotation, scale, who);
            //prevent further rendering
            return false;
        }

        internal static void ExecuteRecolorActionsReversePatch(FarmerRenderer __instance, Farmer who)
        {
            new NotImplementedException("It's a stub!");
        }


        private static void DrawBodyHair(FarmerRenderer __instance, Vector2 ___positionOffset, Vector2 ___rotationAdjustment, Texture2D ___baseTexture, FarmerSprite.AnimationFrame animationFrame, Rectangle sourceRect, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {
            PlayerBaseExtended pbe = PlayerBaseExtended.Get(who);

            if (!pbe.bodyHair.OptionMatchesModData(who))
            {
                pbe.bodyHair.SetOptionFromModData(who, bodyHairOptions);
            }

            if (pbe.bodyHair.option != "Default")
            {
                //Draw the body hair
                b.Draw(pbe.GetBodyHairTexture(who), position + origin + ___positionOffset, sourceRect, Color.White, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + ((who.FarmerSprite.CurrentAnimationFrame.frame == 5) ? 0.00072f : 7.2E-08f));
            }
        }

        private static bool pre_DrawHairAndAccesories(FarmerRenderer __instance, bool ___isDrawingForUI, Vector2 ___positionOffset, Vector2 ___rotationAdjustment, LocalizedContentManager ___farmerTextureManager, Texture2D ___baseTexture, NetInt ___skin, bool ____sickFrame, ref Rectangle ___hairstyleSourceRect, ref Rectangle ___shirtSourceRect, ref Rectangle ___accessorySourceRect, ref Rectangle ___hatSourceRect, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {

            if (b != null)
            {

                int sort_direction = ((!Game1.isUsingBackToFrontSorting) ? 1 : (-1));

                //Get details that the standard Draw method uses
                FarmerSprite.AnimationFrame animationFrame = who.FarmerSprite.CurrentAnimationFrame;
                Rectangle sourceRect = who.FarmerSprite.SourceRect;

                //Vector2 positionOffset = Vector2.Zero;
                //positionOffset.Y = (int)(animationFrame.positionOffset * 4f);
                //positionOffset.X = (int)(animationFrame.xOffset * 4f);

                PlayerBaseExtended pbe = PlayerBaseExtended.Get(who);

                //Draw the shirts
                if (!who.bathingClothes.Value && who.shirtItem.Get() != null)
                {
                    AdjustedVanillaMethods.DrawShirt(__instance, FarmerRenderer.shirtsTexture,  ___positionOffset, ___rotationAdjustment, ref ___shirtSourceRect, b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);

                    //layerDepth += 1.4E-07f;
                    if (who.modData.ContainsKey("DB.overallColor") && who.modData["DB.overallColor"] == "true")
                    {
                        if (who.pants.Value != 14 && who.pantsItem.Get() != null && who.pantsItem.Get().dyeable)
                        {
                            //Draw the tinted overalls/highwaisted pants
                            Texture2D overalls_texture = Game1.content.Load<Texture2D>("Mods/ribeena.dynamicbodies/assets/Character/shirts_overlay.png");

                            if (pbe.shirtOverlayIndex >= 0)
                            {
                                AdjustedVanillaMethods.DrawShirt(__instance, overalls_texture, ___positionOffset, ___rotationAdjustment, ref ___shirtSourceRect, b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetPantsColor()) : overrideColor, layerDepth + 1.8E-07f + 1.4E-07f, false, pbe.shirtOverlayIndex);
                                
                            }
                            else {
                                
                                AdjustedVanillaMethods.DrawShirt(__instance, overalls_texture, ___positionOffset, ___rotationAdjustment, ref ___shirtSourceRect, b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetPantsColor()) : overrideColor, layerDepth + 1.8E-07f + 1.4E-07f, false);
                            }
                        }
                    }
                }

                //Draw the beards
                Texture2D beardTexture = null;
                if (!pbe.beard.OptionMatchesModData(who))
                {
                    pbe.beard.SetOptionFromModData(who, beardOptions);
                } else
                {
                    int accessory_id = (int)who.accessory.Value;
                    if (accessory_id >= 0 && accessory_id < 6)
                    {
                        //Move beard setting
                        pbe.beard.file = accessory_id.ToString();
                        pbe.beard.provider = null;
                        who.changeAccessory(-1);
                    }
                }

                if (pbe.beard.option != "Default")
                {
                    Rectangle accessorySourceRect;
                    int beardheight = 16;
                    if (pbe.beard.provider == null)
                    {
                        int accessory_id = (int)who.accessory.Value;
                        if (pbe.beard.file.Length == 1)
                        {
                            accessory_id = int.Parse(pbe.beard.file);
                        }
                        //get the accessory from vanilla
                        beardTexture = pbe.GetBeardTexture(who, accessory_id, FarmerRenderer.accessoriesTexture, new Rectangle(16 * accessory_id, 0, 16, 32));
                        accessorySourceRect = new Rectangle(0, 0, 16, 16);
                    } else
                    {
                        //get the content pack
                        beardTexture = pbe.GetBeardTexture(who);
                        accessorySourceRect = new Rectangle(0, 0, 16, 32);
                        beardheight = 32;
                    }
                    //crop the beard if they are swimming
                    if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming.Value)
                    {
                        accessorySourceRect.Height = 16;
                    }

                    switch (facingDirection)
                    {
                        case 0:
                            if(accessorySourceRect.Height == 32) {
                                accessorySourceRect.Y = accessorySourceRect.Height*2;
                                b.Draw(beardTexture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)__instance.heightOffset.Value), accessorySourceRect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.95E-05f);
                            }
                            break;
                        case 1:
                            accessorySourceRect.Y = beardheight;
                            b.Draw(beardTexture, position + origin + ___positionOffset + ___rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)__instance.heightOffset.Value), accessorySourceRect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.95E-05f);
                            break;
                        case 2:
                            b.Draw(beardTexture, position + origin + ___positionOffset + ___rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)__instance.heightOffset.Value - 4), accessorySourceRect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.95E-05f);
                            break;
                        case 3:
                            accessorySourceRect.Y = beardheight;
                            b.Draw(beardTexture, position + origin + ___positionOffset + ___rotationAdjustment + new Vector2(-FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)__instance.heightOffset.Value), accessorySourceRect, Color.White, rotation, origin, 4f * scale, SpriteEffects.FlipHorizontally, layerDepth + 1.95E-05f);
                            break;
                    }
                } 

                //Draw Vanilla accessories
                if ((int)who.accessory.Value >= 6)
                {
                    AdjustedVanillaMethods.DrawAccessory(__instance, ___positionOffset, ___rotationAdjustment, ref ___accessorySourceRect, b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth);
                }

                ///////////////////////////////
                /// Setup overlay for rendering new two tone hair
                /// 
                int hair_style = who.getHair(); // (ignore_hat: true);
                HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);
                if (who != null && who.hat.Value != null && who.hat.Value.hairDrawType.Value == 1 && hair_metadata != null && hair_metadata.coveredIndex != -1)
                {
                    hair_style = hair_metadata.coveredIndex;
                    hair_metadata = Farmer.GetHairStyleMetadata(hair_style);
                }
                Rectangle hairstyleSourceOriginalRect = new Rectangle(hair_style * 16 % FarmerRenderer.hairStylesTexture.Width, hair_style * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32 * 3);

                Texture2D hair_texture;
                if (hair_metadata != null)
                {
                    hairstyleSourceOriginalRect = new Rectangle(hair_metadata.tileX * 16, hair_metadata.tileY * 16, 16, 32);
                    if (hair_metadata.usesUniqueLeftSprite)
                    {
                        hairstyleSourceOriginalRect.Height = 32 * 4;
                    }
                    hair_texture = pbe.GetHairTexture(who, hair_style, hair_metadata.texture, hairstyleSourceOriginalRect);
                }
                else
                {
                    hair_texture = pbe.GetHairTexture(who, hair_style, FarmerRenderer.hairStylesTexture, hairstyleSourceOriginalRect);
                }

                //Cached and recoloured hair only has the one version
                Rectangle hairstyleSourceRect = new Rectangle(0, 0, 16, 32);

                float hair_draw_layer = hairlayer; //2.25E-05f //2.2E-05f;

                //float base_layer = layerDepth;

                if (FarmerRenderer.isDrawingForUI)
                {
                    //hair_draw_layer = 1.15E-07f;
                    layerDepth = 0.7f;
                    //context.Monitor.Log($"UI layer is [{layerDepth}].", LogLevel.Debug);
                    facingDirection = 2;

                }

                bool flip = false;
                switch (facingDirection)
                {
                    case 0:
                        hairstyleSourceRect.Offset(0, 64);
                        b.Draw(hair_texture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 4 + ((who.IsMale && hair_style >= 16) ? (-4) : ((!who.IsMale && hair_style < 16) ? 4 : 0))), hairstyleSourceRect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer * (float)sort_direction);
                        break;
                    case 1:
                        hairstyleSourceRect.Offset(0, 32);
                        b.Draw(hair_texture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair.Value >= 16) ? (-4) : ((!who.IsMale && (int)who.hair.Value < 16) ? 4 : 0))), hairstyleSourceRect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer * (float)sort_direction);

                        break;
                    case 2:
                        b.Draw(hair_texture, position + origin + ___positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair.Value >= 16) ? (-4) : ((!who.IsMale && (int)who.hair.Value < 16) ? 4 : 0))), hairstyleSourceRect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer * (float)sort_direction);
                        break;
                    case 3:
                        flip = true;
                        if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
                        {
                            flip = false;
                            hairstyleSourceRect.Offset(0, 96);
                        }
                        else
                        {
                            hairstyleSourceRect.Offset(0, 32);
                        }
                        b.Draw(hair_texture, position + origin + ___positionOffset + new Vector2(-FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair.Value >= 16) ? (-4) : ((!who.IsMale && (int)who.hair.Value < 16) ? 4 : 0))), hairstyleSourceRect, Color.White, rotation, origin, 4f * scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + hair_draw_layer * (float)sort_direction);
                        break;
                }

                ///////////////////////////////
                /// Setup a new overlay drawing for lower body
                //no pants
                bool drawNakedOverlay = who.GetPantsIndex() == 14 || who.pantsItem.Value == null;
                if (!drawNakedOverlay && who.bathingClothes.Value)
                {
                    if (who.modData.ContainsKey("DB.bathers"))
                    {
                        drawNakedOverlay = who.modData["DB.bathers"] == "false";
                    }
                }

                if (drawNakedOverlay)
                {
                    Texture2D nakedOverlayTexture = pbe.GetNakedLowerTexture(who.skin.Value);

                    if (nakedOverlayTexture != null)
                    {
                        Rectangle pants_rect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
                        //below Fasion Sense's pants layer, though doubt this will be compatible
                        float layerOffset = FS_pantslayer;
                        //float FS_pantslayer = 0.009E-05f;
                        if (who.getFacingDirection() == 2)
                        {
                            layerOffset = 5.95E-05f;//above arms when facing forward
                        }

                        if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming.Value)
                        {
                            //don't draw it in the water
                        }
                        else
                        {

                            if (FarmerRenderer.isDrawingForUI)
                            {
                                //Change the frame for UI version
                                sourceRect.X = 0;
                                sourceRect.Y = 0;

                                float layerFix = 2E-05f + 3E-05f;
                                b.Draw(nakedOverlayTexture, position + origin + ___positionOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (layerOffset + layerFix) * (float)sort_direction);
                            }
                            else
                            {
                                b.Draw(nakedOverlayTexture, position + origin + ___positionOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + layerOffset);
                            }
                        }
                    }
                }

                //Draw the Vanilla hat
                if (who.hat.Value != null && !who.bathingClothes.Value)
                {
                    AdjustedVanillaMethods.DrawHat(__instance, ___positionOffset, ref ___hatSourceRect, b, facingDirection, who, position, origin, scale, currentFrame, rotation, layerDepth);
                }

            }
            //prevent further rendering
            return false;

        }

        public static bool pre_drawMiniPortrat(FarmerRenderer __instance, ref Texture2D ___baseTexture, SpriteBatch b, Vector2 position, float layerDepth, float scale, int facingDirection, Farmer who)
        {
            //Stick to a pixel
            position.X = (int)position.X;
            position.Y = (int)position.Y;

            PlayerBaseExtended pbe = PlayerBaseExtended.Get(who);
            //this.executeRecolorActions(who);
            //facingDirection = 2;
            //bool flip = false;
            //int yOffset = 0;
            int feature_y_offset = FarmerRenderer.featureYOffsetPerFrame[0];

            //Draw the base
            b.Draw(___baseTexture, position+ new Vector2(0f, (who.IsMale ? 0 : -4)) * scale / 4f, new Rectangle(0, 0, 16, who.isMale ? 15 : 16), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
            int sort_direction = ((!Game1.isUsingBackToFrontSorting) ? 1 : (-1));

            //Draw the beards
            Texture2D beardTexture = null;
            if (pbe.beard.option != "Default")
            {
                Rectangle accessorySourceRect;
                if (pbe.beard.provider == null)
                {
                    int accessory_id = (int)who.accessory.Value;
                    if (pbe.beard.file.Length == 1)
                    {
                        accessory_id = int.Parse(pbe.beard.file);
                    }
                    //get the accessory from vanilla
                    beardTexture = pbe.GetBeardTexture(who, accessory_id, FarmerRenderer.accessoriesTexture, new Rectangle(16 * accessory_id, 0, 16, 32));
                    accessorySourceRect = new Rectangle(0, 0, 16, 16);
                }
                else
                {
                    //get the content pack
                    beardTexture = pbe.GetBeardTexture(who);
                    accessorySourceRect = new Rectangle(0, 0, 16, 32);
                }
                //crop the beard for the mini
                accessorySourceRect.Height = who.isMale ? 15 : 16;

                b.Draw(beardTexture, position + new Vector2(0f, feature_y_offset * 4 + 4) * scale / 4f, accessorySourceRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + 1.1E-08f * (float)sort_direction);
            }

            //Draw Vanilla accessories
            if ((int)who.accessory.Value >= 6)
            {
                Rectangle accessorySourceRect = new Rectangle((int)who.accessory.Value * 16 % FarmerRenderer.accessoriesTexture.Width, (int)who.accessory.Value * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16);
                accessorySourceRect.Height = who.isMale ? 15 : 16;

                b.Draw(FarmerRenderer.accessoriesTexture, position + new Vector2(0f, feature_y_offset * 4 + 4) * scale / 4f, accessorySourceRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + 0.9E-07f * (float)sort_direction);

            }

            //Draw the hair
            int hair_style = who.getHair(ignore_hat: true);
            HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);
            
            Texture2D hair_texture;
            Rectangle hairstyleSourceOriginalRect = new Rectangle(hair_style * 16 % FarmerRenderer.hairStylesTexture.Width, hair_style * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32 * 3);
            
            if (hair_metadata != null)
            {
                hairstyleSourceOriginalRect = new Rectangle(hair_metadata.tileX * 16, hair_metadata.tileY * 16, 16, 32);
                hair_texture = pbe.GetHairTexture(who, hair_style, hair_metadata.texture, hairstyleSourceOriginalRect);
            }
            else
            {
                hair_texture = pbe.GetHairTexture(who, hair_style, FarmerRenderer.hairStylesTexture, hairstyleSourceOriginalRect);
            }
            //Cached hair just has the one style, and we only need the top part of the hair (15 pixels)
            Rectangle hairstyleSourceRect = new Rectangle(0, 0, 16, 15);

            b.Draw(hair_texture, position + new Vector2(0f, (who.IsMale ? 0 : -4)) * scale / 4f + new Vector2(0f, feature_y_offset * 4 + ((who.IsMale && (int)who.hair >= 16) ? (-4) : ((!who.IsMale && (int)who.hair < 16) ? 4 : 0))) * scale / 4f, hairstyleSourceRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + 1.1E-07f * (float)sort_direction);
            //prevent further rendering
            return false;
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
                foreach(string key in engb.strings.Keys) { debugmsg($"British check: {key}", LogLevel.Debug); }
                debugmsg($"British check: {engb.strings["pants_overlay_setting"]}", LogLevel.Debug);
            }

            //////////////////////////////////////////////
            // Add support for generic mod menu
            // get Generic Mod Config Menu's API (if it's installed)
            configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // read file
            //var model = this.Helper.Data.ReadSaveData<ModData>("dynamic.naked");

            // save file (if needed)
            //this.Helper.Data.WriteSaveData("dynamic.naked", model);

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => { },
                save: () => { }
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

    }

    
    

}
