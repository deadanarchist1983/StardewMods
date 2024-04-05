namespace StardewMods.Common.Services.Integrations.CustomBush;

using StardewValley.TerrainFeatures;

/// <summary>Mod API for custom bushes.</summary>
public interface ICustomBushApi
{
    /// <summary>Gets the data model for all Custom Bush.</summary>
    public Dictionary<string, ICustomBush> Data { get; }

    /// <summary>Determines if the given Bush instance is a custom bush.</summary>
    /// <param name="bush">The bush instance to check.</param>
    /// <returns>True if the bush is a custom bush, otherwise false.</returns>
    public bool IsCustomBush(Bush bush);

    /// <summary>Tries to get the custom bush model associated with the given bush.</summary>
    /// <param name="bush">The bush.</param>
    /// <param name="customBush">When this method returns, contains the custom bush associated with the given bush, if found; otherwise, it contains null.</param>
    /// <returns>true if the custom bush associated with the given bush is found; otherwise, false.</returns>
    public bool TryGetCustomBush(Bush bush, out ICustomBush? customBush);
}