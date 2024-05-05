namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.ItemTypeDefinitions;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class CategorizeChest : BaseFeature<CategorizeChest>
{
    private readonly PerScreen<List<Item>> cachedItems = new(() => []);
    private readonly ICacheTable<ISearchExpression?> cachedSearches;
    private readonly MenuHandler menuHandler;
    private readonly SearchHandler searchHandler;

    /// <summary>Initializes a new instance of the <see cref="CategorizeChest" /> class.</summary>
    /// <param name="cacheManager">Dependency used for managing cache tables.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="searchHandler">Dependency used for handling search.</param>
    public CategorizeChest(
        CacheManager cacheManager,
        IEventManager eventManager,
        MenuHandler menuHandler,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        SearchHandler searchHandler)
        : base(eventManager, log, manifest, modConfig)
    {
        this.cachedSearches = cacheManager.GetCacheTable<ISearchExpression?>();
        this.menuHandler = menuHandler;
        this.searchHandler = searchHandler;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CategorizeChest != FeatureOption.Disabled;

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

    private static IEnumerable<Item> GetItems(Func<Item, bool>? predicate)
    {
        foreach (var item in GetAll())
        {
            if (predicate is null || predicate(item))
            {
                yield return item;
            }

            if (item is not SObject obj
                || obj.bigCraftable.Value
                || item.QualifiedItemId == "(O)447"
                || item.QualifiedItemId == "(O)812")
            {
                continue;
            }

            // Add silver quality item
            obj = (SObject)item.getOne();
            obj.Quality = SObject.medQuality;
            if (predicate is null || predicate(obj))
            {
                yield return obj;
            }

            // Add gold quality item
            obj = (SObject)item.getOne();
            obj.Quality = SObject.highQuality;
            if (predicate is null || predicate(obj))
            {
                yield return obj;
            }

            // Add iridium quality item
            obj = (SObject)item.getOne();
            obj.Quality = SObject.bestQuality;
            if (predicate is null || predicate(obj))
            {
                yield return obj;
            }
        }

        yield break;

        IEnumerable<Item> GetAll(bool flavored = true, params string[]? identifiers)
        {
            foreach (var itemType in ItemRegistry.ItemTypes)
            {
                if (identifiers is not null && identifiers.Any() && !identifiers.Contains(itemType.Identifier))
                {
                    continue;
                }

                var definition = ItemRegistry.GetTypeDefinition(itemType.Identifier);
                foreach (var itemId in itemType.GetAllIds())
                {
                    var item = ItemRegistry.Create(itemType.Identifier + itemId);
                    if (!flavored)
                    {
                        yield return item;

                        continue;
                    }

                    switch (definition)
                    {
                        case ObjectDataDefinition objectDataDefinition:
                            if (item.QualifiedItemId == "(O)340")
                            {
                                yield return objectDataDefinition.CreateFlavoredHoney(null);

                                continue;
                            }

                            var ingredient = item as SObject;
                            switch (item.Category)
                            {
                                case SObject.FruitsCategory:
                                    yield return objectDataDefinition.CreateFlavoredWine(ingredient);
                                    yield return objectDataDefinition.CreateFlavoredJelly(ingredient);
                                    yield return objectDataDefinition.CreateFlavoredDriedFruit(ingredient);

                                    break;

                                case SObject.VegetableCategory:
                                    yield return objectDataDefinition.CreateFlavoredJuice(ingredient);
                                    yield return objectDataDefinition.CreateFlavoredPickle(ingredient);

                                    break;

                                case SObject.flowersCategory:
                                    yield return objectDataDefinition.CreateFlavoredHoney(ingredient);

                                    break;

                                case SObject.FishCategory:
                                    yield return objectDataDefinition.CreateFlavoredBait(ingredient);
                                    yield return objectDataDefinition.CreateFlavoredSmokedFish(ingredient);

                                    break;

                                case SObject.sellAtFishShopCategory when item.QualifiedItemId == "(O)812":
                                    foreach (var fishPondData in DataLoader.FishPondData(Game1.content))
                                    {
                                        if (fishPondData.ProducedItems.All(
                                            producedItem => producedItem.ItemId != item.QualifiedItemId))
                                        {
                                            continue;
                                        }

                                        foreach (var fishPondItem in GetAll(false, "(O)"))
                                        {
                                            if (fishPondItem is SObject fishPondObject
                                                && fishPondData.RequiredTags.All(fishPondItem.HasContextTag))
                                            {
                                                yield return objectDataDefinition.CreateFlavoredRoe(fishPondObject);
                                                yield return objectDataDefinition.CreateFlavoredAgedRoe(fishPondObject);
                                            }
                                        }
                                    }

                                    break;
                            }

                            if (item.HasContextTag("edible_mushroom"))
                            {
                                yield return objectDataDefinition.CreateFlavoredDriedMushroom(item as SObject);
                            }

                            yield return item;

                            break;

                        default:
                            yield return item;

                            break;
                    }
                }
            }
        }
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        var top = this.menuHandler.Top.Container;
        if (e.Container == this.menuHandler.Bottom.Container
            && top is
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
        if (e.Container == top && !e.Container.Items.Contains(e.Item))
        {
            e.UnHighlight();
        }
    }

    [Priority(int.MinValue + 1)]
    private void OnItemsDisplaying(ItemsDisplayingEventArgs e)
    {
        // Append searched items to the end of the list
        if (e.Container == this.menuHandler.Top.Container
            && e.Container.CategorizeChest is FeatureOption.Enabled
            && this.cachedItems.Value.Any())
        {
            e.Edit(items => items.Concat(this.cachedItems.Value.Except(e.Container.Items)));
        }
    }

    [Priority(int.MinValue)]
    private void OnItemTransferring(ItemTransferringEventArgs e)
    {
        // Only test if categorize is enabled
        if (e.Into.CategorizeChest != FeatureOption.Enabled || !this.CanAcceptItem(e.Into, e.Item, out var accepted))
        {
            return;
        }

        if (accepted)
        {
            e.AllowTransfer();
        }
        else if (e.Into.CategorizeChestBlockItems == FeatureOption.Enabled)
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

        this.cachedItems.Value = [..CategorizeChest.GetItems(e.SearchExpression.PartialMatch)];
    }

    private bool CanAcceptItem(IStorageContainer container, Item item, out bool accepted)
    {
        accepted = false;
        var includeStacks = container.CategorizeChestIncludeStacks == FeatureOption.Enabled;
        var hasStacks = container.Items.ContainsId(item.QualifiedItemId);
        if (includeStacks && hasStacks)
        {
            accepted = true;
            return true;
        }

        // Cannot handle if there is no search term
        if (string.IsNullOrWhiteSpace(container.CategorizeChestSearchTerm))
        {
            return false;
        }

        // Retrieve search expression from cache or generate a new one
        if (!this.cachedSearches.TryGetValue(container.CategorizeChestSearchTerm, out var searchExpression))
        {
            this.cachedSearches.AddOrUpdate(
                container.CategorizeChestSearchTerm,
                this.searchHandler.TryParseExpression(container.CategorizeChestSearchTerm, out searchExpression)
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