namespace StardewMods.BetterChests.Framework.Models.Containers;

using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc cref="IStorageContainer{TSource}" />
internal abstract class BaseContainer<TSource> : BaseContainer, IStorageContainer<TSource>
    where TSource : class
{
    /// <summary>Initializes a new instance of the <see cref="BaseContainer{TSource}" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    protected BaseContainer(IStorageOptions baseOptions)
        : base(baseOptions, typeof(TSource) == typeof(Farmer)) { }

    /// <inheritdoc />
    public abstract bool IsAlive { get; }

    /// <inheritdoc />
    public abstract WeakReference<TSource> Source { get; }

    /// <inheritdoc />
    protected override ItemGrabMenu GetItemGrabMenu(
        bool playSound = false,
        bool reverseGrab = false,
        bool showReceivingMenu = true,
        bool snapToBottom = false,
        bool canBeExitedWithKey = true,
        bool playRightClickSound = true,
        bool allowRightClick = true,
        bool showOrganizeButton = true,
        int source = ItemGrabMenu.source_chest,
        Item? sourceItem = null,
        int whichSpecialButton = -1,
        object? context = null) =>
        base.GetItemGrabMenu(
            playSound,
            reverseGrab,
            showReceivingMenu,
            snapToBottom,
            canBeExitedWithKey,
            playRightClickSound,
            allowRightClick,
            showOrganizeButton,
            source,
            sourceItem,
            whichSpecialButton,
            this.Source.TryGetTarget(out var target) ? target : context);
}

/// <inheritdoc />
internal abstract class BaseContainer : IStorageContainer
{
    private readonly IStorageOptions baseOptions;
    private readonly Lazy<IStorageOptions> storageOptions;

    /// <summary>Initializes a new instance of the <see cref="BaseContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="isFarmer">Indicates if the container is for a Farmer.</param>
    protected BaseContainer(IStorageOptions baseOptions, bool isFarmer = false)
    {
        this.baseOptions = baseOptions;
        if (isFarmer)
        {
            this.storageOptions = new Lazy<IStorageOptions>(() => baseOptions);
            return;
        }

        this.storageOptions = new Lazy<IStorageOptions>(this.InitializeStorageOptions);
    }

    /// <inheritdoc />
    public string DisplayName => this.baseOptions.GetDisplayName();

    /// <inheritdoc />
    public string Description => this.baseOptions.GetDescription();

    /// <inheritdoc />
    public abstract int Capacity { get; }

    /// <inheritdoc />
    public IStorageOptions Options => this.storageOptions.Value;

    /// <inheritdoc />
    public abstract IInventory Items { get; }

    /// <inheritdoc />
    public abstract GameLocation Location { get; }

    /// <inheritdoc />
    public abstract Vector2 TileLocation { get; }

    /// <inheritdoc />
    public abstract ModDataDictionary ModData { get; }

    /// <inheritdoc />
    public abstract NetMutex? Mutex { get; }

    /// <inheritdoc />
    public void ForEachItem(Func<Item, bool> action)
    {
        for (var index = this.Items.Count - 1; index >= 0; --index)
        {
            if (this.Items[index] is null)
            {
                continue;
            }

            if (!action(this.Items[index]))
            {
                break;
            }
        }
    }

    /// <inheritdoc />
    public virtual void ShowMenu(bool playSound = false) =>
        Game1.activeClickableMenu = this.GetItemGrabMenu(playSound, context: this);

    /// <inheritdoc />
    public abstract bool TryAdd(Item item, out Item? remaining);

    /// <inheritdoc />
    public abstract bool TryRemove(Item item);

    /// <inheritdoc />
    public virtual void GrabItemFromInventory(Item? item, Farmer who)
    {
        if (item is null)
        {
            return;
        }

        if (item.Stack == 0)
        {
            item.Stack = 1;
        }

        if (!this.TryAdd(item, out var remaining))
        {
            return;
        }

        if (remaining == null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            remaining = who.addItemToInventory(remaining);
        }

        var oldID = Game1.activeClickableMenu.currentlySnappedComponent != null
            ? Game1.activeClickableMenu.currentlySnappedComponent.myID
            : -1;

        this.ShowMenu();
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        itemGrabMenu.heldItem = remaining;
        if (oldID == -1)
        {
            return;
        }

        Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
        Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
    }

    /// <inheritdoc />
    public virtual void GrabItemFromChest(Item? item, Farmer who)
    {
        if (item is null || !who.couldInventoryAcceptThisItem(item))
        {
            return;
        }

        this.Items.Remove(item);
        this.Items.RemoveEmptySlots();
        this.ShowMenu();
    }

    /// <inheritdoc />
    public virtual bool HighlightItems(Item? item) => InventoryMenu.highlightAllItems(item);

    /// <inheritdoc />
    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(this.Options.StorageName))
        {
            return this.Options.StorageName.Trim();
        }

        var sb = new StringBuilder();

        sb.Append(this.DisplayName);
        sb.Append(" at ");
        sb.Append(this.Location?.DisplayName ?? "Unknown");
        sb.Append(CultureInfo.InvariantCulture, $"({this.TileLocation.X:n0}, {this.TileLocation.Y:n0})");
        return sb.ToString();
    }

    /// <summary>Opens an item grab menu for this container.</summary>
    /// <param name="playSound">Whether to play the container open sound.</param>
    /// <param name="reverseGrab">Indicates if an item can be held rather than placed back into the chest.</param>
    /// <param name="showReceivingMenu">Indicates whether the top menu is displayed.</param>
    /// <param name="snapToBottom">Indicates whether the menu will be moved to the bottom of the screen.</param>
    /// <param name="canBeExitedWithKey">Indicates whether the menu can be exited with the menu key.</param>
    /// <param name="playRightClickSound">Indicates whether sound can be played on right-click.</param>
    /// <param name="allowRightClick">Indicates whether right-click can be used for interactions other than tool attachments.</param>
    /// <param name="showOrganizeButton">Indicates whether the organize button will be shown.</param>
    /// <param name="source">
    /// Indicates the source of the <see cref="ItemGrabMenu" />. (0 - none, 1 - chest, 2 - gift, 3 -
    /// fishing chest, 4 - overflow).
    /// </param>
    /// <param name="sourceItem">The source item of the <see cref="ItemGrabMenu" />.</param>
    /// <param name="whichSpecialButton">Indicates whether the Junimo toggle button will be shown.</param>
    /// <param name="context">The context of the <see cref="ItemGrabMenu" />.</param>
    /// <returns>The <see cref="ItemGrabMenu" />.</returns>
    protected virtual ItemGrabMenu GetItemGrabMenu(
        bool playSound = false,
        bool reverseGrab = false,
        bool showReceivingMenu = true,
        bool snapToBottom = false,
        bool canBeExitedWithKey = true,
        bool playRightClickSound = true,
        bool allowRightClick = true,
        bool showOrganizeButton = true,
        int source = ItemGrabMenu.source_chest,
        Item? sourceItem = null,
        int whichSpecialButton = -1,
        object? context = null)
    {
        if (playSound)
        {
            Game1.player.currentLocation.localSound("openChest");
        }

        return new ItemGrabMenu(
            this.Items,
            reverseGrab,
            showReceivingMenu,
            this.HighlightItems,
            this.GrabItemFromInventory,
            null,
            this.GrabItemFromChest,
            snapToBottom,
            canBeExitedWithKey,
            playRightClickSound,
            allowRightClick,
            showOrganizeButton,
            source,
            sourceItem,
            whichSpecialButton,
            context);
    }

    private IStorageOptions InitializeStorageOptions()
    {
        var child = new ModDataStorageOptions(this.ModData);

        // Initialize Storage Name
        if (string.IsNullOrWhiteSpace(child.StorageName)
            && this.ModData.TryGetValue("Pathoschild.ChestsAnywhere/Name", out var name)
            && !string.IsNullOrWhiteSpace(name)
            && this.ModData.TryGetValue("Pathoschild.ChestsAnywhere/Category", out var category)
            && !string.IsNullOrWhiteSpace(category))
        {
            child.StorageName = $"{category} - {name}";
        }

        return new ChildStorageOptions(GetParent, () => child);

        IStorageOptions GetParent() => this.baseOptions;
    }
}