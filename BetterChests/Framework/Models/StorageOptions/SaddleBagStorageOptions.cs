namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewValley.Buildings;

/// <inheritdoc />
internal sealed class SaddleBagStorageOptions : ChildStorageOptions
{
    private readonly Stable stable;

    /// <summary>Initializes a new instance of the <see cref="SaddleBagStorageOptions" /> class.</summary>
    /// <param name="getDefault">Get the default storage options.</param>
    /// <param name="stable">The stable whose horse saddle bag this represents.</param>
    public SaddleBagStorageOptions(Func<IStorageOptions> getDefault, Stable stable)
        : base(getDefault, new CustomFieldsStorageOptions(SaddleBagStorageOptions.GetCustomFields(stable))) =>
        this.stable = stable;

    /// <inheritdoc />
    public override string StorageName
    {
        get => this.stable.getStableHorse().Name;
        set { }
    }

    /// <inheritdoc />
    public override string GetDisplayName() => I18n.Storage_Saddlebag_Name();

    /// <inheritdoc />
    public override string GetDescription() => I18n.Storage_Saddlebag_Tooltip();

    private static Func<bool, Dictionary<string, string>> GetCustomFields(Stable stable) =>
        init =>
        {
            if (init)
            {
                stable.GetData().CustomFields ??= [];
            }

            return stable.GetData().CustomFields ?? [];
        };
}