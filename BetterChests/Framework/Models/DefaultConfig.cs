namespace StardewMods.BetterChests.Framework.Models;

using System.Globalization;
using System.Text;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewValley.Menus;

/// <summary>Mod config data for Better Chests.</summary>
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions { get; set; } = new()
    {
        AccessChest = RangeOption.Location,
        AutoOrganize = FeatureOption.Enabled,
        CarryChest = FeatureOption.Enabled,
        CategorizeChest = FeatureOption.Enabled,
        CategorizeChestIncludeStacks = FeatureOption.Enabled,
        ChestFinder = FeatureOption.Enabled,
        ConfigureChest = FeatureOption.Enabled,
        CookFromChest = RangeOption.Location,
        CraftFromChest = RangeOption.Location,
        CraftFromChestDistance = -1,
        HslColorPicker = FeatureOption.Enabled,
        InventoryTabs = FeatureOption.Enabled,
        OpenHeldChest = FeatureOption.Enabled,
        ResizeChest = ChestMenuOption.Large,
        ResizeChestCapacity = 70,
        SearchItems = FeatureOption.Enabled,
        ShopFromChest = FeatureOption.Enabled,
        StashToChest = RangeOption.Location,
        StashToChestDistance = 16,
    };

    /// <inheritdoc />
    public Dictionary<string, Dictionary<string, DefaultStorageOptions>> StorageOptions { get; set; } = [];

    /// <inheritdoc />
    public bool AccessChestsShowArrows { get; set; } = true;

    /// <inheritdoc />
    public int CarryChestLimit { get; set; } = 3;

    /// <inheritdoc />
    public float CarryChestSlowAmount { get; set; } = -1f;

    /// <inheritdoc />
    public int CarryChestSlowLimit { get; set; } = 1;

    /// <inheritdoc />
    public Controls Controls { get; set; } = new();

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public int HslColorPickerHueSteps { get; set; } = 29;

    /// <inheritdoc />
    public int HslColorPickerSaturationSteps { get; set; } = 16;

    /// <inheritdoc />
    public int HslColorPickerLightnessSteps { get; set; } = 16;

    /// <inheritdoc />
    public InventoryMenu.BorderSide HslColorPickerPlacement { get; set; } = InventoryMenu.BorderSide.Right;

    /// <inheritdoc />
    public List<InventoryTab> InventoryTabList { get; set; } =
    [
        new InventoryTab
        {
            Icon = "furyx639.BetterChests/Clothing",
            Label = I18n.Tabs_Clothing_Name(),
            SearchTerm = "category_clothing category_boots category_hat",
        },
        new InventoryTab
        {
            Icon = "furyx639.BetterChests/Cooking",
            Label = I18n.Tabs_Cooking_Name(),
            SearchTerm =
                "category_syrup category_artisan_goods category_ingredients category_sell_at_pierres_and_marnies category_sell_at_pierres category_meat category_cooking category_milk category_egg",
        },
        new InventoryTab
        {
            Icon = "furyx639.BetterChests/Crops",
            Label = I18n.Tabs_Crops_Name(),
            SearchTerm = "category_greens category_flowers category_fruits category_vegetable",
        },
        new InventoryTab
        {
            Icon = "furyx639.BetterChests/Equipment",
            Label = I18n.Tabs_Equipment_Name(),
            SearchTerm = "category_equipment category_ring category_tool category_weapon",
        },
        new InventoryTab
        {
            Icon = "furyx639.BetterChests/Fishing",
            Label = I18n.Tabs_Fishing_Name(),
            SearchTerm = "category_bait category_fish category_tackle category_sell_at_fish_shop",
        },
        new InventoryTab
        {
            Icon = "furyx639.BetterChests/Materials",
            Label = I18n.Tabs_Materials_Name(),
            SearchTerm =
                "category_monster_loot category_metal_resources category_building_resources category_minerals category_crafting category_gem",
        },
        new InventoryTab
        {
            Icon = "furyx639.BetterChests/Miscellaneous",
            Label = I18n.Tabs_Misc_Name(),
            SearchTerm = "category_big_craftable category_furniture category_junk",
        },
        new InventoryTab
        {
            Icon = "furyx639.BetterChests/Seeds",
            Label = I18n.Tabs_Seeds_Name(),
            SearchTerm = "category_seeds category_fertilizer",
        },
    ];

    /// <inheritdoc />
    public FeatureOption LockItem { get; set; }

    /// <inheritdoc />
    public bool LockItemHold { get; set; } = true;

    /// <inheritdoc />
    public FilterMethod SearchItemsMethod { get; set; } = FilterMethod.GrayedOut;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.AccessChestsShowArrows)}: {this.AccessChestsShowArrows}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CarryChestLimit)}: {this.CarryChestLimit}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.CarryChestSlowLimit)}: {this.CarryChestSlowLimit}");
        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.CraftFromChestDisableLocations)}: {string.Join(", ", this.CraftFromChestDisableLocations)}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.HslColorPickerHueSteps)}: {this.HslColorPickerHueSteps}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.HslColorPickerSaturationSteps)}: {this.HslColorPickerSaturationSteps}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.HslColorPickerLightnessSteps)}: {this.HslColorPickerLightnessSteps}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.HslColorPickerPlacement)}: {this.HslColorPickerPlacement}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.InventoryTabList)}: {string.Join(", ", this.InventoryTabList)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.LockItem)}: {this.LockItem}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.LockItemHold)}: {this.LockItemHold}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.SearchItemsMethod)}: {this.SearchItemsMethod}");

        sb.AppendLine(
            CultureInfo.InvariantCulture,
            $"{nameof(this.StashToChestDisableLocations)}: {string.Join(", ", this.StashToChestDisableLocations)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.DefaultOptions)}: {this.DefaultOptions}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"{nameof(this.Controls)}: {this.Controls}");

        return sb.ToString();
    }
}