namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <inheritdoc />
internal class ChildStorageOptions : IStorageOptions
{
    private readonly Func<IStorageOptions> getChild;
    private readonly Func<IStorageOptions> getParent;

    /// <summary>Initializes a new instance of the <see cref="ChildStorageOptions" /> class.</summary>
    /// <param name="getParent">Get the parent storage options.</param>
    /// <param name="getChild">Get the child storage options.</param>
    public ChildStorageOptions(Func<IStorageOptions> getParent, Func<IStorageOptions> getChild)
    {
        this.getChild = getChild;
        this.getParent = getParent;
    }

    private IStorageOptions Parent => this.getParent();

    private IStorageOptions Child => this.getChild().GetActualOptions();

    /// <inheritdoc />
    public RangeOption AccessChest
    {
        get => this.Get(storage => storage.AccessChest);
        set => this.Child.AccessChest = value == this.Parent.AccessChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public int AccessChestPriority
    {
        get => this.Child.AccessChestPriority == 0 ? this.Parent.AccessChestPriority : this.Child.AccessChestPriority;
        set => this.Child.AccessChestPriority = value == this.Parent.AccessChestPriority ? 0 : value;
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(storage => storage.AutoOrganize);
        set => this.Child.AutoOrganize = value == this.Parent.AutoOrganize ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(storage => storage.CarryChest);
        set => this.Child.CarryChest = value == this.Parent.CarryChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(storage => storage.CategorizeChest);
        set => this.Child.CategorizeChest = value == this.Parent.CategorizeChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestBlockItems
    {
        get => this.Get(storage => storage.CategorizeChestBlockItems);
        set =>
            this.Child.CategorizeChestBlockItems =
                value == this.Parent.CategorizeChestBlockItems ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public string CategorizeChestSearchTerm
    {
        get =>
            string.IsNullOrWhiteSpace(this.Child.CategorizeChestSearchTerm)
                ? this.Parent.CategorizeChestSearchTerm
                : this.Child.CategorizeChestSearchTerm;
        set => this.Child.CategorizeChestSearchTerm = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChestIncludeStacks
    {
        get => this.Get(storage => storage.CategorizeChestIncludeStacks);
        set =>
            this.Child.CategorizeChestIncludeStacks =
                value == this.Parent.CategorizeChestIncludeStacks ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(storage => storage.ChestFinder);
        set => this.Child.ChestFinder = value == this.Parent.ChestFinder ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(storage => storage.CollectItems);
        set => this.Child.CollectItems = value == this.Parent.CollectItems ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(storage => storage.ConfigureChest);
        set => this.Child.ConfigureChest = value == this.Parent.ConfigureChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public RangeOption CookFromChest
    {
        get => this.Get(storage => storage.CookFromChest);
        set => this.Child.CookFromChest = value == this.Parent.CookFromChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(storage => storage.CraftFromChest);
        set => this.Child.CraftFromChest = value == this.Parent.CraftFromChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get =>
            this.Child.CraftFromChestDistance == 0
                ? this.Parent.CraftFromChestDistance
                : this.Child.CraftFromChestDistance;
        set => this.Child.CraftFromChestDistance = value == this.Parent.CraftFromChestDistance ? 0 : value;
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(storage => storage.HslColorPicker);
        set => this.Child.HslColorPicker = value == this.Parent.HslColorPicker ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(storage => storage.InventoryTabs);
        set => this.Child.InventoryTabs = value == this.Parent.InventoryTabs ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(storage => storage.OpenHeldChest);
        set => this.Child.OpenHeldChest = value == this.Parent.OpenHeldChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public ChestMenuOption ResizeChest
    {
        get => this.Get(storage => storage.ResizeChest);
        set => this.Child.ResizeChest = value == this.Parent.ResizeChest ? ChestMenuOption.Default : value;
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.Child.ResizeChestCapacity == 0 ? this.Parent.ResizeChestCapacity : this.Child.ResizeChestCapacity;
        set => this.Child.ResizeChestCapacity = value == this.Parent.ResizeChestCapacity ? 0 : value;
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(storage => storage.SearchItems);
        set => this.Child.SearchItems = value == this.Parent.SearchItems ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption ShopFromChest
    {
        get => this.Get(storage => storage.ShopFromChest);
        set => this.Child.ShopFromChest = value == this.Parent.ShopFromChest ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption SortInventory
    {
        get => this.Get(storage => storage.SortInventory);
        set => this.Child.SortInventory = value == this.Parent.SortInventory ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public string SortInventoryBy
    {
        get =>
            string.IsNullOrWhiteSpace(this.Child.SortInventoryBy)
                ? this.Parent.SortInventoryBy
                : this.Child.SortInventoryBy;
        set =>
            this.Child.SortInventoryBy = value.Equals(this.Parent.SortInventoryBy, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : value;
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(storage => storage.StashToChest);
        set => this.Child.StashToChest = value == this.Parent.StashToChest ? RangeOption.Default : value;
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get =>
            this.Child.StashToChestDistance == 0 ? this.Parent.StashToChestDistance : this.Child.StashToChestDistance;
        set => this.Child.StashToChestDistance = value == this.Parent.StashToChestDistance ? 0 : value;
    }

    /// <inheritdoc />
    public StashPriority StashToChestPriority
    {
        get =>
            this.Child.StashToChestPriority == 0 ? this.Parent.StashToChestPriority : this.Child.StashToChestPriority;
        set => this.Child.StashToChestPriority = value == this.Parent.StashToChestPriority ? 0 : value;
    }

    /// <inheritdoc />
    public string StorageIcon
    {
        get => this.Child.StorageIcon;
        set => this.Child.StorageIcon = value;
    }

    /// <inheritdoc />
    public FeatureOption StorageInfo
    {
        get => this.Get(storage => storage.StorageInfo);
        set => this.Child.StorageInfo = value == this.Parent.StorageInfo ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public FeatureOption StorageInfoHover
    {
        get => this.Get(storage => storage.StorageInfoHover);
        set => this.Child.StorageInfoHover = value == this.Parent.StorageInfoHover ? FeatureOption.Default : value;
    }

    /// <inheritdoc />
    public virtual string StorageName
    {
        get => this.Child.StorageName;
        set => this.Child.StorageName = value;
    }

    /// <inheritdoc />
    public virtual string GetDescription() => this.Parent.GetDescription();

    /// <inheritdoc />
    public IStorageOptions GetActualOptions() => this.Child;

    /// <inheritdoc />
    public IStorageOptions GetParentOptions() => this.Parent;

    /// <inheritdoc />
    public virtual string GetDisplayName() => this.Parent.GetDisplayName();

    private ChestMenuOption Get(Func<IStorageOptions, ChestMenuOption> selector)
    {
        var childValue = selector(this.Child);
        var parentValue = selector(this.Parent);
        return childValue switch
        {
            _ when parentValue is ChestMenuOption.Disabled or ChestMenuOption.Default => ChestMenuOption.Disabled,
            ChestMenuOption.Default => parentValue,
            _ => childValue,
        };
    }

    private FeatureOption Get(Func<IStorageOptions, FeatureOption> selector)
    {
        var childValue = selector(this.Child);
        var parentValue = selector(this.Parent);
        return childValue switch
        {
            _ when parentValue is FeatureOption.Disabled or FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => parentValue,
            _ => childValue,
        };
    }

    private RangeOption Get(Func<IStorageOptions, RangeOption> selector)
    {
        var childValue = selector(this.Child);
        var parentValue = selector(this.Parent);
        return childValue switch
        {
            _ when parentValue is RangeOption.Disabled or RangeOption.Default => RangeOption.Disabled,
            RangeOption.Default => parentValue,
            _ => childValue,
        };
    }
}