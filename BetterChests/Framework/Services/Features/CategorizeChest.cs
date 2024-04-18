namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class CategorizeChest : BaseFeature<CategorizeChest>
{
    private static readonly Lazy<List<Item>> AllItems = new(
        () =>
        {
            return ItemRegistry
                .ItemTypes.SelectMany(
                    itemType => itemType
                        .GetAllIds()
                        .Select(localId => ItemRegistry.Create(itemType.Identifier + localId)))
                .ToList();
        });

    private readonly PerScreen<List<Item>> cachedItems = new(() => []);
    private readonly ICacheTable<ISearchExpression?> cachedSearches;
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<ClickableTextureComponent> existingStacksButton;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<ClickableTextureComponent> saveButton;
    private readonly SearchHandler searchHandler;
    private readonly PerScreen<string> searchText;

    /// <summary>Initializes a new instance of the <see cref="CategorizeChest" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="cacheManager">Dependency used for managing cache tables.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    /// <param name="searchText">Dependency for retrieving the unified search text.</param>
    public CategorizeChest(
        AssetHandler assetHandler,
        CacheManager cacheManager,
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        SearchHandler searchHandler,
        PerScreen<string> searchText)
        : base(eventManager, log, manifest, modConfig)
    {
        this.cachedSearches = cacheManager.GetCacheTable<ISearchExpression?>();
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.searchHandler = searchHandler;
        this.searchText = searchText;
        this.saveButton = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, Game1.tileSize / 2, Game1.tileSize / 2),
                assetHandler.Icons.Value,
                new Rectangle(142, 0, 16, 16),
                2f)
            {
                name = this.Id,
                hoverText = I18n.Button_SaveAsCategorization_Name(),
                myID = 8_675_309,
            });

        this.existingStacksButton = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 27, 27),
                Game1.mouseCursors,
                new Rectangle(227, 425, 9, 9),
                3f)
            {
                name = this.Id,
                hoverText = I18n.Button_IncludeExistingStacks_Name(),
                myID = 8_675_310,
            });
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CategorizeChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Subscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Subscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Unsubscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
        this.Events.Unsubscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    // private static Func<IEnumerable<Item>, IEnumerable<Item>> FilterByCategory(
    //     IStorageContainer container,
    //     IItemFilter itemMatcher)
    // {
    //     return InternalFilterMethod;
    //
    //     IEnumerable<Item> InternalFilterMethod(IEnumerable<Item> items) =>
    //         container.Options.CategorizeChestMethod switch
    //         {
    //             FilterMethod.Sorted or FilterMethod.GrayedOut => items.OrderByDescending(itemMatcher.MatchesFilter),
    //             FilterMethod.Hidden => items.Where(itemMatcher.MatchesFilter),
    //             _ => items,
    //         };
    // }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var container = this.itemGrabMenuManager.Top.Container;
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || this.itemGrabMenuManager.CurrentMenu is null
            || container is null
            || !this.itemGrabMenuManager.CanFocus(this))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);

        // Copy search text to categorization
        if (this.saveButton.Value.containsPoint(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            container.Options.CategorizeChestSearchTerm = this.searchText.Value;
            return;
        }

        if (!this.existingStacksButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        // Toggle include existing stacks
        this.inputHelper.Suppress(e.Button);
        container.Options.CategorizeChestIncludeStacks =
            container.Options.CategorizeChestIncludeStacks == FeatureOption.Enabled
                ? FeatureOption.Disabled
                : FeatureOption.Enabled;

        this.existingStacksButton.Value.sourceRect =
            container.Options.CategorizeChestIncludeStacks == FeatureOption.Enabled
                ? new Rectangle(236, 425, 9, 9)
                : new Rectangle(227, 425, 9, 9);
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        var container = this.itemGrabMenuManager.Top.Container;
        var top = this.itemGrabMenuManager.Top;
        if (top.Menu is not null && container?.Options.CategorizeChest == FeatureOption.Enabled)
        {
            this.isActive.Value = true;
            var width = Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width);
            var x = top.Columns switch
            {
                3 => top.Menu.inventory[1].bounds.Center.X + (width / 2),
                12 => top.Menu.inventory[5].bounds.Right + (width / 2),
                14 => top.Menu.inventory[6].bounds.Right + (width / 2),
            };

            var y = top.Menu.yPositionOnScreen
                - (IClickableMenu.borderWidth / 2)
                - Game1.tileSize
                - (top.Rows == 3 ? 20 : 4)
                + 8;

            this.saveButton.Value.bounds.X = x;
            this.saveButton.Value.bounds.Y = y;

            this.existingStacksButton.Value.bounds.X = x + 32;
            this.existingStacksButton.Value.bounds.Y = y + 2;
            this.existingStacksButton.Value.sourceRect =
                container.Options.CategorizeChestIncludeStacks == FeatureOption.Enabled
                    ? new Rectangle(236, 425, 9, 9)
                    : new Rectangle(227, 425, 9, 9);
        }
        else
        {
            this.isActive.Value = false;
        }
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.saveButton.Value.tryHover(mouseX, mouseY);
        this.existingStacksButton.Value.tryHover(mouseX, mouseY);

        this.saveButton.Value.draw(e.SpriteBatch);
        this.existingStacksButton.Value.draw(e.SpriteBatch);

        if (this.saveButton.Value.containsPoint(mouseX, mouseY))
        {
            this.itemGrabMenuManager.CurrentMenu.hoverText = this.saveButton.Value.hoverText;
            return;
        }

        if (this.existingStacksButton.Value.containsPoint(mouseX, mouseY))
        {
            this.itemGrabMenuManager.CurrentMenu.hoverText = this.existingStacksButton.Value.hoverText;
        }
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        var top = this.itemGrabMenuManager.Top.Container;
        if (e.Container == this.itemGrabMenuManager.Bottom.Container
            && top?.Options.CategorizeChest == FeatureOption.Enabled)
        {
            if (this.CanAcceptItem(top, e.Item, out var accepted) && !accepted)
            {
                e.UnHighlight();
            }

            return;
        }

        // Unhighlight items not actually in container
        if (e.Container == this.itemGrabMenuManager.Top.Container && this.cachedItems.Value.Any())
        {
            if (!e.Container.Items.Contains(e.Item))
            {
                e.UnHighlight();
            }
        }
    }

    [Priority(int.MinValue + 1)]
    private void OnItemsDisplaying(ItemsDisplayingEventArgs e)
    {
        // Append searched items to the end of the list
        if (e.Container == this.itemGrabMenuManager.Top.Container && this.cachedItems.Value.Any())
        {
            e.Edit(items => items.Concat(this.cachedItems.Value.Except(e.Container.Items)));
        }
    }

    [Priority(int.MinValue)]
    private void OnItemTransferring(ItemTransferringEventArgs e)
    {
        // Only test if categorize is enabled
        if (e.Into.Options.CategorizeChest != FeatureOption.Enabled
            || !this.CanAcceptItem(e.Into, e.Item, out var accepted))
        {
            return;
        }

        if (accepted)
        {
            e.AllowTransfer();
        }
        else
        {
            e.PreventTransfer();
        }
    }

    private void OnSearchChanged(SearchChangedEventArgs e)
    {
        if (e.SearchExpression is null)
        {
            this.cachedItems.Value = [];
            return;
        }

        this.cachedItems.Value = [..CategorizeChest.AllItems.Value.Where(e.SearchExpression.PartialMatch)];
    }

    private bool CanAcceptItem(IStorageContainer container, Item item, out bool accepted)
    {
        accepted = false;
        if (container.Options.CategorizeChestIncludeStacks == FeatureOption.Enabled)
        {
            accepted = container.Items.ContainsId(item.QualifiedItemId);
            return true;
        }

        if (string.IsNullOrWhiteSpace(container.Options.CategorizeChestSearchTerm))
        {
            return false;
        }

        // Return results from cache
        if (this.cachedSearches.TryGetValue(container.Options.CategorizeChestSearchTerm, out var searchExpression))
        {
            if (searchExpression is null)
            {
                return false;
            }

            accepted = searchExpression.PartialMatch(item);
            return true;
        }

        // Parse search term and cache results
        if (this.searchHandler.TryParseExpression(container.Options.CategorizeChestSearchTerm, out searchExpression))
        {
            this.cachedSearches.AddOrUpdate(container.Options.CategorizeChestSearchTerm, searchExpression);
            accepted = searchExpression.PartialMatch(item);
            return true;
        }

        // Cache null result if search term fails to parse
        this.cachedSearches.AddOrUpdate(container.Options.CategorizeChestSearchTerm, null);
        return false;
    }
}