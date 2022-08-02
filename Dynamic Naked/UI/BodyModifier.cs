using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.Menus;
using StardewValley;
using StardewModdingAPI;

using DynamicBodies.Data;

namespace DynamicBodies.UI
{
	internal class BodyModifier : IClickableMenu
	{

		public int cost = 0;
		private bool isWizardSubmenu = false;

		public const int region_okbutton = 505, region_backbutton = 81114, region_accLeft = 516, region_accRight = 517, region_directionLeft = 520, region_directionRight = 521;
		public const int region_hairLeft = 514, region_hairRight = 515, region_bodyLeft = 516, region_bodyRight = 517, region_faceLeft = 416, region_faceRight = 417;
		public const int region_armLeft = 518, region_armRight = 519, region_beardLeft = 520, region_beardRight = 521;
		public const int region_bodyHairLeft = 531, region_bodyHairRight = 532, region_nakedLeft = 533, region_nakedRight = 534, region_nakedLeftU = 535, region_nakedRightU = 536;

		public const int region_colorPicker1 = 522, region_colorPicker2 = 523, region_colorPicker3 = 524;
		public const int region_colorPicker4 = 525, region_colorPicker5 = 526, region_colorPicker6 = 527;
		public const int region_colorPicker7 = 528, region_colorPicker8 = 529, region_colorPicker9 = 530;

		public const int region_eyeSwatch1 = 701, region_eyeSwatch2 = 702, region_eyeSwatch3 = 703, region_eyeSwatch4 = 704;
		public const int region_hairSwatch1 = 705, region_hairSwatch2 = 706, region_hairSwatch3 = 707, region_hairSwatch4 = 708;
		public const int region_hairDarkSwatch1 = 709, region_hairDarkSwatch2 = 710, region_hairDarkSwatch3 = 711, region_hairDarkSwatch4 = 712;

		public const int region_nameBox = 536;

		public const int swatchsize = 40;

		public Farmer who;

		private PlayerBaseExtended pbe;

		private string hoverText;
		private string hoverTitle;

		public Dictionary<string, ClothingToggle> clothingToggles = new Dictionary<string, ClothingToggle>();
		public ClickableComponent hatButton;
		public ClickableComponent shirtButton;
		public ClickableComponent pantsButton;
		public ClickableComponent shoesButton;

		public List<ClickableComponent> labels = new List<ClickableComponent>();
		public List<ClickableComponent> swatches = new List<ClickableComponent>();
		public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();
		public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();

		private TextBox nameBox;
		public ClickableTextureComponent okButton;
		public ClickableTextureComponent backButton;
		private ClickableComponent costLabel;

		public ClickableComponent hairLabel;
		public ClickableComponent accLabel;
		public List<int> accessoryOptions;
		public ClickableComponent bodyLabel;
		public ClickableComponent faceLabel;
		public ClickableComponent armLabel;
		public ClickableComponent beardLabel;
		public ClickableComponent bodyHairLabel;
		public ClickableComponent nakedLabel;
		public ClickableComponent nakedULabel;

		private Dictionary<string, string> settingsBefore = new Dictionary<string, string>();

		public Rectangle CharacterBackgroundRect;

		public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();
		public ColorPicker hairColorPicker;
		public ColorPicker hairDarkColorPicker;
		private readonly Action _recolorHairAction;
		public ColorPicker eyeColorPicker;
		private readonly Action _recolorEyesAction;
		private int colorPickerTimer;
		private ColorPicker _sliderOpTarget;
		private ColorPicker lastHeldColorPicker;
		private Action _sliderAction;
		public const int colorPickerTimerDelay = 100;
		private Color initEyeColor;
		private Color initHairColor;

		private int doubleBackTimer;

		protected Farmer _displayFarmer;
		public Rectangle portraitBox;

		Texture2D UItexture;
		private int eyeSwatch = 0;
		public Color[] eyeSwatchColors;
		private int hairSwatch = 0;
		public Color[] hairSwatchColors;
		private int hairDarkSwatch = 0;
		public Color[] hairDarkSwatchColors;


		public BodyModifier(int windowHeight, bool wizardSub = false)
		: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (windowHeight + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, windowHeight + IClickableMenu.borderWidth * 2 + 64)
		{
			who = Game1.player;

			isWizardSubmenu = wizardSub;
			this.pbe = PlayerBaseExtended.Get(who);
			if (pbe == null)
			{
				pbe = new PlayerBaseExtended(who, "Characters\\Farmer\\farmer_base");
			}
			//Store settings for resetting
			initEyeColor = who.newEyeColor.Value;
			initHairColor = who.hairstyleColor.Value;
			settingsBefore["acc"] = who.accessory.ToString();
			settingsBefore["DB." + pbe.body.name] = pbe.body.GetOptionModData(who);
			settingsBefore["DB." + pbe.arm.name] = pbe.arm.GetOptionModData(who);
			settingsBefore["DB." + pbe.beard.name] = pbe.beard.GetOptionModData(who);
			
			settingsBefore["DB." + pbe.bodyHair.name] = pbe.bodyHair.GetOptionModData(who);
			settingsBefore["DB." + pbe.nakedLower.name] = pbe.nakedLower.GetOptionModData(who);
			settingsBefore["DB." + pbe.nakedUpper.name] = pbe.nakedUpper.GetOptionModData(who);
			if (who.modData.ContainsKey("DB.darkHair"))
			{
				settingsBefore["DB.darkHair"] = who.modData["DB.darkHair"];
			}
			else
			{
				settingsBefore["DB.darkHair"] = new Color(57, 57, 57).PackedValue.ToString();
			}

			this.accessoryOptions = new List<int> { 0, 1, 2, 3, 4, 5 };
			this.UItexture = Game1.content.Load<Texture2D>("Mods/ribeena.dynamicbodies/assets/Interface/ui.png");

			clothingToggles["hat"] = new ClothingToggle() { offset = 16 };
			clothingToggles["hat"].active = clothingToggles["hat"].showing = who.hat.Value != null;
			clothingToggles["shirt"] = new ClothingToggle() { offset = 32 };
			clothingToggles["shirt"].active = clothingToggles["shirt"].showing = who.shirtItem.Get() != null;
			clothingToggles["pants"] = new ClothingToggle() { offset = 48 };
			clothingToggles["pants"].active = clothingToggles["pants"].showing = who.GetPantsIndex() != 14;
			clothingToggles["shoes"] = new ClothingToggle() { offset = 64 };
			clothingToggles["shoes"].active = clothingToggles["shoes"].showing = who.shoes.Value != 12;

			this._recolorEyesAction = delegate
			{
				who.changeEyeColor(this.eyeColorPicker.getSelectedColor());
			};
			this._recolorHairAction = delegate
			{
				who.changeHairColor(this.hairColorPicker.getSelectedColor());
				who.modData["DB.darkHair"] = this.hairDarkColorPicker.getSelectedColor().PackedValue.ToString();
				ModEntry.MakePlayerDirty();
			};
			this._displayFarmer = this.GetOrCreateDisplayFarmer();


		}

		//Make translation easier
		static public string T(string toTranslate)
        {
			return ModEntry.translate(toTranslate);
		}

		//Run actions needed based on what is happening
		public override void update(GameTime time)
		{
			base.update(time);
		}
		//farmer for rendering
		public Farmer GetOrCreateDisplayFarmer()
		{
			if (this._displayFarmer == null)
			{
				this._displayFarmer = who;
				this._displayFarmer.faceDirection(2);
				this._displayFarmer.FarmerSprite.StopAnimation();
			}
			return this._displayFarmer;
		}
		public void setupGeneralPositions(string title)
		{
			int arrow_size = 64;

			this.labels.Clear();
			this.leftSelectionButtons.Clear();
			this.rightSelectionButtons.Clear();

			this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 505,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.backButton = new ClickableTextureComponent("Back", new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = region_backbutton,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};

			this.costLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 84-20, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 32, 1, 1), T("cost")+": "+(cost == 0 ? T("free") : cost));
			this.labels.Add(costLabel);

			//Draw the farmer
			this.portraitBox = new Rectangle(base.xPositionOnScreen + 64 + 4, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 128, 192);
			
			//Arrows to change farmers direction
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.X - arrow_size / 2, this.portraitBox.Y + 144, arrow_size, arrow_size), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = region_directionLeft,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			this.rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.Right - arrow_size / 2, this.portraitBox.Y + 144, arrow_size, arrow_size), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = region_directionRight,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			//Items next to portrait box
			this.nameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = portraitBox.X + portraitBox.Width + 10 * 4,
				Y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16,
				Width = 104 * 4,
				Text = title+who.Name
			};

			int yOffset = 32;
			int label_col2_position = portraitBox.X + portraitBox.Width + 12 * 4;
			//Clothing toggle buttons
			foreach (KeyValuePair<string, ClothingToggle> ctkv in clothingToggles)
			{
				ctkv.Value.clickable = new ClickableComponent(new Rectangle(label_col2_position + (ctkv.Value.offset - 16) * 2 + (ctkv.Value.offset / 16) * 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 16 * 2, 16 * 2), ctkv.Key)
				{
					myID = 600 + ctkv.Value.offset / 16,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				};
				switch (ctkv.Key)
				{
					case "hat":
						hatButton = ctkv.Value.clickable;
						break;
					case "shirt":
						shirtButton = ctkv.Value.clickable;
						break;
					case "pants":
						pantsButton = ctkv.Value.clickable;
						break;
					case "shoes":
						shoesButton = ctkv.Value.clickable;
						break;
				}
			}
		}

		public virtual void setUpPositions()
        {
			//Override how this
        }

		//Close the window
		public override bool readyToClose()
		{
			RevertClothing();
			//Add close prevention
			return base.readyToClose();
		}

		private void RevertChanges()
        {
			PlayerBaseExtended pbe = PlayerBaseExtended.Get(who);
			who.changeEyeColor(initEyeColor);
			who.changeHairColor(initHairColor);
			who.modData["DB.darkHair"] = settingsBefore["DB.darkHair"];
			who.accessory.Set(Int32.Parse(settingsBefore["acc"]));

			who.modData["DB." + pbe.body.name] = settingsBefore["DB." + pbe.body.name];
			who.modData["DB." + pbe.arm.name] = settingsBefore["DB." + pbe.arm.name];
			who.modData["DB." + pbe.beard.name] = settingsBefore["DB." + pbe.beard.name];
			who.modData["DB." + pbe.bodyHair.name] = settingsBefore["DB." + pbe.bodyHair.name];
			who.modData["DB." + pbe.nakedLower.name] = settingsBefore["DB." + pbe.nakedLower.name];
			who.modData["DB." + pbe.nakedUpper.name] = settingsBefore["DB." + pbe.nakedUpper.name];
			pbe.dirty = true;
		}

		private void RevertClothing()
        {
			if (!clothingToggles["pants"].showing)
			{
				if (clothingToggles["pants"].storeItem != null)
				{
					who.pantsItem.Set(clothingToggles["pants"].storeItem as Clothing);
				} else
                {
					who.changePantStyle(clothingToggles["pants"].store);
				}
				clothingToggles["pants"].showing = true;
			}

			if (!clothingToggles["shoes"].showing)
			{

				who.shoes.Set(clothingToggles["shoes"].store);
				Game1.playSound("pickUpItem");

				clothingToggles["shoes"].showing = true;
			}

			if (!clothingToggles["shirt"].showing)
			{
				who.shirtItem.Set(clothingToggles["shirt"].storeItem as Clothing);

				clothingToggles["shirt"].showing = true;

			}

			if (!clothingToggles["hat"].showing)
			{

				who.hat.Set(clothingToggles["hat"].storeItem as Hat);
				clothingToggles["hat"].showing = true;

			}
		}
		public override void emergencyShutDown()
		{
			RevertClothing();
			//Handle any reverts as required
			base.emergencyShutDown();
		}

		/* not surehow automatic snapping works 
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (a.region != b.region)
			{
				return false;
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}
		protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
        {
            return true;
        }
        */

		protected override void cleanupBeforeExit()
		{
			RevertClothing();
		}

		public void cancelAndExit()
        {
			RevertChanges();
			if (isWizardSubmenu)
			{
				Game1.activeClickableMenu = new WizardCharacterCharacterCustomization();
				Game1.playSound("shwip");
			}
			else
			{
				this.exitThisMenu(false);
				Game1.playSound("cancel");
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			base.currentlySnappedComponent = base.getComponentWithID(region_directionRight);
			this.snapCursorToCurrentSnappedComponent();
		}
		private void optionButtonClick(string name)
		{

			switch (name)
			{
				case "OK":
					{
						RevertClothing();
						who.isCustomized.Value = true;
						// Declare beauty change
						/*if (source == Source.Doctors)
						{
							Game1.multiplayer.globalChatInfoMessage("Makeover", who.Name+" got some surgery.");
						}*/
						
						if (isWizardSubmenu)
						{
							Game1.activeClickableMenu = new WizardCharacterCharacterCustomization();
							Game1.playSound("shwip");
						}
						else
						{
							who.Money -= cost;
							this.exitThisMenu(false);
							Game1.playSound("purchase");
						}
						break;
					}
				case "Back":
					{

						cancelAndExit();
						break;
					}

			}
			
		}
		//Handle click particular items
		private void selectionClick(string name, int change)
		{
			switch (name)
			{
				case "Hair":
					{
						List<int> all_hairs = Farmer.GetAllHairstyleIndices();
						int current_index = all_hairs.IndexOf(who.hair.Value);
						current_index += change;
						if (current_index >= all_hairs.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_hairs.Count() - 1;
						}
						who.changeHairStyle(all_hairs[current_index]);
						//Update the base file recording as needed
						pbe.SetVanillaFile(who.getTexture());
						ModEntry.MakePlayerDirty();
						Game1.playSound("grassyStep");
						break;
					}
				case "Body":
					{
						List<string> all_bodys = ModEntry.getContentPackOptions(ModEntry.bodyOptions, who.IsMale).ToList();
						int current_index = all_bodys.IndexOf((who.modData.ContainsKey("DB.body")) ? who.modData["DB.body"] : "Default");
						current_index += change;
						if (current_index >= all_bodys.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_bodys.Count() - 1;
						}
						pbe.SetModData(who, "DB.body", all_bodys[current_index]);

						Game1.playSound("grassyStep");
						break;
					}
				case "Face":
					{
						List<string> all_faces = ModEntry.getContentPackOptions(ModEntry.faceOptions, who.IsMale).ToList();
						int current_index = all_faces.IndexOf((who.modData.ContainsKey("DB.face")) ? who.modData["DB.face"] : "Default");
						current_index += change;
						if (current_index >= all_faces.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_faces.Count() - 1;
						}
						pbe.SetModData(who, "DB.face", all_faces[current_index]);

						Game1.playSound("grassyStep");
						break;
					}
				case "Arm":
					{
						List<string> all_arms = ModEntry.getContentPackOptions(ModEntry.armOptions, who.IsMale).ToList();
						int current_index = all_arms.IndexOf((who.modData.ContainsKey("DB.arm")) ? who.modData["DB.arm"] : "Default");
						current_index += change;
						if (current_index >= all_arms.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_arms.Count() - 1;
						}
						pbe.SetModData(who, "DB.arm", all_arms[current_index]);

						Game1.playSound("grassyStep");
						break;
					}
				case "Beard":
					{
						List<string> all_beards = ModEntry.getContentPackOptions(ModEntry.beardOptions, who.IsMale).ToList();
						int current_index = all_beards.IndexOf((who.modData.ContainsKey("DB.beard")) ? who.modData["DB.beard"] : "Default");
						current_index += change;
						if (current_index >= all_beards.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_beards.Count() - 1;
						}
						pbe.SetModData(who, "DB.beard", all_beards[current_index]);

						Game1.playSound("grassyStep");
						break;
					}
				case "BodyHair":
					{
						List<string> all_bh = ModEntry.getContentPackOptions(ModEntry.bodyHairOptions, who.IsMale).ToList();
						int current_index = all_bh.IndexOf((who.modData.ContainsKey("DB.bodyHair")) ? who.modData["DB.bodyHair"] : "Default");
						current_index += change;
						if (current_index >= all_bh.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_bh.Count() - 1;
						}
						pbe.SetModData(who, "DB.bodyHair", all_bh[current_index]);
						
						Game1.playSound("grassyStep");
						break;
					}
				case "Naked":
					{
						List<string> all_no = ModEntry.getContentPackOptions(ModEntry.nudeLOptions, who.IsMale).ToList();
						int current_index = all_no.IndexOf((who.modData.ContainsKey("DB.nakedLower")) ? who.modData["DB.nakedLower"] : "Default");
						current_index += change;
						if (current_index >= all_no.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_no.Count() - 1;
						}
						pbe.SetModData(who, "DB.nakedLower", all_no[current_index]);

						Game1.playSound("grassyStep");
						break;
					}
				case "NakedU":
					{
						List<string> all_no = ModEntry.getContentPackOptions(ModEntry.nudeUOptions, who.IsMale).ToList();
						int current_index = all_no.IndexOf((who.modData.ContainsKey("DB.nakedUpper")) ? who.modData["DB.nakedUpper"] : "Default");
						current_index += change;
						if (current_index >= all_no.Count)
						{
							current_index = 0;
						}
						else if (current_index < 0)
						{
							current_index = all_no.Count() - 1;
						}
						pbe.SetModData(who, "DB.nakedUpper", all_no[current_index]);

						Game1.playSound("grassyStep");
						break;
					}
				case "Acc":
					int newacc = (int)who.accessory.Value;
					//Skip the beards
					if (newacc >= 0 && newacc <= 6 && change < 0)
					{
						newacc = -1;
					} else if (newacc >= 18 && change > 0)
					{
						newacc = -1;
					} else if (newacc == -1 && change > 0) {
						newacc = 6;
					} else
					{
						newacc += change;
					}
					who.changeAccessory(newacc);
					Game1.playSound("purchase");
					break;
				case "Direction":
					this._displayFarmer.faceDirection((this._displayFarmer.FacingDirection - change + 4) % 4);
					this._displayFarmer.FarmerSprite.StopAnimation();
					this._displayFarmer.completelyStopAnimatingOrDoingAction();
					Game1.playSound("pickUpItem");
					break;
			}
		}

		private void swatchClick(string name)
		{
			int change = 0;
            if (name.EndsWith("Left")) { change = -1; } else { 
				change = int.Parse(name[^1..]);
			}

			if (change != 0)
			{
				if (name.StartsWith("Eye"))
				{
					//cycle through 3 swatches

					//Special cases
					if (eyeSwatch == 0 && change < 0)
					{
						eyeSwatch = eyeSwatchColors.Length;
						change = 0;
					}

					if (eyeSwatch == eyeSwatchColors.Length && change > 0)
					{
						eyeSwatch = change - 1;
						change = 0;
					}

					//Other cases
					eyeSwatch += change;

					if (eyeSwatch != 0)
					{
						who.changeEyeColor(eyeSwatchColors[eyeSwatch - 1]);
					}
					else
					{
						who.changeEyeColor(initEyeColor);
					}

					Game1.playSound("purchase");
				}

				if (name.StartsWith("Hair"))
				{
					//Special cases
					if (hairSwatch == 0 && change < 0)
					{
						hairSwatch = hairSwatchColors.Length;
						change = 0;
					}

					if (hairSwatch == hairSwatchColors.Length && change > 0)
					{
						hairSwatch = change - 1;
						change = 0;
					}

					//Other cases
					hairSwatch += change;

					if (hairSwatch != 0)
					{
						who.changeHairColor(hairSwatchColors[hairSwatch - 1]);
						pbe.CheckHairTextures(who);
					}
					else
					{
						who.changeHairColor(initHairColor);
						pbe.CheckHairTextures(who);
					}

					Game1.playSound("purchase");
				}

				if (name.StartsWith("DarkHair"))
				{
					//Special cases
					if (hairDarkSwatch == 0 && change < 0)
					{
						hairDarkSwatch = hairDarkSwatchColors.Length;
						change = 0;
					}

					if (hairDarkSwatch == hairDarkSwatchColors.Length && change > 0)
					{
						hairDarkSwatch = change - 1;
						change = 0;
					}

					//Other cases
					hairDarkSwatch += change;

					if (hairDarkSwatch != 0)
					{
						pbe.SetModData(who, "DB.darkHair", hairDarkSwatchColors[hairDarkSwatch - 1].PackedValue.ToString());
					}
					else
					{
						pbe.SetModData(who, "DB.darkHair", settingsBefore["DB.darkHair"]);
					}

					Game1.playSound("purchase");
				}
			}
		}
		//Handle gamepad pressed
		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
			if (base.currentlySnappedComponent == null)
			{
				return;
			}
			switch (b)
			{
				case Buttons.DPadRight:
				case Buttons.LeftThumbstickRight:
					switch (base.currentlySnappedComponent.myID)
					{
						case region_colorPicker1:
							this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
							this.eyeColorPicker.changeHue(1);
							this.eyeColorPicker.Dirty = true;
							this._sliderOpTarget = this.eyeColorPicker;
							this._sliderAction = this._recolorEyesAction;
							break;
						case region_colorPicker2:
							this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
							this.eyeColorPicker.changeSaturation(1);
							this.eyeColorPicker.Dirty = true;
							this._sliderOpTarget = this.eyeColorPicker;
							this._sliderAction = this._recolorEyesAction;
							break;
						case region_colorPicker3:
							this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
							this.eyeColorPicker.changeValue(1);
							this.eyeColorPicker.Dirty = true;
							this._sliderOpTarget = this.eyeColorPicker;
							this._sliderAction = this._recolorEyesAction;
							break;
						case region_colorPicker4:
							this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
							this.hairColorPicker.changeHue(1);
							this.hairColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker5:
							this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
							this.hairColorPicker.changeSaturation(1);
							this.hairColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker6:
							this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
							this.hairColorPicker.changeValue(1);
							this.hairColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker7:
							this.hairDarkColorPicker.LastColor = this.hairDarkColorPicker.getSelectedColor();
							this.hairDarkColorPicker.changeHue(1);
							this.hairDarkColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairDarkColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker8:
							this.hairDarkColorPicker.LastColor = this.hairDarkColorPicker.getSelectedColor();
							this.hairDarkColorPicker.changeSaturation(1);
							this.hairColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairDarkColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker9:
							this.hairDarkColorPicker.LastColor = this.hairDarkColorPicker.getSelectedColor();
							this.hairDarkColorPicker.changeValue(1);
							this.hairDarkColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairDarkColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
					}
					break;
				case Buttons.DPadLeft:
				case Buttons.LeftThumbstickLeft:
					switch (base.currentlySnappedComponent.myID)
					{
						case region_colorPicker1:
							this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
							this.eyeColorPicker.changeHue(-1);
							this.eyeColorPicker.Dirty = true;
							this._sliderOpTarget = this.eyeColorPicker;
							this._sliderAction = this._recolorEyesAction;
							break;
						case region_colorPicker2:
							this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
							this.eyeColorPicker.changeSaturation(-1);
							this.eyeColorPicker.Dirty = true;
							this._sliderOpTarget = this.eyeColorPicker;
							this._sliderAction = this._recolorEyesAction;
							break;
						case region_colorPicker3:
							this.eyeColorPicker.LastColor = this.eyeColorPicker.getSelectedColor();
							this.eyeColorPicker.changeValue(-1);
							this.eyeColorPicker.Dirty = true;
							this._sliderOpTarget = this.eyeColorPicker;
							this._sliderAction = this._recolorEyesAction;
							break;
						case region_colorPicker4:
							this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
							this.hairColorPicker.changeHue(-1);
							this.hairColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker5:
							this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
							this.hairColorPicker.changeSaturation(-1);
							this.hairColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker6:
							this.hairColorPicker.LastColor = this.hairColorPicker.getSelectedColor();
							this.hairColorPicker.changeValue(-1);
							this.hairColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker7:
							this.hairDarkColorPicker.LastColor = this.hairDarkColorPicker.getSelectedColor();
							this.hairDarkColorPicker.changeHue(-1);
							this.hairDarkColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairDarkColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker8:
							this.hairDarkColorPicker.LastColor = this.hairDarkColorPicker.getSelectedColor();
							this.hairDarkColorPicker.changeSaturation(-1);
							this.hairDarkColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairDarkColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
						case region_colorPicker9:
							this.hairDarkColorPicker.LastColor = this.hairDarkColorPicker.getSelectedColor();
							this.hairDarkColorPicker.changeValue(-1);
							this.hairDarkColorPicker.Dirty = true;
							this._sliderOpTarget = this.hairDarkColorPicker;
							this._sliderAction = this._recolorHairAction;
							break;
					}
					break;
				//Jumpt to the exit button when pressing b
				case Buttons.B:
					if (base.currentlySnappedComponent.myID == region_backbutton)
					{
						this.doubleBackTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
						if (this.doubleBackTimer <= 0)
						{
							cancelAndExit();
						}
					}
					else
					{
						doubleBackTimer = Game1.currentGameTime.ElapsedGameTime.Milliseconds + 200;
						base.currentlySnappedComponent = base.getComponentWithID(region_backbutton);
						this.snapCursorToCurrentSnappedComponent();
					}
					break;
			}
		}
		//Handle gamepad input
		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (base.currentlySnappedComponent == null)
			{
				return;
			}
			switch (b)
			{
				case Buttons.RightTrigger:
					{
						int myID = base.currentlySnappedComponent.myID;
						if ((uint)(myID - 512) <= 9u)
						{
							this.selectionClick(base.currentlySnappedComponent.name, 1);
						}
						break;
					}
				case Buttons.LeftTrigger:
					{
						int myID = base.currentlySnappedComponent.myID;
						if ((uint)(myID - 512) <= 9u)
						{
							this.selectionClick(base.currentlySnappedComponent.name, -1);
						}
						break;
					}
			}
		}
		//Handle mouse down event
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.leftSelectionButtons.Count > 0)
			{
				foreach (ClickableComponent c2 in this.leftSelectionButtons)
				{
					if (c2.containsPoint(x, y))
					{
						this.selectionClick(c2.name, -1);
						if (c2.scale != 0f)
						{
							c2.scale -= 0.25f;
							c2.scale = Math.Max(0.75f, c2.scale);
						}
					}
				}
			}
			if (this.rightSelectionButtons.Count > 0)
			{
				foreach (ClickableComponent c in this.rightSelectionButtons)
				{
					if (c.containsPoint(x, y))
					{
						this.selectionClick(c.name, 1);
						if (c.scale != 0f)
						{
							c.scale -= 0.25f;
							c.scale = Math.Max(0.75f, c.scale);
						}
					}
				}
			}

			if (this.swatches.Count > 0)
			{
				foreach (ClickableComponent c in this.swatches)
				{
					if (c.containsPoint(x, y))
					{
						this.swatchClick(c.name);
						if (!c.name.EndsWith("0"))
						{
							if (c.scale != 0f)
							{
								c.scale -= 0.25f;
								c.scale = Math.Max(0.75f, c.scale);
							}
						}
					}
				}
			}

			if (this.okButton.containsPoint(x, y) && canPay())
			{
				this.optionButtonClick(this.okButton.name);
				this.okButton.scale -= 0.25f;
				this.okButton.scale = Math.Max(0.75f, this.okButton.scale);
			}

			if (this.backButton.containsPoint(x, y))
			{
				this.optionButtonClick(this.backButton.name);
				this.backButton.scale -= 0.25f;
				this.backButton.scale = Math.Max(0.75f, this.backButton.scale);
			}

			if (this.hairColorPicker != null && this.hairColorPicker.containsPoint(x, y))
			{
				Color color2 = this.hairColorPicker.click(x, y);
				who.changeHairColor(color2);
				this.lastHeldColorPicker = this.hairColorPicker;
			} else if (this.hairDarkColorPicker != null && this.hairDarkColorPicker.containsPoint(x, y))
			{
				Color color2 = this.hairDarkColorPicker.click(x, y);
				pbe.SetModData(who, "DB.darkHair", color2.PackedValue.ToString());
				this.lastHeldColorPicker = this.hairDarkColorPicker;
			} else if (this.eyeColorPicker != null && this.eyeColorPicker.containsPoint(x, y))
			{
				who.changeEyeColor(this.eyeColorPicker.click(x, y));
				this.lastHeldColorPicker = this.eyeColorPicker;
			}

			if (clothingToggles["pants"].active && clothingToggles["pants"].clickable.containsPoint(x, y))
			{
                if (clothingToggles["pants"].showing)
                {
					if (who.pantsItem.Value != null)
					{
						clothingToggles["pants"].storeItem = who.pantsItem.Value as Item;
						who.pantsItem.Set(null);
					}
					else
					{
						clothingToggles["pants"].store = who.GetPantsIndex();
						who.changePantStyle(14, false);
					}
					Game1.playSound("scissors");
				}
                else
                {
					if (clothingToggles["pants"].storeItem != null)
					{
						who.pantsItem.Set(clothingToggles["pants"].storeItem as Clothing);
					}
					else
					{
						who.changePantStyle(clothingToggles["pants"].store);
					}
					Game1.playSound("pickUpItem");
				}
				clothingToggles["pants"].showing = !clothingToggles["pants"].showing;
				
			}

			if (clothingToggles["shoes"].active && clothingToggles["shoes"].clickable.containsPoint(x, y))
			{
				if (clothingToggles["shoes"].showing)
				{
					clothingToggles["shoes"].store = who.shoes.Get();
					who.shoes.Set(12);
			
					Game1.playSound("scissors");
				}
				else
				{
					who.shoes.Set(clothingToggles["shoes"].store);
					Game1.playSound("pickUpItem");
				}
				clothingToggles["shoes"].showing = !clothingToggles["shoes"].showing;
				
			}

			if (clothingToggles["shirt"].active && clothingToggles["shirt"].clickable.containsPoint(x, y))
			{
				if (clothingToggles["shirt"].showing)
				{
					clothingToggles["shirt"].storeItem = who.shirtItem.Get() as Item;
					who.shirtItem.Set(null);
					Game1.playSound("scissors");
				}
				else
				{
					who.shirtItem.Set(clothingToggles["shirt"].storeItem as Clothing);
					Game1.playSound("pickUpItem");
				}
				clothingToggles["shirt"].showing = !clothingToggles["shirt"].showing;

			}

			if (clothingToggles["hat"].active && clothingToggles["hat"].clickable.containsPoint(x, y))
			{
				if (clothingToggles["hat"].showing)
				{
					clothingToggles["hat"].storeItem = who.hat.Get() as Item;
					who.hat.Set(null);
					Game1.playSound("scissors");
				}
				else
				{
					who.hat.Set(clothingToggles["hat"].storeItem as Hat);
					Game1.playSound("pickUpItem");
				}
				clothingToggles["hat"].showing = !clothingToggles["hat"].showing;

			}

			//string sound = "drumkit6";
			//Game1.playSound(sound);
		}

		//Handle the mouse down-held event on the colour picker
		public override void leftClickHeld(int x, int y)
		{
			this.colorPickerTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (this.colorPickerTimer > 0)
			{
				return;
			}
			if (this.lastHeldColorPicker != null && !Game1.options.SnappyMenus)
			{
				if (this.lastHeldColorPicker.Equals(this.hairColorPicker))
				{
					Color color2 = this.hairColorPicker.clickHeld(x, y);
					who.changeHairColor(color2);
					ModEntry.MakePlayerDirty();
				}

				if (this.lastHeldColorPicker.Equals(this.hairDarkColorPicker))
				{
					Color color2 = this.hairDarkColorPicker.clickHeld(x, y);
					//who.changeHairColor(color2);
					pbe.SetModData(who, "DB.darkHair", color2.PackedValue.ToString());
				}

				if (this.lastHeldColorPicker.Equals(this.eyeColorPicker))
				{
					who.changeEyeColor(this.eyeColorPicker.clickHeld(x, y));
				}
			}
			this.colorPickerTimer = 100;
		}

		//Handle mouse up event on the colour picker
		public override void releaseLeftClick(int x, int y)
		{
			if (this.hairColorPicker != null)
			{
				this.hairColorPicker.releaseClick();
			}

			if (this.hairDarkColorPicker != null)
			{
				this.hairDarkColorPicker.releaseClick();
			}

			if (this.eyeColorPicker != null)
			{
				this.eyeColorPicker.releaseClick();
			}
			this.lastHeldColorPicker = null;
		}

		//Provide tooltip information
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.hoverTitle = "";

			foreach (ClickableComponent swatch in this.swatches)
			{
				if (!swatch.name.EndsWith("0"))
				{
					if (swatch.containsPoint(x, y))
					{
						swatch.scale = Math.Min(swatch.scale + 0.02f, 1f + 0.1f);
					}
					else
					{
						swatch.scale = Math.Max(swatch.scale - 0.02f, 1f);
					}
				}
			}

			foreach (ClickableTextureComponent c6 in this.leftSelectionButtons)
			{
				if (c6.containsPoint(x, y))
				{
					c6.scale = Math.Min(c6.scale + 0.02f, c6.baseScale + 0.1f);
				}
				else
				{
					c6.scale = Math.Max(c6.scale - 0.02f, c6.baseScale);
				}
			}

			foreach (ClickableTextureComponent c5 in this.rightSelectionButtons)
			{
				if (c5.containsPoint(x, y))
				{
					c5.scale = Math.Min(c5.scale + 0.02f, c5.baseScale + 0.1f);
				}
				else
				{
					c5.scale = Math.Max(c5.scale - 0.02f, c5.baseScale);
				}
			}

			if (this.okButton.containsPoint(x, y) && this.canPay())
			{
				this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.1f);
			}
			else
			{
				this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
			}

			if (this.backButton.containsPoint(x, y))
			{
				this.backButton.scale = Math.Min(this.backButton.scale + 0.02f, this.backButton.baseScale + 0.1f);
			}
			else
			{
				this.backButton.scale = Math.Max(this.backButton.scale - 0.02f, this.backButton.baseScale);
			}

			if ((this.hairColorPicker != null && this.hairColorPicker.containsPoint(x, y))
				|| (this.hairDarkColorPicker != null && this.hairDarkColorPicker.containsPoint(x, y))
				|| (this.eyeColorPicker != null && this.eyeColorPicker.containsPoint(x, y)))
			{
				Game1.SetFreeCursorDrag();
			}

			foreach (KeyValuePair<string, ClothingToggle> ctkv in clothingToggles)
			{
				ctkv.Value.hover = ctkv.Value.clickable.containsPoint(x, y) && ctkv.Value.active;
				if(ctkv.Value.hover)
                {
					hoverText = T("toggle_show")+" " + T(ctkv.Key);
                }
			}
		}


		public bool canPay()
		{
			return who.Money >= cost;
		}

		//Draw the menu and its various buttons
		public override void draw(SpriteBatch b)
		{
			bool ignoreTitleSafe = true;
			Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe);
			//b.Draw(Game1.daybg, new Vector2(this.portraitBox.X, this.portraitBox.Y), Color.White);
			//Draw hospital back
			b.Draw(UItexture, new Vector2(this.portraitBox.X, this.portraitBox.Y), CharacterBackgroundRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			//Draw frame over
			b.Draw(UItexture, new Vector2(this.portraitBox.X, this.portraitBox.Y), new Rectangle(0, 32, 32, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);


			nameBox.Draw(b);

			foreach (KeyValuePair<string, ClothingToggle> ctkv in clothingToggles)
			{
				Color tint = Color.White;
                if (!ctkv.Value.active)
                {
					tint.A = 128;
                }
				b.Draw(UItexture, new Vector2(ctkv.Value.clickable.bounds.X, ctkv.Value.clickable.bounds.Y), new Rectangle(0, ctkv.Value.hover ? 16:0, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.8f);
				b.Draw(UItexture, new Vector2(ctkv.Value.clickable.bounds.X, ctkv.Value.clickable.bounds.Y), new Rectangle(ctkv.Value.offset, ctkv.Value.showing ? 0 : 16, 16, 16), tint, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.8f);
			}

			//Draw purchase background border
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), costLabel.bounds.X - 20 - (int)Game1.smallFont.MeasureString(costLabel.name).X, okButton.bounds.Y, (int)Game1.smallFont.MeasureString(costLabel.name).X+ 56, okButton.bounds.Height, Color.White, 4f, drawShadow: false);

			//Draw the coin icon
			b.Draw(Game1.mouseCursors, new Vector2(okButton.bounds.X - (9 + 10) * 2, okButton.bounds.Y+24), new Rectangle(193, 373, 9, 9), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.8f);


			foreach (ClickableTextureComponent leftSelectionButton in this.leftSelectionButtons)
			{
				leftSelectionButton.draw(b);
			}

			Texture2D whiteRectangle = new Texture2D(b.GraphicsDevice, 1, 1);
			whiteRectangle.SetData(new[] { Color.White });
			Texture2D greyRectangle = new Texture2D(b.GraphicsDevice, 1, 1);
			greyRectangle.SetData(new[] { Color.Gray });
			foreach (ClickableComponent swatch in this.swatches)
			{
                if (!swatch.visible)
                {
					continue;
				}
				Color[] cswatches = eyeSwatchColors;
				int toDraw = eyeSwatch;
                if (swatch.name.StartsWith("Hair"))
                {
					cswatches = hairSwatchColors;
					toDraw = hairSwatch;
                }
				if (swatch.name.StartsWith("DarkHair"))
				{
					cswatches = hairDarkSwatchColors;
					toDraw = hairDarkSwatch;
				}

				if (swatch.name.EndsWith("Left"))
				{
					toDraw--;
				}
				else
				{
					if (toDraw == cswatches.Length)
					{
						toDraw = int.Parse(swatch.name[^1..]) - 1;
					}
					else
					{
						toDraw += int.Parse(swatch.name[^1..]);
					}
				}
				if (toDraw > cswatches.Length) { toDraw -= cswatches.Length; }
				if (toDraw < 0) { toDraw = cswatches.Length; }

				if (toDraw > 0)
				{

					b.Draw(greyRectangle, new Rectangle(swatch.bounds.X+(swatchsize/2)-(int)(swatchsize*swatch.scale/2), swatch.bounds.Y + (swatchsize / 2) - (int)(swatchsize * swatch.scale / 2), (int)((float)swatch.bounds.Width*swatch.scale), (int)((float)swatch.bounds.Height * swatch.scale)), cswatches[toDraw - 1]);
					b.Draw(whiteRectangle, new Rectangle(swatch.bounds.X + (swatchsize / 2) - (int)(swatchsize * swatch.scale / 2) + 2, swatch.bounds.Y + (swatchsize / 2) - (int)(swatchsize * swatch.scale / 2) + 2, (int)((float)swatch.bounds.Width * swatch.scale) - 4, (int)((float)swatch.bounds.Height * swatch.scale) - 4), cswatches[toDraw - 1]);
				}
				//Draw a cross
				if (toDraw == 0)
				{
					b.Draw(Game1.mouseCursors, new Vector2(swatch.bounds.X+swatchsize/2-(int)(12*swatch.scale), swatch.bounds.Y + swatchsize / 2 - (int)(12 * swatch.scale)), new Rectangle(322, 498, 12, 12), Color.White, 0f, Vector2.Zero, 2f*swatch.scale, SpriteEffects.None, 0.8f);
				}

				if (swatch.name.EndsWith("0"))
				{
					//Indicator arrow
					b.Draw(UItexture, new Vector2(swatch.bounds.X + (swatchsize / 2) - (8 * 2 / 2), swatch.bounds.Y - (4 * 2 / 2)), new Rectangle(112, 0, 8, 6), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.8f);
				}

			}


			foreach (ClickableComponent c3 in this.labels)
			{
				if (!c3.visible)
				{
					continue;
				}
				string sub = "";//Line below the label
				string tiny_sub = "";//Smaller line below label
				float offset = 0f;
				float subYOffset = 0f;
				Color color = Game1.textColor;

				//Colour and size the strings
				if (c3 == this.hairLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					if (c3 == this.hairLabel)
					{
						sub = (Farmer.GetAllHairstyleIndices().IndexOf(who.hair.Value) + 1).ToString() ?? "";
					}
				}
				else if (c3 == this.accLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					sub = ((int)who.accessory.Value + 2).ToString() ?? "";
					if (who.accessory.Value == -1)
					{
						sub = T("none");
					}
				}
				else if (c3 == this.bodyLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					tiny_sub = (who.modData.ContainsKey("DB.body")) ? who.modData["DB.body"] : "Default";
				}
				else if (c3 == this.faceLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					tiny_sub = (who.modData.ContainsKey("DB.face")) ? who.modData["DB.face"] : "Default";
				}
				else if (c3 == this.armLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					tiny_sub = (who.modData.ContainsKey("DB.arm")) ? who.modData["DB.arm"] : "Default";
				}
				else if (c3 == this.beardLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					tiny_sub = (who.modData.ContainsKey("DB.beard")) ? who.modData["DB.beard"] : "Default";
				}
				else if (c3 == this.bodyHairLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					tiny_sub = (who.modData.ContainsKey("DB.bodyHair")) ? who.modData["DB.bodyHair"] : "Default";
				}
				else if (c3 == this.nakedLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					tiny_sub = (who.modData.ContainsKey("DB.nakedLower")) ? who.modData["DB.nakedLower"] : "Default";
				}
				else if (c3 == this.nakedULabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					tiny_sub = (who.modData.ContainsKey("DB.nakedUpper")) ? who.modData["DB.nakedUpper"] : "Default";
				}
				else if (c3 == this.costLabel)
				{
					offset = - Game1.smallFont.MeasureString(c3.name).X;
					color = new Color(86, 22, 12);
				}
				else
				{
					color = Game1.textColor;
				}

				if (tiny_sub == "Default") tiny_sub = T("default");

				Utility.drawTextWithShadow(b, c3.name, Game1.smallFont, new Vector2((float)c3.bounds.X + offset, c3.bounds.Y), color);
				if (sub.Length > 0)
				{
					Utility.drawTextWithShadow(b, sub, Game1.smallFont, new Vector2((float)(c3.bounds.X + 21) - Game1.smallFont.MeasureString(sub).X / 2f, (float)(c3.bounds.Y + 32) + subYOffset), color);
				}
				if (tiny_sub.Length > 0)
				{
					Utility.drawTextWithShadow(b, tiny_sub, Game1.dialogueFont, new Vector2((float)(c3.bounds.X + 21) - (Game1.dialogueFont.MeasureString(tiny_sub).X / 2f) / 2f, (float)(c3.bounds.Y + 32) + subYOffset), color, 0.5f);
				}
			}
			foreach (ClickableTextureComponent rightSelectionButton in this.rightSelectionButtons)
			{
				rightSelectionButton.draw(b);
			}

			if (this.canPay())
			{
				this.okButton.draw(b, Color.White, 0.75f);
			}
			else
			{
				Color semi = Color.White;
				semi.A = 128;
				this.okButton.draw(b, semi, 0.75f);
			}
			

			this.backButton.draw(b, Color.White, 0.75f);

			if (this.hairColorPicker != null)
			{
				this.hairColorPicker.draw(b);
			}

			if (this.hairDarkColorPicker != null)
			{
				this.hairDarkColorPicker.draw(b);
			}

			if (this.eyeColorPicker != null)
			{
				this.eyeColorPicker.draw(b);
			}

			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			this._displayFarmer.FarmerRenderer.draw(b, this._displayFarmer.FarmerSprite.CurrentAnimationFrame, this._displayFarmer.FarmerSprite.CurrentFrame, this._displayFarmer.FarmerSprite.SourceRect, new Vector2(this.portraitBox.Center.X - 32, this.portraitBox.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, this._displayFarmer);
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (this.hoverText != null && this.hoverText.Count() > 0)
			{
				IClickableMenu.drawHoverText(b, Game1.parseText(this.hoverText, Game1.smallFont, 256), Game1.smallFont, 0, 0, -1, this.hoverTitle);
			}
			base.drawMouse(b);
		}
	}
	class ClothingToggle
    {
		public int offset = 0;
		public int store;
		public Item storeItem;
		public bool showing = true;
		public bool active = true;
		public bool hover = false;
		public ClickableComponent clickable;
    }
}
