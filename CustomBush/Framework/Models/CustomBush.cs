namespace StardewMods.CustomBush.Framework.Models;

using StardewMods.Common.Services.Integrations.CustomBush;
using StardewValley.GameData;

/// <inheritdoc />
internal sealed class CustomBush : ICustomBush
{
    /// <summary>Gets or sets the items produced by this custom bush.</summary>
    public List<CustomBushDrop> ItemsProduced { get; set; } = [];

    /// <inheritdoc />
    public int AgeToProduce { get; set; } = 20;

    /// <inheritdoc />
    public int DayToBeginProducing { get; set; } = 22;

    /// <inheritdoc />
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc />
    public string DisplayName { get; set; } = string.Empty;

    /// <inheritdoc />
    public string IndoorTexture { get; set; } = string.Empty;

    /// <inheritdoc />
    public List<Season> Seasons { get; set; } = [];

    /// <inheritdoc />
    public List<PlantableRule> PlantableLocationRules { get; set; } = [];

    /// <inheritdoc />
    public string Texture { get; set; } = string.Empty;

    /// <inheritdoc />
    public int TextureSpriteRow { get; set; }

    /// <inheritdoc />
    public IEnumerable<ICustomBushDrop> GetItemsProduced() => this.ItemsProduced;
}