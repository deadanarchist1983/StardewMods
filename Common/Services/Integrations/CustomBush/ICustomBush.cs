namespace StardewMods.Common.Services.Integrations.CustomBush;

using StardewValley.GameData;

/// <summary>Model used for custom bushes.</summary>
public interface ICustomBush
{
    /// <summary>Gets the age needed to produce.</summary>
    public int AgeToProduce { get; }

    /// <summary>Gets the day of month to begin producing.</summary>
    public int DayToBeginProducing { get; }

    /// <summary>Gets the description of the bush.</summary>
    public string Description { get; }

    /// <summary>Gets the display name of the bush.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the default texture used when planted indoors.</summary>
    public string IndoorTexture { get; }

    /// <summary>Gets the season in which this bush will produce its drops.</summary>
    public List<Season> Seasons { get; }

    /// <summary>Gets the rules which override the locations that custom bushes can be planted in.</summary>
    public List<PlantableRule> PlantableLocationRules { get; }

    /// <summary>Gets the texture of the tea bush.</summary>
    public string Texture { get; }

    /// <summary>Gets the row index for the custom bush's sprites.</summary>
    public int TextureSpriteRow { get; }

    /// <summary>Retrieves the items produced by the custom bush.</summary>
    /// <returns>An enumerable collection of objects implementing the ICustomBushDrop interface. Each object represents an item produced by the custom bush.</returns>
    public IEnumerable<ICustomBushDrop> GetItemsProduced();
}