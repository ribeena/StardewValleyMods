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
    
	internal class FullColourModifier : BodyModifier
	{
		private const int leah_cost = 100;
		public FullColourModifier(bool isWizardSubmenu = false) : base(528, isWizardSubmenu)
		{
			CharacterBackgroundRect = new Rectangle(64, 32, 32, 48);

			base.cost = isWizardSubmenu ? 0 : leah_cost;
			if (ModEntry.Config.freecustomisation) cost = 0;
			setUpPositions();
		}

		public override void setUpPositions()
		{
			setupGeneralPositions(T("color_specialist") + ": ");
			this.colorPickerCCs.Clear();

			int leftPadding = 64 + 4;
			int yOffset = 32;
			int label_col1_width = 42 * 4;
			int arrow_size = 64;

			int leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-20) : 0);
			int label_col2_position = portraitBox.X + portraitBox.Width + 12 * 4;
			int label_col2_width = 40 * 4;

			//Items next to portrait box

			//Line below 
			yOffset += 32;
			Point top = new Point(label_col2_position + label_col2_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);

			//Eye colour picker
			this.labels.Add(new ClickableComponent(new Rectangle(label_col2_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_EyeColor")));

			this.eyeColorPicker = new ColorPicker("Eyes", top.X, top.Y);
			this.eyeColorPicker.setColor(who.newEyeColor.Value);
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = region_colorPicker1,
				downNeighborID = -99998,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = region_colorPicker2,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = region_colorPicker3,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			//Next line
			yOffset += 68;
			top = new Point(label_col2_position + label_col2_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);

			//Hair Colour
			this.labels.Add(new ClickableComponent(new Rectangle(label_col2_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_HairColor")));

			this.hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
			this.hairColorPicker.setColor(who.hairstyleColor.Value);
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = region_colorPicker4,
				downNeighborID = -99998,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = region_colorPicker5,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = region_colorPicker6,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});

			//Next line
			yOffset += 68;
			top = new Point(label_col2_position + label_col2_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);

			//Dark hair
			this.labels.Add(new ClickableComponent(new Rectangle(label_col2_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), T("hair_dark") + ":"));
			this.hairDarkColorPicker = new ColorPicker("HairDark", top.X, top.Y);

			if (who.modData.ContainsKey("DB.darkHair"))
			{
				this.hairDarkColorPicker.setColor(new Color(uint.Parse(who.modData["DB.darkHair"])));
			}
			else
			{
				//57 grey is often the darket colour of a hair style
				this.hairDarkColorPicker.setColor(new Color(57, 57, 57));
			}
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = region_colorPicker7,
				downNeighborID = -99998,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = region_colorPicker8,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			this.colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = region_colorPicker9,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});

			//Next line
			yOffset += 68;

			//After portraitbox
			//yOffset = 128;
			//label_col2_position = base.xPositionOnScreen + leftPadding + label_col1_width + arrow_size / 2 + (12 * 4);

			//Wider selections
			label_col1_width += 48;
			label_col2_position = base.xPositionOnScreen + leftPadding + label_col1_width + arrow_size / 2 + (12 * 4);

			//Hair Style
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(base.xPositionOnScreen + leftPadding + leftSelectionXOffset - arrow_size / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, arrow_size, arrow_size), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = region_hairLeft,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.hairLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + leftPadding + (label_col1_width / 2) + (leftSelectionXOffset / 2), base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Hair"));
			this.labels.Add(this.hairLabel);
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(base.xPositionOnScreen + leftPadding + label_col1_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = region_hairRight,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			//Next line
			yOffset += 68;

			//Beard Style
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Beard", new Rectangle(base.xPositionOnScreen + leftPadding + leftSelectionXOffset - arrow_size / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = region_beardLeft,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.beardLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + leftPadding + (label_col1_width / 2) + (leftSelectionXOffset / 2), base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), T("beard"));
			this.labels.Add(this.beardLabel);
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Beard", new Rectangle(base.xPositionOnScreen + leftPadding + label_col1_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = region_beardRight,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			//BodyHair Style
			this.leftSelectionButtons.Add(new ClickableTextureComponent("BodyHair", new Rectangle(label_col2_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = region_bodyHairLeft,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.bodyHairLabel = new ClickableComponent(new Rectangle(label_col2_position + arrow_size / 2 + label_col1_width / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), T("body_hair"));
			this.labels.Add(this.bodyHairLabel);
			this.rightSelectionButtons.Add(new ClickableTextureComponent("BodyHair", new Rectangle(label_col2_position + label_col1_width + arrow_size / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = region_bodyHairRight,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				base.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

	}
}
