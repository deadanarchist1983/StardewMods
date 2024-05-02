namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <inheritdoc />
internal sealed class TemporaryStorageOptions : DefaultStorageOptions
{
    private readonly IStorageOptions defaultOptions;
    private readonly IStorageOptions storageOptions;

    /// <summary>Initializes a new instance of the <see cref="TemporaryStorageOptions" /> class.</summary>
    /// <param name="storageOptions">The storage options to copy.</param>
    /// <param name="defaultOptions">The default storage options.</param>
    public TemporaryStorageOptions(IStorageOptions storageOptions, IStorageOptions defaultOptions)
    {
        this.storageOptions = storageOptions;
        this.defaultOptions = defaultOptions;
        this.ForEachOption(
            (name, option) =>
            {
                switch (option)
                {
                    case FeatureOption when storageOptions.TryGetOption(name, out FeatureOption featureOption):
                        this.SetOption(name, featureOption);
                        return;

                    case RangeOption when storageOptions.TryGetOption(name, out RangeOption rangeOption):
                        this.SetOption(name, rangeOption);
                        return;

                    case ChestMenuOption when storageOptions.TryGetOption(name, out ChestMenuOption chestMenuOption):
                        this.SetOption(name, chestMenuOption);
                        return;

                    case StashPriority when storageOptions.TryGetOption(name, out StashPriority stashPriority):
                        this.SetOption(name, stashPriority);
                        return;

                    case string when storageOptions.TryGetOption(name, out string stringValue):
                        this.SetOption(name, stringValue);
                        return;

                    case int when storageOptions.TryGetOption(name, out int intValue):
                        this.SetOption(name, intValue);
                        return;
                }
            });
    }

    /// <inheritdoc />
    public override string GetDisplayName() => this.storageOptions.GetDisplayName();

    /// <inheritdoc />
    public override string GetDescription() => this.storageOptions.GetDescription();

    /// <summary>Saves the options back to the default.</summary>
    public void Reset() =>
        this.ForEachOption(
            (name, option) =>
            {
                switch (option)
                {
                    case FeatureOption when this.defaultOptions.TryGetOption(name, out FeatureOption featureOption):
                        this.SetOption(name, featureOption);
                        return;

                    case RangeOption when this.defaultOptions.TryGetOption(name, out RangeOption rangeOption):
                        this.SetOption(name, rangeOption);
                        return;

                    case ChestMenuOption when this.defaultOptions.TryGetOption(
                        name,
                        out ChestMenuOption chestMenuOption):
                        this.SetOption(name, chestMenuOption);
                        return;

                    case StashPriority when this.defaultOptions.TryGetOption(name, out StashPriority stashPriority):
                        this.SetOption(name, stashPriority);
                        return;

                    case string when this.defaultOptions.TryGetOption(name, out string stringValue):
                        this.SetOption(name, stringValue);
                        return;

                    case int when this.defaultOptions.TryGetOption(name, out int intValue):
                        this.SetOption(name, intValue);
                        return;
                }
            });

    /// <summary>Saves the changes back to storage options.</summary>
    public void Save() =>
        this.storageOptions.ForEachOption(
            (name, option) =>
            {
                switch (option)
                {
                    case FeatureOption when this.TryGetOption(name, out FeatureOption featureOption):
                        this.storageOptions.SetOption(name, featureOption);
                        return;

                    case RangeOption when this.TryGetOption(name, out RangeOption rangeOption):
                        this.storageOptions.SetOption(name, rangeOption);
                        return;

                    case ChestMenuOption when this.TryGetOption(name, out ChestMenuOption chestMenuOption):
                        this.storageOptions.SetOption(name, chestMenuOption);
                        return;

                    case StashPriority when this.TryGetOption(name, out StashPriority stashPriority):
                        this.storageOptions.SetOption(name, stashPriority);
                        return;

                    case string when this.TryGetOption(name, out string stringValue):
                        this.storageOptions.SetOption(name, stringValue);
                        return;

                    case int when this.TryGetOption(name, out int intValue):
                        this.storageOptions.SetOption(name, intValue);
                        return;
                }
            });
}