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
	internal class DoctorModifierFacePage : BodyModifier
	{

		DoctorModifier dm;
		public DoctorModifierFacePage(DoctorModifier dm, bool isWizardSubmenu, int height)
		: base(height, isWizardSubmenu)
		{
			this.dm = dm;

			Texture2D eye_texture = Game1.content.Load<Texture2D>("Mods/ribeena.dynamicbodies/assets/Interface/eyeColors.png");
			eyeSwatchColors = new Color[eye_texture.Width * eye_texture.Height];
			eye_texture.GetData(eyeSwatchColors, 0, eyeSwatchColors.Length);

			Texture2D hair_texture = Game1.content.Load<Texture2D>("Mods/ribeena.dynamicbodies/assets/Interface/doctor_hairColors.png");
			base.hairSwatchColors = new Color[hair_texture.Width * hair_texture.Height];
			hair_texture.GetData(hairSwatchColors, 0, hairSwatchColors.Length);

			Texture2D haird_texture = Game1.content.Load<Texture2D>("Mods/ribeena.dynamicbodies/assets/Interface/doctor_hairDarkColors.png");
			hairDarkSwatchColors = new Color[haird_texture.Width * haird_texture.Height];
			haird_texture.GetData(hairDarkSwatchColors, 0, hairDarkSwatchColors.Length);

			lashSwatchColors = new Color[hairSwatchColors.Length+ hairDarkSwatchColors.Length];
			lashSwatchColors = hairSwatchColors.Concat(hairDarkSwatchColors).ToArray(); //All basic hair colours

			CharacterBackgroundRect = new Rectangle(32, 32, 32, 48);

			base.cost = dm.isWizardSubmenu ? 0 : DoctorModifier.doctor_cost;
			if (ModEntry.Config.freecustomisation) cost = 0;

			base.isPage = true;


			setUpPositions();
		}

		public void setUpPositions()
		{
			setupGeneralPositions(T("cosmetic_patient") + ": ");

			swatches.Clear();

			int leftPadding = 64 + 4;
			int yOffset = 32;
			int label_col1_width = 42 * 4;
			int arrow_size = 64;

			int leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-20) : 0);


			int label_col2_position = portraitBox.X + portraitBox.Width + 12 * 4;
			int label_col2_width = 40 * 4;


			//Items next to portrait box

			//Next line
			yOffset += 32;
			Point top = new Point(label_col2_position + label_col2_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);

			//Eye colour
			labels.Add(new ClickableComponent(new Rectangle(label_col2_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_EyeColor")));
			swatches.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 12, swatchsize, swatchsize), "EyeSwatchLeft")
			{
				myID = region_eyeSwatch1,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			ClickableComponent eyeSwatchSelected = new ClickableComponent(new Rectangle(top.X + swatchsize + 4, top.Y + 12, swatchsize, swatchsize), "EyeSwatch0")
			{
				myID = region_eyeSwatch2,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			eyeSwatchSelected.scale = 1.1f;
			swatches.Add(eyeSwatchSelected);
			swatches.Add(new ClickableComponent(new Rectangle(top.X + swatchsize * 2 + 8, top.Y + 12, swatchsize, swatchsize), "EyeSwatch1")
			{
				myID = region_eyeSwatch2,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			swatches.Add(new ClickableComponent(new Rectangle(top.X + swatchsize * 3 + 12, top.Y + 12, swatchsize, swatchsize), "EyeSwatch2")
			{
				myID = region_eyeSwatch4,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			//Next line
			yOffset += 58;


			top = new Point(label_col2_position + label_col2_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);

			//Lash Colour
			labels.Add(new ClickableComponent(new Rectangle(label_col2_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), T("lash_color")));

			swatches.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 12, swatchsize, swatchsize), "LashSwatchLeft")
			{
				myID = region_lashSwatch1,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			ClickableComponent hairSwatchSelected = new ClickableComponent(new Rectangle(top.X + swatchsize + 4, top.Y + 12, swatchsize, swatchsize), "LashSwatch0")
			{
				myID = region_lashSwatch2,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			hairSwatchSelected.scale = 1.1f;
			swatches.Add(hairSwatchSelected);
			swatches.Add(new ClickableComponent(new Rectangle(top.X + swatchsize * 2 + 8, top.Y + 12, swatchsize, swatchsize), "LashSwatch1")
			{
				myID = region_lashSwatch3,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			swatches.Add(new ClickableComponent(new Rectangle(top.X + swatchsize * 3 + 12, top.Y + 12, swatchsize, swatchsize), "LashSwatch2")
			{
				myID = region_lashSwatch4,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			//Next line
			yOffset += 58;


			//After portraitbox

			//Wider selections
			label_col1_width += 48;
			label_col2_position = base.xPositionOnScreen + leftPadding + label_col1_width + arrow_size / 2 + (12 * 4);


			label_col2_position = base.xPositionOnScreen + leftPadding + label_col1_width + arrow_size / 2 + (12 * 4);

			//Face Base
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Face", new Rectangle(base.xPositionOnScreen + leftPadding + leftSelectionXOffset - arrow_size / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = region_faceLeft,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			this.labels.Add(faceLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + leftPadding + (label_col1_width / 2) + (leftSelectionXOffset / 2), base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), T("face_style")));
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Face", new Rectangle(base.xPositionOnScreen + leftPadding + label_col1_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = region_faceRight,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});


			//Beard Style
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Beard", new Rectangle(label_col2_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = region_beardLeft,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			beardLabel = new ClickableComponent(new Rectangle(label_col2_position + arrow_size / 2 + label_col1_width / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), T("beard"));
			this.labels.Add(beardLabel);
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Beard", new Rectangle(label_col2_position + label_col1_width + arrow_size / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = region_beardRight,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			//Next line
			yOffset += 68;

			//Body Style
			/*leftSelectionButtons.Add(new ClickableTextureComponent("Body", new Rectangle(base.xPositionOnScreen + leftPadding + leftSelectionXOffset - arrow_size / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = region_bodyLeft,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			bodyLabel = new ClickableComponent(new Rectangle(base.xPositionOnScreen + leftPadding + (label_col1_width / 2) + (leftSelectionXOffset / 2), base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), T("body_style"));
			this.labels.Add(bodyLabel);
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Body", new Rectangle(base.xPositionOnScreen + leftPadding + label_col1_width, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = region_bodyRight,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			//Arm Style
			this.leftSelectionButtons.Add(new ClickableTextureComponent("Arm", new Rectangle(label_col2_position, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = region_armLeft,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			armLabel = new ClickableComponent(new Rectangle(label_col2_position + arrow_size / 2 + label_col1_width / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), T("arm_style"));
			this.labels.Add(armLabel);
			this.rightSelectionButtons.Add(new ClickableTextureComponent("Arm", new Rectangle(label_col2_position + label_col1_width + arrow_size / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = region_armRight,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});

			//Next line
			yOffset += 68;
			*/
			

			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				base.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}
	}
}
