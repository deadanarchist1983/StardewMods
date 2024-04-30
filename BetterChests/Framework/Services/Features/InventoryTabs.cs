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
    private readonly IInputHelper inputHelper;
    private readonly MenuManager menuManager;
    private readonly PerScreen<List<TabComponent>> tabs = new(() => []);

    /// <summary>Initializes a new instance of the <see cref="InventoryTabs" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="menuManager">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public InventoryTabs(
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        MenuManager menuManager,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.inputHelper = inputHelper;
        this.menuManager = menuManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.InventoryTabs != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var container = this.menuManager.Top.Container;
        if (container is null
            || !this.tabs.Value.Any()
            || this.menuManager.CurrentMenu is not ItemGrabMenu
            || !this.menuManager.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA: break;

            case SButton.MouseRight or SButton.ControllerB: break;
        }
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        var container = this.menuManager.Top.Container;
        var top = this.menuManager.Top;
        this.tabs.Value.Clear();
        if (top.Menu is null || container?.Options.InventoryTabs is not FeatureOption.Enabled) { }
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        var container = this.menuManager.Top.Container;
        if (!this.tabs.Value.Any()
            || this.menuManager.CurrentMenu is not ItemGrabMenu itemGrabMenu
            || container is null)
        {
            return;
        }

        foreach (var tab in this.tabs.Value)
        {
            tab.Draw(e.SpriteBatch);
        }
    }
}