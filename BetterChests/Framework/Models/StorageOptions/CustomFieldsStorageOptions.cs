namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

/// <inheritdoc />
internal sealed class CustomFieldsStorageOptions : DictionaryStorageOptions
{
    private readonly Func<Dictionary<string, string>> getData;

    /// <summary>Initializes a new instance of the <see cref="CustomFieldsStorageOptions" /> class.</summary>
    /// <param name="getData">Get the custom field data.</param>
    public CustomFieldsStorageOptions(Func<Dictionary<string, string>> getData) => this.getData = getData;

    /// <inheritdoc />
    protected override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
        this.getData().TryGetValue(key, out value);

    /// <inheritdoc />
    protected override void SetValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            this.getData().Remove(key);
            return;
        }

        this.getData()[key] = value;
    }
}