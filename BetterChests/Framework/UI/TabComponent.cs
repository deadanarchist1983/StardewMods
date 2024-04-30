namespace StardewMods.BetterChests.Framework.UI;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models;
using StardewValley.Menus;

/// <summary>Represents a component with an icon that expands into a label when hovered.</summary>
internal sealed class TabComponent : ClickableComponent
{
    private readonly ClickableTextureComponent icon;

    /// <summary>Initializes a new instance of the <see cref="TabComponent" /> class.</summary>
    /// <param name="x">The x-coordinate of the tab component.</param>
    /// <param name="y">The y-coordinate of the tab component.</param>
    /// <param name="tabIcon">The tab icon.</param>
    /// <param name="label">The label of the tab.</param>
    public TabComponent(int x, int y, TabIcon tabIcon, string label)
        : base(
            new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
            ((int)Math.Pow(y, 2) + x).ToString(CultureInfo.InvariantCulture),
            label)
    {
        this.myID = (int)(Math.Pow(y, 2) + x);
        this.icon = new ClickableTextureComponent(
            new Rectangle(x, y, Game1.tileSize, Game1.tileSize),
            Game1.content.Load<Texture2D>(tabIcon.Path),
            tabIcon.Area,
            Game1.pixelZoom);
    }

    /// <summary>Draw the color picker.</summary>
    /// <param name="spriteBatch">The sprite batch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch) => this.icon.draw(spriteBatch);

    /// <summary>Performs a left-click action based on the given mouse coordinates.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse.</param>
    /// <param name="mouseY">The y-coordinate of the mouse.</param>
    /// <returns>true if the left-click action was successfully performed; otherwise, false.</returns>
    public bool LeftClick(int mouseX, int mouseY) => false;

    /// <summary>Performs a right-click action based on the given mouse coordinates.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse.</param>
    /// <param name="mouseY">The y-coordinate of the mouse.</param>
    /// <returns>true if the right-click action was successfully performed; otherwise, false.</returns>
    public bool RightClick(int mouseX, int mouseY) => false;

    /// <summary>Updates the tab component based on the mouse position.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse position.</param>
    /// <param name="mouseY">The y-coordinate of the mouse position.</param>
    public void Update(int mouseX, int mouseY) { }
}