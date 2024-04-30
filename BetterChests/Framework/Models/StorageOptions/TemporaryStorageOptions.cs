namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

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
                        break;

                    case RangeOption when storageOptions.TryGetOption(name, out RangeOption rangeOption):
                        this.SetOption(name, rangeOption);
                        break;

                    case ChestMenuOption when storageOptions.TryGetOption(name, out ChestMenuOption chestMenuOption):
                        this.SetOption(name, chestMenuOption);
                        break;

                    case StashPriority when storageOptions.TryGetOption(name, out StashPriority stashPriority):
                        this.SetOption(name, stashPriority);
                        break;

                    case string when storageOptions.TryGetOption(name, out string stringValue):
                        this.SetOption(name, stringValue);
                        break;

                    case int when storageOptions.TryGetOption(name, out int intValue):
                        this.SetOption(name, intValue);
                        break;
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
                        break;

                    case RangeOption when this.defaultOptions.TryGetOption(name, out RangeOption rangeOption):
                        this.SetOption(name, rangeOption);
                        break;

                    case ChestMenuOption when this.defaultOptions.TryGetOption(
                        name,
                        out ChestMenuOption chestMenuOption):
                        this.SetOption(name, chestMenuOption);
                        break;

                    case StashPriority when this.defaultOptions.TryGetOption(name, out StashPriority stashPriority):
                        this.SetOption(name, stashPriority);
                        break;

                    case string when this.defaultOptions.TryGetOption(name, out string stringValue):
                        this.SetOption(name, stringValue);
                        break;

                    case int when this.defaultOptions.TryGetOption(name, out int intValue):
                        this.SetOption(name, intValue);
                        break;
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
                        this.SetOption(name, featureOption);
                        break;

                    case RangeOption when this.TryGetOption(name, out RangeOption rangeOption):
                        this.SetOption(name, rangeOption);
                        break;

                    case ChestMenuOption when this.TryGetOption(name, out ChestMenuOption chestMenuOption):
                        this.SetOption(name, chestMenuOption);
                        break;

                    case StashPriority when this.TryGetOption(name, out StashPriority stashPriority):
                        this.SetOption(name, stashPriority);
                        break;

                    case string when this.TryGetOption(name, out string stringValue):
                        this.SetOption(name, stringValue);
                        break;

                    case int when this.TryGetOption(name, out int intValue):
                        this.SetOption(name, intValue);
                        break;
                }
            });
}