namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Customize how an inventory gets sorted.</summary>
internal sealed class SortInventory : BaseFeature<SortInventory>
{
    private readonly ContainerHandler containerHandler;
    private readonly IInputHelper inputHelper;
    private readonly MenuManager menuManager;

    /// <summary>Initializes a new instance of the <see cref="SortInventory" /> class.</summary>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="menuManager">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public SortInventory(
        ContainerHandler containerHandler,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        MenuManager menuManager,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.containerHandler = containerHandler;
        this.inputHelper = inputHelper;
        this.menuManager = menuManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.SortInventory != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Subscribe<ContainerSortingEventArgs>(this.OnContainerSorting);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Unsubscribe<ContainerSortingEventArgs>(this.OnContainerSorting);
    }

    private static string GenerateSortKey(Item item, string sortBy) => string.Empty;

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var container = this.menuManager.Top.Container;
        if (container?.Options.SortInventory is not FeatureOption.Enabled
            || this.menuManager.CurrentMenu is not ItemGrabMenu itemGrabMenu
            || !this.menuManager.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA:
                if (itemGrabMenu.organizeButton.containsPoint(mouseX, mouseY))
                {
                    this.inputHelper.Suppress(e.Button);
                    Game1.playSound("Ship");
                    this.containerHandler.Sort(container);
                }

                return;

            case SButton.MouseRight or SButton.ControllerB:
                if (itemGrabMenu.organizeButton.containsPoint(mouseX, mouseY))
                {
                    this.inputHelper.Suppress(e.Button);
                    Game1.playSound("Ship");
                    this.containerHandler.Sort(container, true);
                }

                return;
        }
    }

    private void OnContainerSorting(ContainerSortingEventArgs e)
    {
        if (e.Container.Options.SortInventory is not FeatureOption.Enabled
            || string.IsNullOrWhiteSpace(e.Container.Options.SortInventoryBy))
        {
            return;
        }

        // Check if any keys needs to be generated
        foreach (var item in e.Container.Items)
        {
            if (item.modData.TryGetValue(this.UniqueId, out var sortByKey)
                && sortByKey.Equals(e.Container.Options.SortInventoryBy, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Generate the key
            item.modData[this.UniqueId] = e.Container.Options.SortInventoryBy;
            item.modData[this.Prefix + "Key"] =
                SortInventory.GenerateSortKey(item, e.Container.Options.SortInventoryBy);
        }

        // Sort inventory by the key
        var copy = e.Container.Items.OrderBy(i => i.modData[this.Prefix + "Key"]).ToList();
        e.Container.Items.OverwriteWith(copy);
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        var container = this.menuManager.Top.Container;
        var top = this.menuManager.Top;

        if (this.menuManager.CurrentMenu is not ItemGrabMenu itemGrabMenu
            || top.Menu is null
            || container?.Options.SortInventory is not FeatureOption.Enabled) { }
    }
}