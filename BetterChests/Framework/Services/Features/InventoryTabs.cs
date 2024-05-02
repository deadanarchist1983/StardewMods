namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Adds inventory tabs to the side of the <see cref="ItemGrabMenu" />.</summary>
internal sealed class InventoryTabs : BaseFeature<InventoryTabs>
{
    private readonly AssetHandler assetHandler;
    private readonly IInputHelper inputHelper;
    private readonly MenuManager menuManager;
    private readonly PerScreen<ISearchExpression?> searchExpression;
    private readonly SearchHandler searchHandler;
    private readonly PerScreen<string> searchText;
    private readonly PerScreen<List<TabComponent>> tabs = new(() => []);

    /// <summary>Initializes a new instance of the <see cref="InventoryTabs" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="menuManager">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchExpression">Dependency for retrieving a parsed search expression.</param>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    /// <param name="searchText">Dependency for retrieving the unified search text.</param>
    public InventoryTabs(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        MenuManager menuManager,
        IModConfig modConfig,
        PerScreen<ISearchExpression?> searchExpression,
        SearchHandler searchHandler,
        PerScreen<string> searchText)
        : base(eventManager, log, manifest, modConfig)
    {
        this.assetHandler = assetHandler;
        this.inputHelper = inputHelper;
        this.menuManager = menuManager;
        this.searchExpression = searchExpression;
        this.searchHandler = searchHandler;
        this.searchText = searchText;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.InventoryTabs != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var container = this.menuManager.Top.Container;
        if (container?.Options.InventoryTabs is not FeatureOption.Enabled
            || !this.tabs.Value.Any()
            || this.menuManager.CurrentMenu is not ItemGrabMenu
            || !this.menuManager.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA:
                if (this.tabs.Value.Any(tab => tab.LeftClick(mouseX, mouseY)))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                return;

            case SButton.MouseRight or SButton.ControllerB:
                if (this.tabs.Value.Any(tab => tab.RightClick(mouseX, mouseY)))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                return;
        }
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        var container = this.menuManager.Top.Container;
        var top = this.menuManager.Top;
        this.tabs.Value.Clear();

        if (this.menuManager.CurrentMenu is not ItemGrabMenu itemGrabMenu
            || top.Menu is null
            || container?.Options.InventoryTabs is not FeatureOption.Enabled)
        {
            return;
        }

        var x = itemGrabMenu.xPositionOnScreen - Game1.tileSize - (IClickableMenu.borderWidth / 2);
        var y = top.Menu.inventory[0].bounds.Y;

        foreach (var inventoryTab in this.Config.InventoryTabList)
        {
            if (!this.assetHandler.TabIcons.TryGetValue(inventoryTab.Icon, out var tabIcon))
            {
                continue;
            }

            this.tabs.Value.Add(
                new TabComponent(
                    x,
                    y,
                    tabIcon,
                    inventoryTab,
                    () =>
                    {
                        this.Log.Trace("{0}: Switching tab to {1}.", this.Id, inventoryTab.Label);
                        this.searchText.Value = inventoryTab.SearchTerm;
                        this.searchExpression.Value =
                            this.searchHandler.TryParseExpression(inventoryTab.SearchTerm, out var expression)
                                ? expression
                                : null;

                        this.Events.Publish(new SearchChangedEventArgs(this.searchExpression.Value));
                    }));

            y += Game1.tileSize;
        }
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (!this.tabs.Value.Any() || this.menuManager.CurrentMenu is not ItemGrabMenu)
        {
            return;
        }

        foreach (var tab in this.tabs.Value)
        {
            tab.Draw(e.SpriteBatch);
        }
    }

    private void OnRenderingActiveMenu(RenderingActiveMenuEventArgs e)
    {
        if (!this.tabs.Value.Any() || this.menuManager.CurrentMenu is not ItemGrabMenu)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        foreach (var tab in this.tabs.Value)
        {
            tab.Update(mouseX, mouseY);
        }
    }
}