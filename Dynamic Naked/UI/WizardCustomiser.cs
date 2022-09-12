using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Menus;

using DynamicBodies.Data;

namespace DynamicBodies.UI
{
	public class WizardCustomiser : IClickableMenu
	{

		public const int region_okbutton = 505, region_randomButton = 507, region_male = 508, region_female = 509;

		public const int region_dog = 510, region_cat = 511;

		public const int region_skinLeft = 518, region_skinRight = 519;
		public const int region_directionLeft = 520, region_directionRight = 521;

		public const int region_nameBox = 536;
		public const int region_farmNameBox = 537;
		public const int region_favThingBox = 538;

		private int currentPet;


		public List<ClickableComponent> labels = new List<ClickableComponent>();
		public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();
		public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();

		public List<ClickableComponent> genderButtons = new List<ClickableComponent>();
		public List<ClickableComponent> petButtons = new List<ClickableComponent>();

		public ClickableTextureComponent okButton;
		public ClickableTextureComponent randomButton;

		private TextBox nameBox;
		private TextBox farmnameBox;
		private TextBox favThingBox;

		public bool isModifyingExistingPet;

		private Vector2 helpStringSize;
		private string hoverText;
		private string hoverTitle;

		public ClickableComponent nameBoxCC;
		public ClickableComponent favThingBoxCC;

		public ClickableComponent backButton;

		private ClickableComponent nameLabel;

		private ClickableComponent favoriteLabel;
		private ClickableComponent skinLabel;

		protected bool _shouldShowBackButton = true;

		protected Farmer _displayFarmer;
		public Rectangle portraitBox;
		public Rectangle? petPortraitBox;

		public string oldName = "";

		private int timesRandom;

		Multiplayer multiplayer;


		public WizardCustomiser()
			: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 648 + IClickableMenu.borderWidth * 2 + 64)
		{

			this.oldName = Game1.player.Name;
			this.setUpPositions();

			this._displayFarmer = Game1.player;

			//get the multiplayer instance to be able to send messages
			multiplayer = ModEntry.context.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);

			base.xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			base.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;

			this.setUpPositions();
		}

		private void setUpPositions()
		{
			this.labels.Clear();
			this.petButtons.Clear();
			this.genderButtons.Clear();
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
			this.backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -198 - 48, Game1.uiViewport.Height - 81 - 24, 198, 81), "")
			{
				myID = 81114,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.nameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
				Y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16,
				Text = Game1.player.Name
			};
			this.nameBoxCC = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 192, 48), "")
			{
				myID = 536,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			int textBoxLabelsXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-4) : 0);
			this.labels.Add(this.nameLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Name")));


			int favThingBoxXoffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 48 : 0);
			this.favThingBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
			{
				X = base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256 + favThingBoxXoffset,
				Y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64,
				Text = Game1.player.favoriteThing
			};
			this.favThingBoxCC = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 192, 48), "")
			{
				myID = 538,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};

			this.labels.Add(this.favoriteLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_FavoriteThing")));
			this.randomButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + 64 + 56, 40, 40), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f)
			{
				myID = 507,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.portraitBox = new Rectangle(base.xPositionOnScreen + 64 + 42 - 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 128, 192);

			int yOffset = 128;
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.X - 32, this.portraitBox.Y + 144, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 520,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(this.portraitBox.Right - 32, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 521,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			int leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-20) : 0);

			this.isModifyingExistingPet = false;


			this.genderButtons.Add(new ClickableTextureComponent("Male", new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 8, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), 4f)
			{
				myID = 508,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.genderButtons.Add(new ClickableTextureComponent("Female", new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 64 + 24, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), 4f)
			{
				myID = 509,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			int start_x = base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 320 + 16;
			int start_y = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 64 + 48;
			for (int i = 0; i < this.genderButtons.Count; i++)
			{
				this.genderButtons[i].bounds.X = start_x + 80 * i;
				this.genderButtons[i].bounds.Y = start_y;
			}

			yOffset = 192;
			leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? (-20) : 0);
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + leftSelectionXOffset, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 518,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(this.skinLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + 64 + 8 + leftSelectionXOffset / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Skin")));
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 128, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 519,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			Point top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
			int label_position = base.xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8;

			top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);

			Pet pet = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), mustBeVillager: false);
			if (pet != null)
			{
				Game1.player.whichPetBreed = pet.whichBreed;
				Game1.player.catPerson = pet is Cat;
				this.isModifyingExistingPet = true;
				yOffset += 60;
				this.labels.Add(new ClickableComponent(new Rectangle((int)((float)(base.xPositionOnScreen + base.width / 2) - Game1.smallFont.MeasureString(pet.name).X / 2f), base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), pet.Name));
				top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
				top.X = base.xPositionOnScreen + base.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 128;
				yOffset += 42;
				top = new Point(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
				top.X = base.xPositionOnScreen + base.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 128;
				this.petPortraitBox = new Rectangle(base.xPositionOnScreen + base.width / 2 - 32, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64);


				this.leftSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(this.petPortraitBox.Value.Left - 64, this.petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = 511,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
				this.rightSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(this.petPortraitBox.Value.Left + 64, this.petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 510,
					upNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					downNeighborID = -99998
				});
			}


			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				base.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}


		public override void snapToDefaultClickableComponent()
		{
			base.currentlySnappedComponent = base.getComponentWithID(521);
			this.snapCursorToCurrentSnappedComponent();
		}

		private void optionButtonClick(string name)
		{
			switch (name)
			{
				case "Male":
					ModEntry.debugmsg($"Changed player gender - Male", LogLevel.Debug);
					Game1.player.changeGender(male: true);
					Game1.player.changeHairStyle(0);
					break;
				case "Female":
					ModEntry.debugmsg($"Changed player gender - Female", LogLevel.Debug);
					Game1.player.changeGender(male: false);
					Game1.player.changeHairStyle(16);
					break;
				case "OK":
					{
						if (!this.canLeaveMenu())
						{
							return;
						}
						Game1.player.Name = this.nameBox.Text.Trim();
						Game1.player.displayName = Game1.player.Name;
						Game1.player.favoriteThing.Value = this.favThingBox.Text.Trim();
						Game1.player.isCustomized.Value = true;

						try
						{
							if (Game1.player.Name != this.oldName && Game1.player.Name.IndexOf("[") != -1 && Game1.player.Name.IndexOf("]") != -1)
							{
								int start = Game1.player.Name.IndexOf("[");
								int end = Game1.player.Name.IndexOf("]");
								if (end > start)
								{
									string s = Game1.player.Name.Substring(start + 1, end - start - 1);
									int item_index = -1;
									if (int.TryParse(s, out item_index))
									{
										string itemName = Game1.objectInformation[item_index].Split('/')[0];
										switch (Game1.random.Next(5))
										{
											case 0:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg1"), new Color(104, 214, 255));
												break;
											case 1:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg2", Lexicon.makePlural(itemName)), new Color(100, 50, 255));
												break;
											case 2:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg3", Lexicon.makePlural(itemName)), new Color(0, 220, 40));
												break;
											case 3:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg4"), new Color(0, 220, 40));
												DelayedAction.functionAfterDelay(delegate
												{
													Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg5"), new Color(104, 214, 255));
												}, 12000);
												break;
											case 4:
												Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg6", Lexicon.getProperArticleForWord(itemName), itemName), new Color(100, 120, 255));
												break;
										}
									}
								}
							}
						}
						catch (Exception)
						{
						}
						string changed_pet_name = null;
						if (this.petPortraitBox.HasValue && Game1.gameMode == 3 && Game1.locations != null)
						{
							Pet pet = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), mustBeVillager: false);
							if (pet != null && this.petHasChanges(pet))
							{
								pet.whichBreed.Value = Game1.player.whichPetBreed;
								changed_pet_name = pet.getName();
							}
						}
						Game1.exitActiveMenu();

						if (changed_pet_name != null)
						{
							multiplayer.globalChatInfoMessage("Makeover_Pet", Game1.player.Name, changed_pet_name);
						}
						else
						{
							multiplayer.globalChatInfoMessage("Makeover", Game1.player.Name);
						}
						Game1.flashAlpha = 1f;
						Game1.playSound("yoba");

						break;
					}
			}
			Game1.playSound("coin");
		}

		public bool petHasChanges(Pet pet)
		{
			if (Game1.player.catPerson && pet == null)
			{
				return true;
			}
			if (Game1.player.whichPetBreed != pet.whichBreed.Value)
			{
				return true;
			}
			return false;
		}

		private void selectionClick(string name, int change)
		{
			switch (name)
			{
				case "Skin":
					Game1.player.changeSkinColor((int)Game1.player.skin + change);
					Game1.playSound("skeletonStep");
					break;
				case "Direction":
					this._displayFarmer.faceDirection((this._displayFarmer.FacingDirection - change + 4) % 4);
					this._displayFarmer.FarmerSprite.StopAnimation();
					this._displayFarmer.completelyStopAnimatingOrDoingAction();
					Game1.playSound("pickUpItem");
					break;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{

			if (this.genderButtons.Count > 0)
			{
				foreach (ClickableComponent c6 in this.genderButtons)
				{
					if (c6.containsPoint(x, y))
					{
						
						PlayerBaseExtended.Get(Game1.player).DefaultOptions(Game1.player);
						this.optionButtonClick(c6.name);
						
						c6.scale -= 0.5f;
						c6.scale = Math.Max(3.5f, c6.scale);
					}
				}
			}
			if (this.petButtons.Count > 0)
			{
				foreach (ClickableComponent c4 in this.petButtons)
				{
					if (c4.containsPoint(x, y))
					{
						this.optionButtonClick(c4.name);
						c4.scale -= 0.5f;
						c4.scale = Math.Max(3.5f, c4.scale);
					}
				}
			}

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
			if (this.okButton.containsPoint(x, y) && this.canLeaveMenu())
			{
				this.optionButtonClick(this.okButton.name);
				this.okButton.scale -= 0.25f;
				this.okButton.scale = Math.Max(0.75f, this.okButton.scale);
			}

			this.nameBox.Update();
			this.favThingBox.Update();

			if (!this.randomButton.containsPoint(x, y))
			{
				return;
			}
			string sound = "drumkit6";
			if (this.timesRandom > 0)
			{
				switch (Game1.random.Next(15))
				{
					case 0:
						sound = "drumkit1";
						break;
					case 1:
						sound = "dirtyHit";
						break;
					case 2:
						sound = "axchop";
						break;
					case 3:
						sound = "hoeHit";
						break;
					case 4:
						sound = "fishSlap";
						break;
					case 5:
						sound = "drumkit6";
						break;
					case 6:
						sound = "drumkit5";
						break;
					case 7:
						sound = "drumkit6";
						break;
					case 8:
						sound = "junimoMeep1";
						break;
					case 9:
						sound = "coin";
						break;
					case 10:
						sound = "axe";
						break;
					case 11:
						sound = "hammer";
						break;
					case 12:
						sound = "drumkit2";
						break;
					case 13:
						sound = "drumkit4";
						break;
					case 14:
						sound = "drumkit3";
						break;
				}
			}
			Game1.playSound(sound);
			this.timesRandom++;
			if (this.skinLabel != null && this.skinLabel.visible)
			{
				Game1.player.changeSkinColor(Game1.random.Next(6));
				if (Game1.random.NextDouble() < 0.25)
				{
					Game1.player.changeSkinColor(Game1.random.Next(24));
				}
			}
			this.randomButton.scale = 3.5f;
		}

		public override void leftClickHeld(int x, int y)
		{
		}

		public override void releaseLeftClick(int x, int y)
		{
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.SnappyMenus && !Game1.options.doesInputListContain(Game1.options.menuButton, key) && Game1.GetKeyboardState().GetPressedKeys().Count() == 0)
			{
				base.receiveKeyPress(key);
			}
		}

		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.hoverTitle = "";

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

			foreach (ClickableTextureComponent c3 in this.genderButtons)
			{
				if (c3.containsPoint(x, y))
				{
					c3.scale = Math.Min(c3.scale + 0.05f, c3.baseScale + 0.5f);
				}
				else
				{
					c3.scale = Math.Max(c3.scale - 0.05f, c3.baseScale);
				}
			}

			if (this.okButton.containsPoint(x, y) && this.canLeaveMenu())
			{
				this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.1f);
			}
			else
			{
				this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
			}

			this.randomButton.tryHover(x, y, 0.25f);
			this.randomButton.tryHover(x, y, 0.25f);
			this.nameBox.Hover(x, y);
			this.favThingBox.Hover(x, y);
		}

		public bool canLeaveMenu()
		{
			if (Game1.player.Name.Length > 0 && Game1.player.farmName.Length > 0)
			{
				return Game1.player.favoriteThing.Length > 0;
			}
			return false;
		}

		public override void draw(SpriteBatch b)
		{
			bool ignoreTitleSafe = false;
			ignoreTitleSafe = true;

			Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe);

			b.Draw(Game1.daybg, new Vector2(this.portraitBox.X, this.portraitBox.Y), Color.White);
			foreach (ClickableTextureComponent c2 in this.genderButtons)
			{
				if (c2.visible)
				{
					c2.draw(b);
					if ((c2.name.Equals("Male") && Game1.player.IsMale) || (c2.name.Equals("Female") && !Game1.player.IsMale))
					{
						b.Draw(Game1.mouseCursors, c2.bounds, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34), Color.White);
					}
				}
			}

			Game1.player.Name = this.nameBox.Text;
			Game1.player.favoriteThing.Set(this.favThingBox.Text);
			
			foreach (ClickableTextureComponent leftSelectionButton in this.leftSelectionButtons)
			{
				leftSelectionButton.draw(b);
			}
			foreach (ClickableComponent c3 in this.labels)
			{
				if (!c3.visible)
				{
					continue;
				}
				string sub = "";
				float offset = 0f;
				float subYOffset = 0f;
				Color color = Game1.textColor;
				if (c3 == this.nameLabel)
				{
					color = ((Game1.player.Name != null && Game1.player.Name.Length < 1) ? Color.Red : Game1.textColor);
				}
				else if (c3 == this.favoriteLabel)
				{
					color = ((Game1.player.favoriteThing.Value != null && Game1.player.favoriteThing.Length < 1) ? Color.Red : Game1.textColor);
				}
				else if (c3 == this.skinLabel)
				{
					offset = 21f - Game1.smallFont.MeasureString(c3.name).X / 2f;
					sub = ((int)Game1.player.skin + 1).ToString() ?? "";
				}
				else
				{
					color = Game1.textColor;
				}
				Utility.drawTextWithShadow(b, c3.name, Game1.smallFont, new Vector2((float)c3.bounds.X + offset, c3.bounds.Y), color);
				if (sub.Length > 0)
				{
					Utility.drawTextWithShadow(b, sub, Game1.smallFont, new Vector2((float)(c3.bounds.X + 21) - Game1.smallFont.MeasureString(sub).X / 2f, (float)(c3.bounds.Y + 32) + subYOffset), color);
				}
			}
			foreach (ClickableTextureComponent rightSelectionButton in this.rightSelectionButtons)
			{
				rightSelectionButton.draw(b);
			}

			if (this.petPortraitBox.HasValue)
			{
				b.Draw(Game1.mouseCursors, this.petPortraitBox.Value, new Rectangle(160 + ((!Game1.player.catPerson) ? 48 : 0) + Game1.player.whichPetBreed * 16, 208, 16, 16), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.89f);
			}

			if (this.canLeaveMenu())
			{
				this.okButton.draw(b, Color.White, 0.75f);
			}
			else
			{
				this.okButton.draw(b, Color.White, 0.75f);
				this.okButton.draw(b, Color.Black * 0.5f, 0.751f);
			}

			this.nameBox.Draw(b);
			this.favThingBox.Draw(b);


			this.randomButton.draw(b);
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

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (a.region != b.region)
			{
				return false;
			}

			if (a == this.favThingBoxCC && b.myID >= 522 && b.myID <= 530)
			{
				return false;
			}
			if (b == this.favThingBoxCC && a.myID >= 522 && a.myID <= 530)
			{
				return false;
			}

			if (a.name == "Direction" && b.name == "Pet")
			{
				return false;
			}
			if (b.name == "Direction" && a.name == "Pet")
			{
				return false;
			}

			if (this.randomButton != null)
			{
				switch (direction)
				{
					case 3:
						if (b == this.randomButton && a.name == "Direction")
						{
							return false;
						}
						break;
					default:
						if (a == this.randomButton && b.name != "Direction")
						{
							return false;
						}
						if (b == this.randomButton && a.name != "Direction")
						{
							return false;
						}
						break;
					case 0:
						break;
				}
				if (a.myID == 622 && direction == 1 && (b == this.nameBoxCC || b == this.favThingBoxCC ))
				{
					return false;
				}
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		public override void update(GameTime time)
		{
			base.update(time);

			this.backButton.visible = this._shouldShowBackButton;
		}

		protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			return true;
		}
	}
}
