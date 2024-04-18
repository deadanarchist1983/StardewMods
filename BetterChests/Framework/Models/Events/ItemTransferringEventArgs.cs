namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.BetterChests.Interfaces.IItemTransferring" />
internal sealed class ItemTransferringEventArgs : EventArgs, IItemTransferring
{
    private bool isAllowed;
    private bool isPrevented;

    /// <summary>Initializes a new instance of the <see cref="ItemTransferringEventArgs" /> class.</summary>
    /// <param name="into">The container being transferred into.</param>
    /// <param name="item">The item being transferred.</param>
    /// <param name="isAllowedByDefault">Indicates whether the transfer is allowed by default.</param>
    public ItemTransferringEventArgs(IStorageContainer into, Item item, bool isAllowedByDefault)
    {
        this.Into = into;
        this.Item = item;
        this.IsAllowedByDefault = isAllowedByDefault;
    }

    /// <inheritdoc />
    public IStorageContainer Into { get; }

    /// <inheritdoc />
    public Item Item { get; }

    /// <inheritdoc />
    public bool IsAllowed => this.isAllowed && !this.isPrevented;

    /// <inheritdoc />
    public bool IsAllowedByDefault { get; }

    /// <inheritdoc />
    public void AllowTransfer() => this.isAllowed = true;

    /// <inheritdoc />
    public void PreventTransfer() => this.isPrevented = true;
}