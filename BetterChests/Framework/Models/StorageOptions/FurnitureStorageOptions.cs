namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

/// <inheritdoc />
internal sealed class FurnitureStorageOptions : ChildStorageOptions
{
    private readonly string itemId;

    /// <summary>Initializes a new instance of the <see cref="FurnitureStorageOptions" /> class.</summary>
    /// <param name="getParent">Get the parent storage options.</param>
    /// <param name="getChild">The child storage options.</param>
    /// <param name="itemId">Get the furniture item id.</param>
    public FurnitureStorageOptions(Func<IStorageOptions> getParent, Func<IStorageOptions> getChild, string itemId)
        : base(getParent, getChild) =>
        this.itemId = itemId;

    /// <summary>Gets the item data.</summary>
    public ParsedItemData Data => ItemRegistry.RequireTypeDefinition("(F)").GetData(this.itemId);

    /// <inheritdoc />
    public override string GetDescription() => TokenParser.ParseText(this.Data.Description);

    /// <inheritdoc />
    public override string GetDisplayName() => TokenParser.ParseText(this.Data.DisplayName);
}