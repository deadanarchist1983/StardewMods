namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

/// <inheritdoc />
internal class BuildingStorageOptions : ChildStorageOptions
{
    private static readonly Dictionary<string, IStorageOptions> ChildOptions = new();

    private readonly string buildingType;

    /// <summary>Initializes a new instance of the <see cref="BuildingStorageOptions" /> class.</summary>
    /// <param name="getDefault">Get the default storage options.</param>
    /// <param name="buildingType">The building type.</param>
    public BuildingStorageOptions(Func<IStorageOptions> getDefault, string buildingType)
        : base(getDefault, BuildingStorageOptions.GetChild(buildingType)) =>
        this.buildingType = buildingType;

    /// <summary>Gets the building data.</summary>
    public BuildingData Data =>
        Game1.buildingData.TryGetValue(this.buildingType, out var buildingData) ? buildingData : new BuildingData();

    /// <inheritdoc />
    public override string GetDescription() =>
        this.buildingType switch
        {
            "Stable" => I18n.Storage_Saddlebag_Tooltip(), _ => TokenParser.ParseText(this.Data.Description),
        };

    /// <inheritdoc />
    public override string GetDisplayName() =>
        this.buildingType switch
        {
            "Stable" => I18n.Storage_Saddlebag_Name(), _ => TokenParser.ParseText(this.Data.Name),
        };

    private static Func<IStorageOptions> GetChild(string buildingType) =>
        () =>
        {
            if (BuildingStorageOptions.ChildOptions.TryGetValue(buildingType, out var storageOptions))
            {
                return storageOptions;
            }

            storageOptions = new CustomFieldsStorageOptions(BuildingStorageOptions.GetCustomFields(buildingType));

            BuildingStorageOptions.ChildOptions.Add(buildingType, storageOptions);
            return storageOptions;
        };

    private static Func<Dictionary<string, string>> GetCustomFields(string buildingType) =>
        () =>
        {
            if (!Game1.buildingData.TryGetValue(buildingType, out var buildingData))
            {
                buildingData = new BuildingData();
            }

            return buildingData.CustomFields ?? [];
        };
}