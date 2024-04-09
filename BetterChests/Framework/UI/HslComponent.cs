namespace StardewMods.BetterChests.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Models;
using StardewValley.Menus;
using StardewValley.Objects;

internal sealed class HslComponent
{
    private static readonly PerScreen<HslColor> SavedColor = new(() => HslComponent.Transparent);
    private static readonly HslColor Transparent = new(0, 0, 0);

    private readonly DiscreteColorPicker colorPicker;
    private readonly Rectangle copyArea;
    private readonly ClickableTextureComponent copyComponent;
    private readonly Rectangle defaultColorArea;
    private readonly ClickableTextureComponent defaultColorComponent;
    private readonly Func<Color> getColor;
    private readonly Slider hue;
    private readonly IInputHelper inputHelper;
    private readonly Slider lightness;
    private readonly Slider saturation;
    private readonly Action<Color> setColor;
    private readonly int xPosition;
    private readonly int yPosition;

    private Slider? holding;

    public HslComponent(
        AssetHandler assetHandler,
        DiscreteColorPicker colorPicker,
        IInputHelper inputHelper,
        Func<Color> getColor,
        Action<Color> setColor,
        int xPosition,
        int yPosition)
    {
        this.colorPicker = colorPicker;
        this.inputHelper = inputHelper;
        this.getColor = getColor;
        this.setColor = setColor;
        this.xPosition = xPosition;
        this.yPosition = yPosition;

        this.CurrentColor = HslComponent.Transparent;
        if (colorPicker.colorSelection != 0)
        {
            var playerChoiceColor = getColor();
            this.CurrentColor = HslColor.FromColor(playerChoiceColor);
            colorPicker.colorSelection =
                (playerChoiceColor.R << 0) | (playerChoiceColor.G << 8) | (playerChoiceColor.B << 16);
        }

        this.copyArea = new Rectangle(xPosition + 30, yPosition - 4, 36, 36);
        this.copyComponent = new ClickableTextureComponent(
            new Rectangle(xPosition + 34, yPosition + 2, 8, 8),
            assetHandler.Icons.Value,
            new Rectangle(116, 4, 8, 8),
            3);

        this.defaultColorArea = new Rectangle(xPosition - 6, yPosition - 4, 36, 36);
        this.defaultColorComponent = new ClickableTextureComponent(
            new Rectangle(xPosition - 2, yPosition, 7, 7),
            Game1.mouseCursors,
            new Rectangle(295, 503, 7, 7),
            Game1.pixelZoom);

        this.hue = new Slider(
            assetHandler.HslTexture,
            () => this.CurrentColor.H,
            value =>
            {
                this.CurrentColor = this.colorPicker.colorSelection == 0
                    ? assetHandler.HslColors[(int)(value * assetHandler.HslColors.Length)]
                    : this.CurrentColor with { H = value };

                this.UpdateColor();
            },
            new Rectangle(xPosition, yPosition + 36, 23, 522),
            29);

        this.saturation = new Slider(
            value => (this.CurrentColor with
            {
                S = this.colorPicker.colorSelection == 0 ? 0 : value,
                L = Math.Max(0.01f, this.colorPicker.colorSelection == 0 ? value : this.CurrentColor.L),
            }).ToRgbColor(),
            () => this.CurrentColor.S,
            value =>
            {
                this.CurrentColor = this.colorPicker.colorSelection == 0
                    ? new HslColor(0, value, 0.5f)
                    : this.CurrentColor with
                    {
                        S = value,
                        L = Math.Max(0.01f, this.CurrentColor.L),
                    };

                this.UpdateColor();
            },
            new Rectangle(xPosition + 32, yPosition + 300, 23, 256),
            16);

        this.lightness = new Slider(
            value => (this.CurrentColor with
            {
                S = this.colorPicker.colorSelection == 0 ? 0 : this.CurrentColor.S,
                L = value,
            }).ToRgbColor(),
            () => this.CurrentColor.L,
            value =>
            {
                this.CurrentColor = this.CurrentColor with { L = value };
                this.UpdateColor();
            },
            new Rectangle(xPosition + 32, yPosition + 36, 23, 256),
            16);
    }

    /// <summary>Gets the current hsl color.</summary>
    public HslColor CurrentColor { get; private set; }

    public bool LeftClick(int mouseX, int mouseY)
    {
        if (this.holding is not null)
        {
            return false;
        }

        if (this.defaultColorArea.Contains(mouseX, mouseY))
        {
            this.CurrentColor = HslComponent.Transparent;
            this.colorPicker.colorSelection = 0;
            this.UpdateColor();
            return true;
        }

        if (this.copyArea.Contains(mouseX, mouseY))
        {
            HslComponent.SavedColor.Value = this.CurrentColor;
            return true;
        }

        if (this.hue.LeftClick(mouseX, mouseY))
        {
            this.holding = this.hue;
            return true;
        }

        if (this.saturation.LeftClick(mouseX, mouseY))
        {
            this.holding = this.saturation;
            return true;
        }

        if (this.lightness.LeftClick(mouseX, mouseY))
        {
            this.holding = this.lightness;
            return true;
        }

        return false;
    }

    public bool RightClick(int mouseX, int mouseY)
    {
        if (this.holding is not null)
        {
            return false;
        }

        if (this.copyArea.Contains(mouseX, mouseY))
        {
            this.CurrentColor = HslComponent.SavedColor.Value;
            this.UpdateColor();
            return true;
        }

        return false;
    }

    /// <summary>Draw the color picker.</summary>
    /// <param name="spriteBatch">The sprite batch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        var isDown = this.inputHelper.IsDown(SButton.MouseLeft) || this.inputHelper.IsSuppressed(SButton.MouseLeft);
        if (!isDown)
        {
            if (this.holding is not null)
            {
                this.holding.Holding = false;
                this.holding = null;
            }
        }

        // Get Input states
        var (mouseX, mouseY) = Game1.getMousePosition(true);

        // Update components
        this.hue.Update(mouseX, mouseY);
        this.saturation.Update(mouseX, mouseY);
        this.lightness.Update(mouseX, mouseY);

        // Background
        IClickableMenu.drawTextureBox(
            spriteBatch,
            this.xPosition - (IClickableMenu.borderWidth / 2),
            this.yPosition - (IClickableMenu.borderWidth / 2),
            58 + IClickableMenu.borderWidth,
            558 + IClickableMenu.borderWidth,
            Color.LightGray);

        // Default color component
        this.defaultColorComponent.draw(spriteBatch);

        // Copy component
        this.copyComponent.draw(spriteBatch);

        // Hue slider
        this.hue.Draw(spriteBatch);

        // Saturation slider
        this.saturation.Draw(spriteBatch);

        // Lightness slider
        this.lightness.Draw(spriteBatch);

        // Default color selected
        if (this.colorPicker.colorSelection == 0)
        {
            IClickableMenu.drawTextureBox(
                spriteBatch,
                Game1.mouseCursors,
                new Rectangle(375, 357, 3, 3),
                this.defaultColorArea.X,
                this.defaultColorArea.Y,
                36,
                36,
                Color.Black,
                Game1.pixelZoom,
                false);
        }

        // Chest
        (this.colorPicker.itemToDrawColored as Chest)?.draw(
            spriteBatch,
            this.xPosition,
            this.yPosition - Game1.tileSize - (IClickableMenu.borderWidth / 2),
            local: true);
    }

    private void UpdateColor()
    {
        var c = this.CurrentColor.ToRgbColor();
        this.setColor(c);
        this.colorPicker.colorSelection = (c.R << 0) | (c.G << 8) | (c.B << 16);

        if (this.colorPicker.itemToDrawColored is Chest chest)
        {
            chest.playerChoiceColor.Value = this.colorPicker.colorSelection == 0 ? Color.Black : c;
        }
    }
}