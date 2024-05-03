namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests;
using StardewValley.GameData.Locations;

/// <inheritdoc />
internal sealed class LocationStorageOptions : ChildStorageOptions
{
    private static readonly Dictionary<string, IStorageOptions> ChildOptions = new();

    private readonly string locationName;

    /// <summary>Initializes a new instance of the <see cref="LocationStorageOptions" /> class.</summary>
    /// <param name="getDefault">Get the default storage options.</param>
    /// <param name="locationName">The location name.</param>
    public LocationStorageOptions(Func<IStorageOptions> getDefault, string locationName)
        : base(getDefault, LocationStorageOptions.GetChild(locationName)) =>
        this.locationName = locationName;

    /// <summary>Gets the location data.</summary>
    public LocationData Data =>
        DataLoader.Locations(Game1.content).GetValueOrDefault(this.locationName) ?? new LocationData();

    /// <inheritdoc />
    public override string GetDescription() => I18n.Storage_Fridge_Tooltip();

    /// <inheritdoc />
    public override string GetDisplayName() => I18n.Storage_Fridge_Name();

    private static Func<IStorageOptions> GetChild(string locationName) =>
        () =>
        {
            if (LocationStorageOptions.ChildOptions.TryGetValue(locationName, out var storageOptions))
            {
                return storageOptions;
            }

            storageOptions = new CustomFieldsStorageOptions(LocationStorageOptions.GetCustomFields(locationName));

            LocationStorageOptions.ChildOptions.Add(locationName, storageOptions);
            return storageOptions;
        };

    private static Func<Dictionary<string, string>> GetCustomFields(string locationName) =>
        () =>
        {
            var locationData =
                DataLoader.Locations(Game1.content).GetValueOrDefault(locationName) ?? new LocationData();

            return locationData.CustomFields;
        };
}