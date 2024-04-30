namespace StardewMods.BetterChests.Framework;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <summary>Extension methods for Better Chests.</summary>
internal static class Extensions
{
    /// <summary>Executes the specified action for each config in the class.</summary>
    /// <param name="config">The config.</param>
    /// <param name="action">The action to be performed for each config.</param>
    public static void ForEachConfig(this IModConfig config, Action<string, object> action)
    {
        action(nameof(config.AccessChestsShowArrows), config.AccessChestsShowArrows);
        action(nameof(config.CarryChestLimit), config.CarryChestLimit);
        action(nameof(config.CarryChestSlowAmount), config.CarryChestSlowAmount);
        action(nameof(config.CraftFromChestDisableLocations), config.CraftFromChestDisableLocations);
        action(nameof(config.HslColorPickerHueSteps), config.HslColorPickerHueSteps);
        action(nameof(config.HslColorPickerSaturationSteps), config.HslColorPickerSaturationSteps);
        action(nameof(config.HslColorPickerLightnessSteps), config.HslColorPickerLightnessSteps);
        action(nameof(config.HslColorPickerLightnessSteps), config.HslColorPickerLightnessSteps);
        action(nameof(config.InventoryTabList), config.InventoryTabList);
        action(nameof(config.LockItem), config.LockItem);
        action(nameof(config.LockItemHold), config.LockItemHold);
        action(nameof(config.SearchItemsMethod), config.SearchItemsMethod);
        action(nameof(config.StashToChestDisableLocations), config.StashToChestDisableLocations);
        action(nameof(config.Controls), config.Controls);
        action(nameof(config.DefaultOptions), config.DefaultOptions);
        action(nameof(config.StorageOptions), config.StorageOptions);
    }

    /// <summary>Executes the specified action for each option in the class.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="action">The action to be performed for each option.</param>
    public static void ForEachOption(this IStorageOptions options, Action<string, object> action)
    {
        action(nameof(options.AccessChest), options.AccessChest);
        action(nameof(options.AutoOrganize), options.AutoOrganize);
        action(nameof(options.CarryChest), options.CarryChest);
        action(nameof(options.CategorizeChest), options.CategorizeChest);
        action(nameof(options.CategorizeChestBlockItems), options.CategorizeChestBlockItems);
        action(nameof(options.CategorizeChestSearchTerm), options.CategorizeChestSearchTerm);
        action(nameof(options.CategorizeChestIncludeStacks), options.CategorizeChestIncludeStacks);
        action(nameof(options.ChestFinder), options.ChestFinder);
        action(nameof(options.ChestInfo), options.ChestInfo);
        action(nameof(options.CollectItems), options.CollectItems);
        action(nameof(options.ConfigureChest), options.ConfigureChest);
        action(nameof(options.CookFromChest), options.CookFromChest);
        action(nameof(options.CraftFromChest), options.CraftFromChest);
        action(nameof(options.CraftFromChestDistance), options.CraftFromChestDistance);
        action(nameof(options.HslColorPicker), options.HslColorPicker);
        action(nameof(options.InventoryTabs), options.InventoryTabs);
        action(nameof(options.OpenHeldChest), options.OpenHeldChest);
        action(nameof(options.ResizeChest), options.ResizeChest);
        action(nameof(options.ResizeChestCapacity), options.ResizeChestCapacity);
        action(nameof(options.SearchItems), options.SearchItems);
        action(nameof(options.ShopFromChest), options.ShopFromChest);
        action(nameof(options.StashToChest), options.StashToChest);
        action(nameof(options.StashToChestDistance), options.StashToChestDistance);
        action(nameof(options.StashToChestPriority), options.StashToChestPriority);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns>true if the options exists; otherwise, false.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out FeatureOption value)
    {
        value = name switch
        {
            nameof(options.AutoOrganize) => options.AutoOrganize,
            nameof(options.CarryChest) => options.CarryChest,
            nameof(options.CategorizeChest) => options.CategorizeChest,
            nameof(options.CategorizeChestBlockItems) => options.CategorizeChestBlockItems,
            nameof(options.CategorizeChestIncludeStacks) => options.CategorizeChestIncludeStacks,
            nameof(options.ChestFinder) => options.ChestFinder,
            nameof(options.ChestInfo) => options.ChestInfo,
            nameof(options.CollectItems) => options.CollectItems,
            nameof(options.ConfigureChest) => options.ConfigureChest,
            nameof(options.HslColorPicker) => options.HslColorPicker,
            nameof(options.InventoryTabs) => options.InventoryTabs,
            nameof(options.OpenHeldChest) => options.OpenHeldChest,
            nameof(options.SearchItems) => options.SearchItems,
            nameof(options.ShopFromChest) => options.ShopFromChest,
            _ => default(FeatureOption),
        };

        return value is not default(FeatureOption);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns>true if the options exists; otherwise, false.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out RangeOption value)
    {
        value = name switch
        {
            nameof(options.AccessChest) => options.AccessChest,
            nameof(options.CookFromChest) => options.CookFromChest,
            nameof(options.CraftFromChest) => options.CraftFromChest,
            nameof(options.StashToChest) => options.StashToChest,
            _ => default(RangeOption),
        };

        return value is not default(RangeOption);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns>true if the options exists; otherwise, false.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out ChestMenuOption value)
    {
        value = name switch { nameof(options.ResizeChest) => options.ResizeChest, _ => default(ChestMenuOption) };

        return value is not default(ChestMenuOption);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns>true if the options exists; otherwise, false.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out StashPriority value)
    {
        value = name switch
        {
            nameof(options.StashToChestPriority) => options.StashToChestPriority, _ => default(StashPriority),
        };

        return value is not default(StashPriority);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns>true if the options exists; otherwise, false.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out string value)
    {
        value = name switch
        {
            nameof(options.CategorizeChestSearchTerm) => options.CategorizeChestSearchTerm, _ => string.Empty,
        };

        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>ets the value of the specified option from the given storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value of the specified option or null.</param>
    /// <returns>true if the options exists; otherwise, false.</returns>
    public static bool TryGetOption(this IStorageOptions options, string name, out int value)
    {
        value = name switch
        {
            nameof(options.CraftFromChestDistance) => options.CraftFromChestDistance,
            nameof(options.ResizeChestCapacity) => options.ResizeChestCapacity,
            nameof(options.StashToChestDistance) => options.StashToChestDistance,
            _ => 0,
        };

        return value != 0;
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, FeatureOption value)
    {
        switch (name)
        {
            case nameof(options.AutoOrganize):
                options.AutoOrganize = value;
                break;
            case nameof(options.CarryChest):
                options.CarryChest = value;
                break;
            case nameof(options.CategorizeChest):
                options.CategorizeChest = value;
                break;
            case nameof(options.CategorizeChestBlockItems):
                options.CategorizeChestBlockItems = value;
                break;
            case nameof(options.CategorizeChestIncludeStacks):
                options.CategorizeChestIncludeStacks = value;
                break;
            case nameof(options.ChestFinder):
                options.ChestFinder = value;
                break;
            case nameof(options.ChestInfo):
                options.ChestInfo = value;
                break;
            case nameof(options.CollectItems):
                options.CollectItems = value;
                break;
            case nameof(options.ConfigureChest):
                options.ConfigureChest = value;
                break;
            case nameof(options.HslColorPicker):
                options.HslColorPicker = value;
                break;
            case nameof(options.InventoryTabs):
                options.InventoryTabs = value;
                break;
            case nameof(options.OpenHeldChest):
                options.OpenHeldChest = value;
                break;
            case nameof(options.SearchItems):
                options.SearchItems = value;
                break;
            case nameof(options.ShopFromChest):
                options.ShopFromChest = value;
                break;
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, RangeOption value)
    {
        switch (name)
        {
            case nameof(options.AccessChest):
                options.AccessChest = value;
                break;
            case nameof(options.CookFromChest):
                options.CookFromChest = value;
                break;
            case nameof(options.CraftFromChest):
                options.CraftFromChest = value;
                break;
            case nameof(options.StashToChest):
                options.StashToChest = value;
                break;
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, string value)
    {
        switch (name)
        {
            case nameof(options.CategorizeChestSearchTerm):
                options.CategorizeChestSearchTerm = value;
                break;
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, int value)
    {
        switch (name)
        {
            case nameof(options.CraftFromChestDistance):
                options.CraftFromChestDistance = value;
                break;
            case nameof(options.ResizeChestCapacity):
                options.ResizeChestCapacity = value;
                break;
            case nameof(options.StashToChestDistance):
                options.StashToChestDistance = value;
                break;
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, ChestMenuOption value)
    {
        switch (name)
        {
            case nameof(options.ResizeChest):
                options.ResizeChest = value;
                break;
        }
    }

    /// <summary>Sets the value of a specific option in the storage options.</summary>
    /// <param name="options">The storage options.</param>
    /// <param name="name">The name of the option.</param>
    /// <param name="value">The value to set.</param>
    public static void SetOption(this IStorageOptions options, string name, StashPriority value)
    {
        switch (name)
        {
            case nameof(options.StashToChestPriority):
                options.StashToChestPriority = value;
                break;
        }
    }

    /// <summary>Tests whether the player is within range of the location.</summary>
    /// <param name="range">The range.</param>
    /// <param name="distance">The distance in tiles to the player.</param>
    /// <param name="parent">The context where the source object is contained.</param>
    /// <param name="position">The coordinates.</param>
    /// <returns>true if the location is within range; otherwise, false.</returns>
    public static bool WithinRange(this RangeOption range, int distance, object parent, Vector2 position) =>
        range switch
        {
            RangeOption.World => true,
            RangeOption.Inventory when parent is Farmer farmer && farmer.Equals(Game1.player) => true,
            RangeOption.Default or RangeOption.Disabled or RangeOption.Inventory => false,
            RangeOption.Location when parent is GameLocation location && !location.Equals(Game1.currentLocation) =>
                false,
            RangeOption.Location when distance == -1 => true,
            RangeOption.Location when Math.Abs(position.X - Game1.player.Tile.X)
                + Math.Abs(position.Y - Game1.player.Tile.Y)
                <= distance => true,
            _ => false,
        };
}