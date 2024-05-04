namespace StardewMods.BetterChests.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Provides access to all known storages for other services.</summary>
internal sealed class ContainerFactory : BaseService
{
    private readonly ConditionalWeakTable<object, IStorageContainer> cachedContainers = new();
    private readonly IModConfig modConfig;
    private readonly ProxyChestFactory proxyChestFactory;
    private readonly Dictionary<string, IStorageOptions> storageOptions = new();

    /// <summary>Initializes a new instance of the <see cref="ContainerFactory" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    public ContainerFactory(ILog log, IManifest manifest, IModConfig modConfig, ProxyChestFactory proxyChestFactory)
        : base(log, manifest)
    {
        this.modConfig = modConfig;
        this.proxyChestFactory = proxyChestFactory;
    }

    /// <summary>Retrieves the storage options for a given item.</summary>
    /// <param name="item">The item for which to retrieve the storage options.</param>
    /// <returns>The storage options for the given item.</returns>
    public IStorageOptions GetStorageOptions(Item item)
    {
        if (this.storageOptions.TryGetValue(item.QualifiedItemId, out var storageOption))
        {
            return storageOption;
        }

        storageOption = new BigCraftableStorageOptions(() => this.modConfig.DefaultOptions, item.ItemId);
        this.storageOptions.Add(item.QualifiedItemId, storageOption);
        return storageOption;
    }

    /// <summary>Retrieves the storage options for a given location.</summary>
    /// <param name="location">The location for which to retrieve the storage options.</param>
    /// <returns>The storage options for the given item.</returns>
    public IStorageOptions GetStorageOptions(GameLocation location)
    {
        if (this.storageOptions.TryGetValue($"(L){location.Name}", out var storageOption))
        {
            return storageOption;
        }

        storageOption = new LocationStorageOptions(() => this.modConfig.DefaultOptions, location.Name);
        this.storageOptions.Add($"(L){location.Name}", storageOption);
        return storageOption;
    }

    /// <summary>Retrieves the storage options for a given building.</summary>
    /// <param name="building">The building for which to retrieve the storage options.</param>
    /// <returns>The storage options for the given item.</returns>
    public IStorageOptions GetStorageOptions(Building building)
    {
        if (this.storageOptions.TryGetValue($"(B){building.buildingType.Value}", out var storageOption))
        {
            return storageOption;
        }

        storageOption = new BuildingStorageOptions(() => this.modConfig.DefaultOptions, building.buildingType.Value);
        this.storageOptions.Add($"(B){building.buildingType.Value}", storageOption);
        return storageOption;
    }

    /// <summary>Retrieves all containers that match the optional predicate.</summary>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAll(Func<IStorageContainer, bool>? predicate = default)
    {
        var foundContainers = new HashSet<IStorageContainer>();
        var containerQueue = new Queue<IStorageContainer>();

        foreach (var container in this.GetAllFromPlayers(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }

        foreach (var container in this.GetAllFromLocations(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }

        foreach (var container in this.GetAllFromContainers(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }
    }

    /// <summary>Retrieves all containers from the specified container that match the optional predicate.</summary>
    /// <param name="parentContainer">The container where the container items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAll(
        IStorageContainer parentContainer,
        Func<IStorageContainer, bool>? predicate = default)
    {
        foreach (var item in parentContainer.Items)
        {
            if (item is null || !this.TryGetAny(item, out var childContainer))
            {
                continue;
            }

            var container = new ChildContainer(parentContainer, childContainer);
            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }
    }

    /// <summary>Retrieves all containers from the specified game location that match the optional predicate.</summary>
    /// <param name="location">The game location where the container will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAll(
        GameLocation location,
        Func<IStorageContainer, bool>? predicate = default)
    {
        // Get container for fridge in location
        if (this.TryGetOne(location, out var container))
        {
            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from buildings
        foreach (var building in location.buildings)
        {
            if (!this.TryGetOne(building, out container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from NPCs
        foreach (var npc in location.characters.OfType<NPC>())
        {
            if (!this.TryGetOne(npc, out container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from placed objects
        foreach (var (pos, obj) in location.Objects.Pairs)
        {
            if (pos.X <= 0 || pos.Y <= 0 || !this.TryGetAny(obj, out container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from placed furniture
        foreach (var furniture in location.furniture.OfType<StorageFurniture>())
        {
            if (!this.TryGetAny(furniture, out container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }
    }

    /// <summary>Retrieves all container items from the specified player matching the optional predicate.</summary>
    /// <param name="farmer">The player whose container items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAll(Farmer farmer, Func<IStorageContainer, bool>? predicate = default)
    {
        // Get container from farmer backpack
        if (!this.TryGetOne(farmer, out var farmerContainer))
        {
            yield break;
        }

        if (predicate is not null && predicate(farmerContainer))
        {
            yield return farmerContainer;
        }

        // Search for containers from farmer inventory
        foreach (var item in farmer.Items)
        {
            if (item is null || !this.TryGetAny(item, out var childContainer))
            {
                continue;
            }

            var container = new ChildContainer(farmerContainer, childContainer);
            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }
    }

    /// <summary>Tries to retrieve a container from the active menu.</summary>
    /// <param name="menu">The menu to retrieve a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(IClickableMenu? menu, [NotNullWhen(true)] out IStorageContainer? container)
    {
        // Get the actual menu
        var actualMenu = menu switch
        {
            { } menuWithChild when menuWithChild.GetChildMenu() is
                { } childMenu => childMenu,
            GameMenu gameMenu => gameMenu.GetCurrentPage(),
            not null => menu,
            _ => Game1.activeClickableMenu,
        };

        // Get a default actual inventory for the menu
        var actualInventory = actualMenu switch
        {
            InventoryMenu inventoryMenu => inventoryMenu.actualInventory,
            ItemGrabMenu
            {
                ItemsToGrabMenu:
                { } itemsToGrabMenu,
                showReceivingMenu: true,
            } => itemsToGrabMenu.actualInventory,
            InventoryPage
            {
                inventory:
                { } inventory,
            } => inventory.actualInventory,
            ShopMenu
            {
                source: StorageFurniture furniture,
            } => furniture.heldItems,
            ShopMenu
            {
                inventory:
                { } inventory,
            } => inventory.actualInventory,
            _ => null,
        };

        // Get the context for the menu
        var context = Game1.activeClickableMenu switch
        {
            ItemGrabMenu itemGrabMenu => itemGrabMenu.context, ShopMenu shopMenu => shopMenu.source, _ => null,
        };

        // Find container based on the context
        container = context switch
        {
            // The same storage container
            IStorageContainer contextContainer when actualInventory is null
                || actualInventory.Equals(contextContainer.Items) => contextContainer,

            // Farmer backpack container
            not null when actualInventory is not null
                && actualInventory.Equals(Game1.player.Items)
                && this.TryGetOne(Game1.player, out var farmerContainer) => farmerContainer,

            // Placed chest container
            Chest chest when actualInventory is not null
                && actualInventory.Equals(chest.GetItemsForPlayer())
                && chest.Location is not null
                && this.TryGetOne(chest.Location, chest.TileLocation, out var placedContainer) => placedContainer,

            // Held chest container
            Chest chest when chest == Game1.player.ActiveObject
                && actualInventory is not null
                && this.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var heldContainer)
                && actualInventory.Equals(heldContainer.Items) => heldContainer,

            // Placed object container
            SObject
                {
                    heldObject.Value: Chest heldChest,
                } obj when actualInventory is not null
                && actualInventory.Equals(heldChest.GetItemsForPlayer())
                && obj.Location is not null
                && this.TryGetOne(obj.Location, obj.TileLocation, out var objectContainer) => objectContainer,

            // Proxy container
            SObject obj when obj == Game1.player.ActiveObject
                && actualInventory is not null
                && this.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var proxyContainer)
                && actualInventory.Equals(proxyContainer.Items) => proxyContainer,

            // Building container
            Building building when this.TryGetOne(building, out var buildingContainer)
                && (actualInventory is null || actualInventory.Equals(buildingContainer.Items)) => buildingContainer,

            // Storage furniture container
            StorageFurniture furniture when actualInventory is not null
                && furniture.heldItems.Equals(actualInventory)
                && this.TryGetOne(furniture, out var furnitureContainer) => furnitureContainer,

            // Return the shipping bin (Chests Anywhere)
            Farm farm when this.TryGetOne(farm.getBuildingByType("Shipping Bin"), out var shippingContainer)
                && (actualInventory is null || actualInventory.Equals(shippingContainer.Items)) => shippingContainer,

            // Return the saddle bag (Horse Overhaul)
            Stable stable when actualInventory is not null
                && this.TryGetOne(stable.getStableHorse(), out var stableContainer)
                && actualInventory.Equals(stableContainer.Items) => stableContainer,
            Horse horse when actualInventory is not null
                && this.TryGetOne(horse, out var horseContainer)
                && actualInventory.Equals(horseContainer.Items) => horseContainer,

            // Unknown or unsupported
            _ => null,
        };

        return container is not null;
    }

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The player whose container will be retrieved.</param>
    /// <param name="index">The index of the player's inventory. Defaults to the active item.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Farmer farmer, int index, [NotNullWhen(true)] out IStorageContainer? container)
    {
        var item = farmer.Items.ElementAtOrDefault(index);
        if (item is null)
        {
            container = null;
            return false;
        }

        if (this.cachedContainers.TryGetValue(item, out container))
        {
            return true;
        }

        if (!this.TryGetAny(item, out var childContainer) || !this.TryGetOne(farmer, out var farmerContainer))
        {
            container = null;
            return false;
        }

        container = new ChildContainer(farmerContainer, childContainer);
        this.cachedContainers.AddOrUpdate(item, container);
        return true;
    }

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The farmer to get a container from.</param>
    /// <param name="farmerContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? farmerContainer)
    {
        if (this.cachedContainers.TryGetValue(farmer, out farmerContainer))
        {
            return true;
        }

        var storageType = new BackpackStorageOptions(farmer);
        farmerContainer = new FarmerContainer(storageType, farmer);
        this.cachedContainers.AddOrUpdate(farmer, farmerContainer);
        return true;
    }

    /// <summary>Tries to get a container from the specified building.</summary>
    /// <param name="building">The building to get a container from.</param>
    /// <param name="buildingContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Building building, [NotNullWhen(true)] out IStorageContainer? buildingContainer)
    {
        if (this.cachedContainers.TryGetValue(building, out buildingContainer))
        {
            return true;
        }

        IStorageOptions? storageType;
        switch (building)
        {
            case ShippingBin shippingBin:
                storageType = this.GetStorageOptions(building);
                buildingContainer = new BuildingContainer(storageType, shippingBin);
                this.cachedContainers.AddOrUpdate(shippingBin, buildingContainer);
                return true;

            default:
                var chest = building.GetBuildingChest("Output");
                if (chest is null)
                {
                    return false;
                }

                if (this.cachedContainers.TryGetValue(chest, out buildingContainer))
                {
                    return true;
                }

                storageType = this.GetStorageOptions(building);
                buildingContainer = new BuildingContainer(storageType, building, chest);
                this.cachedContainers.AddOrUpdate(building, buildingContainer);
                this.cachedContainers.AddOrUpdate(chest, buildingContainer);
                return true;
        }
    }

    /// <summary>Tries to get a container from the specified NPC.</summary>
    /// <param name="npc">The NPC to get a container from.</param>
    /// <param name="npcContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(NPC npc, [NotNullWhen(true)] out IStorageContainer? npcContainer)
    {
        if (this.cachedContainers.TryGetValue(npc, out npcContainer))
        {
            return true;
        }

        switch (npc)
        {
            // Horse Overhaul
            case Horse horse:
                var stable = horse.TryFindStable();
                if (stable is null
                    || !stable.modData.TryGetValue("Goldenrevolver.HorseOverhaul/stableID", out var stableData)
                    || string.IsNullOrWhiteSpace(stableData)
                    || !int.TryParse(stableData, out var stableId)
                    || !Game1.getFarm().Objects.TryGetValue(new Vector2(stableId, 0), out var saddleBag)
                    || saddleBag is not Chest saddleBagChest)
                {
                    return false;
                }

                if (this.cachedContainers.TryGetValue(saddleBagChest, out npcContainer))
                {
                    return true;
                }

                var storageType = this.GetStorageOptions(stable);
                npcContainer = new NpcContainer(storageType, horse, saddleBagChest);
                this.cachedContainers.AddOrUpdate(npc, npcContainer);
                this.cachedContainers.AddOrUpdate(saddleBagChest, npcContainer);
                return true;

            default: return false;
        }
    }

    /// <summary>Tries to get a container from the specified location and position.</summary>
    /// <param name="location">The location to get a container from.</param>
    /// <param name="pos">The position to get a the container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(GameLocation location, Vector2 pos, [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (pos.Equals(Vector2.Zero) && this.TryGetOne(location, out container))
        {
            return true;
        }

        var bounds = new Rectangle(
            (int)(pos.X * Game1.tileSize),
            (int)(pos.Y * Game1.tileSize),
            Game1.tileSize,
            Game1.tileSize);

        // Container is a placed object
        foreach (var obj in location.Objects.Values)
        {
            if (obj.GetBoundingBox().Intersects(bounds) && this.TryGetAny(obj, out container))
            {
                return true;
            }
        }

        // Container is a building
        foreach (var building in location.buildings)
        {
            if (building.GetBoundingBox().Intersects(bounds) && this.TryGetOne(building, out container))
            {
                return true;
            }
        }

        // Container is a storage furniture
        foreach (var furniture in location.furniture.OfType<StorageFurniture>())
        {
            if (furniture.IntersectsForCollision(bounds) && this.TryGetAny(furniture, out container))
            {
                return true;
            }
        }

        // Container is an npc
        foreach (var npc in location.characters.OfType<NPC>())
        {
            if (npc.GetBoundingBox().Intersects(bounds) && this.TryGetOne(npc, out container))
            {
                return true;
            }
        }

        container = null;
        return false;
    }

    /// <summary>Tries to get a container from the specified location.</summary>
    /// <param name="location">The location to get a container from.</param>
    /// <param name="locationContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(GameLocation location, [NotNullWhen(true)] out IStorageContainer? locationContainer)
    {
        if (this.cachedContainers.TryGetValue(location, out locationContainer))
        {
            return true;
        }

        if (location.GetFridge() is not
            { } fridge)
        {
            return false;
        }

        if (this.cachedContainers.TryGetValue(fridge, out locationContainer))
        {
            return true;
        }

        var storageType = this.GetStorageOptions(location);
        locationContainer = new FridgeContainer(storageType, location, fridge);
        this.cachedContainers.AddOrUpdate(fridge, locationContainer);
        this.cachedContainers.AddOrUpdate(location, locationContainer);
        return true;
    }

    /// <summary>Tries to get a container from the specified object.</summary>
    /// <param name="item">The item to get a container from.</param>
    /// <param name="itemContainer">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Item item, [NotNullWhen(true)] out IStorageContainer? itemContainer)
    {
        if (this.cachedContainers.TryGetValue(item, out itemContainer))
        {
            return true;
        }

        itemContainer = this.GetAll(Predicate).FirstOrDefault();
        return itemContainer is not null;

        bool Predicate(IStorageContainer container) =>
            container switch
            {
                ChestContainer chestContainer => chestContainer.Chest == item,
                ObjectContainer objectContainer => objectContainer.Object.heldObject.Value == item,
                _ => false,
            };
    }

    private IEnumerable<IStorageContainer> GetAllFromPlayers(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        foreach (var farmer in Game1.getAllFarmers())
        {
            foreach (var container in this.GetAll(farmer, predicate))
            {
                if (!foundContainers.Add(container))
                {
                    continue;
                }

                containerQueue.Enqueue(container);
                yield return container;
            }
        }
    }

    private IEnumerable<IStorageContainer> GetAllFromLocations(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        var foundLocations = new HashSet<GameLocation>();
        var locationQueue = new Queue<GameLocation>();

        locationQueue.Enqueue(Game1.currentLocation);

        foreach (var location in Game1.locations)
        {
            if (!location.Equals(Game1.currentLocation))
            {
                locationQueue.Enqueue(location);
            }
        }

        while (locationQueue.TryDequeue(out var location))
        {
            if (!foundLocations.Add(location))
            {
                continue;
            }

            foreach (var container in this.GetAll(location, predicate))
            {
                if (!foundContainers.Add(container))
                {
                    continue;
                }

                containerQueue.Enqueue(container);
                yield return container;
            }

            foreach (var building in location.buildings)
            {
                if (building.GetIndoorsType() == IndoorsType.Instanced)
                {
                    locationQueue.Enqueue(building.GetIndoors());
                }
            }
        }
    }

    private IEnumerable<IStorageContainer> GetAllFromContainers(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        while (containerQueue.TryDequeue(out var container))
        {
            foreach (var childContainer in this.GetAll(container, predicate))
            {
                if (!foundContainers.Add(childContainer))
                {
                    continue;
                }

                containerQueue.Enqueue(childContainer);
                yield return childContainer;
            }
        }
    }

    private bool TryGetAny(Item item, [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (this.cachedContainers.TryGetValue(item, out container))
        {
            return true;
        }

        Chest storageChest;
        IStorageOptions? storageOption;
        switch (item)
        {
            case Chest
            {
                playerChest.Value: true,
            } chest:
                storageChest = chest;
                storageOption = this.GetStorageOptions(item);
                container = new ChestContainer(storageOption, storageChest);
                break;

            case SObject
            {
                heldObject.Value: Chest chestObject,
            } obj:
                storageChest = chestObject;
                storageOption = this.GetStorageOptions(item);
                container = new ObjectContainer(storageOption, obj, storageChest);
                break;

            case not null when this.proxyChestFactory.TryGetProxy(item, out var proxyChest):
                storageChest = proxyChest;
                storageOption = this.GetStorageOptions(item);
                container = new ChestContainer(storageOption, storageChest);
                break;

            case StorageFurniture furniture:
                if (!this.storageOptions.TryGetValue(item.QualifiedItemId, out storageOption))
                {
                    if (this.modConfig.StorageOptions.GetValueOrDefault("Furniture")?.GetValueOrDefault(item.ItemId) is
                        null)
                    {
                        return false;
                    }

                    IStorageOptions GetChild() =>
                        this.modConfig.StorageOptions.GetValueOrDefault("Furniture")?.GetValueOrDefault(item.ItemId)
                        ?? new DefaultStorageOptions();

                    storageOption = new FurnitureStorageOptions(
                        () => this.modConfig.DefaultOptions,
                        GetChild,
                        item.ItemId);

                    this.storageOptions.Add(item.QualifiedItemId, storageOption);
                }

                container = new FurnitureContainer(storageOption, furniture);
                this.cachedContainers.AddOrUpdate(item, container);
                return true;

            default:
                container = null;
                return false;
        }

        storageChest.fridge.Value = storageOption.CookFromChest is not RangeOption.Disabled;
        this.cachedContainers.AddOrUpdate(item, container);
        this.cachedContainers.AddOrUpdate(storageChest, container);
        return true;
    }
}