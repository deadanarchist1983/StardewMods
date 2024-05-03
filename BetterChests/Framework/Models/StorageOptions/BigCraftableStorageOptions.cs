namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.GameData.BigCraftables;
using StardewValley.TokenizableStrings;

/// <inheritdoc />
internal sealed class BigCraftableStorageOptions : ChildStorageOptions
{
    private static readonly Dictionary<string, IStorageOptions> ChildOptions = new();

    private readonly string itemId;

    /// <summary>Initializes a new instance of the <see cref="BigCraftableStorageOptions" /> class.</summary>
    /// <param name="getDefault">Get the default storage options.</param>
    /// <param name="itemId">he big craftable object id.</param>
    public BigCraftableStorageOptions(Func<IStorageOptions> getDefault, string itemId)
        : base(getDefault, BigCraftableStorageOptions.GetChild(itemId)) =>
        this.itemId = itemId;

    /// <summary>Gets the big craftable data.</summary>
    public BigCraftableData Data =>
        Game1.bigCraftableData.TryGetValue(this.itemId, out var bigCraftableData)
            ? bigCraftableData
            : new BigCraftableData();

    /// <inheritdoc />
    public override string GetDescription() => TokenParser.ParseText(this.Data.Description);

    /// <inheritdoc />
    public override string GetDisplayName() => TokenParser.ParseText(this.Data.DisplayName);

    private static Func<IStorageOptions> GetChild(string itemId) =>
        () =>
        {
            if (BigCraftableStorageOptions.ChildOptions.TryGetValue(itemId, out var storageOptions))
            {
                return storageOptions;
            }

            storageOptions = new CustomFieldsStorageOptions(BigCraftableStorageOptions.GetCustomFields(itemId));
            BigCraftableStorageOptions.ChildOptions.Add(itemId, storageOptions);
            return storageOptions;
        };

    private static Func<Dictionary<string, string>> GetCustomFields(string itemId) =>
        () =>
        {
            if (!Game1.bigCraftableData.TryGetValue(itemId, out var bigCraftableData))
            {
                bigCraftableData = new BigCraftableData();
            }

            return bigCraftableData.CustomFields ?? [];
        };
}