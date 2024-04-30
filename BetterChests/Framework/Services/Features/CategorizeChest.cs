namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.ItemTypeDefinitions;

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
    private readonly MenuManager menuManager;
    private readonly SearchHandler searchHandler;

    /// <summary>Initializes a new instance of the <see cref="CategorizeChest" /> class.</summary>
    /// <param name="cacheManager">Dependency used for managing cache tables.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="menuManager">Dependency used for managing the current menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    public CategorizeChest(
        CacheManager cacheManager,
        IEventManager eventManager,
        MenuManager menuManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        SearchHandler searchHandler)
        : base(eventManager, log, manifest, modConfig)
    {
        this.cachedSearches = cacheManager.GetCacheTable<ISearchExpression?>();
        this.menuManager = menuManager;
        this.searchHandler = searchHandler;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CategorizeChest != FeatureOption.Disabled;

    private static IEnumerable<Item> GetItems(Func<Item, bool>? predicate)
    {
        foreach (var itemType in ItemRegistry.ItemTypes)
        {
            var definition = ItemRegistry.GetTypeDefinition(itemType.Identifier);
            foreach (var itemId in itemType.GetAllIds())
            {
                var item = ItemRegistry.Create(itemType.Identifier + itemId);
                switch (definition)
                {
                    case BigCraftableDataDefinition bigCraftableDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case BootsDataDefinition bootsDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case FlooringDataDefinition flooringDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case FurnitureDataDefinition furnitureDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case HatDataDefinition hatDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case MannequinDataDefinition mannequinDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case ObjectDataDefinition objectDataDefinition:
                        if (item.QualifiedItemId == "(O)340")
                        {
                            item = objectDataDefinition.CreateFlavoredHoney(null);
                        }

                        var ingredient = item as SObject;
                        switch (item.Category)
                        {
                            case SObject.FruitsCategory:
                                var wine = objectDataDefinition.CreateFlavoredWine(ingredient);
                                var jelly = objectDataDefinition.CreateFlavoredJelly(ingredient);
                                var driedFruit = objectDataDefinition.CreateFlavoredDriedFruit(ingredient);
                                break;

                            case SObject.VegetableCategory:
                                var juice = objectDataDefinition.CreateFlavoredJuice(ingredient);
                                var pickle = objectDataDefinition.CreateFlavoredPickle(ingredient);
                                break;

                            case SObject.flowersCategory:
                                var honey = objectDataDefinition.CreateFlavoredHoney(ingredient);
                                break;
                        }

                        if (item.HasContextTag("edible_mushroom"))
                        {
                            var driedMushroom = objectDataDefinition.CreateFlavoredDriedMushroom(item as SObject);
                        }

                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case PantsDataDefinition pantsDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case ShirtDataDefinition shirtDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case ToolDataDefinition toolDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case TrinketDataDefinition trinketDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case WallpaperDataDefinition wallpaperDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;

                    case WeaponDataDefinition weaponDataDefinition:
                        if (predicate is null || predicate(item))
                        {
                            yield return item;
                        }

                        break;
                }
            }
        }
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Subscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Subscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Unsubscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
        this.Events.Unsubscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        var top = this.menuManager.Top.Container;
        if (e.Container == this.menuManager.Bottom.Container
            && top?.Options is
            {
                CategorizeChest: FeatureOption.Enabled,
                CategorizeChestBlockItems: FeatureOption.Enabled,
            })
        {
            if (this.CanAcceptItem(top, e.Item, out var accepted) && !accepted)
            {
                e.UnHighlight();
            }

            return;
        }

        // Unhighlight items not actually in container
        if (e.Container == this.menuManager.Top.Container && this.cachedItems.Value.Any())
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
        if (e.Container == this.menuManager.Top.Container && this.cachedItems.Value.Any())
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
        else if (e.Into.Options.CategorizeChestBlockItems == FeatureOption.Enabled)
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
        var includeStacks = container.Options.CategorizeChestIncludeStacks == FeatureOption.Enabled;
        var hasStacks = container.Items.ContainsId(item.QualifiedItemId);
        if (includeStacks && hasStacks)
        {
            accepted = true;
            return true;
        }

        // Cannot handle if there is no search term
        if (string.IsNullOrWhiteSpace(container.Options.CategorizeChestSearchTerm))
        {
            return false;
        }

        // Retrieve search expression from cache or generate a new one
        if (!this.cachedSearches.TryGetValue(container.Options.CategorizeChestSearchTerm, out var searchExpression))
        {
            this.cachedSearches.AddOrUpdate(
                container.Options.CategorizeChestSearchTerm,
                this.searchHandler.TryParseExpression(container.Options.CategorizeChestSearchTerm, out searchExpression)
                    ? searchExpression
                    : null);
        }

        // Cannot handle if search term is invalid
        if (searchExpression is null)
        {
            return false;
        }

        // Check if item matches search expressions
        accepted = searchExpression.PartialMatch(item);
        return true;
    }
}