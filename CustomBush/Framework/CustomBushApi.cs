namespace StardewMods.CustomBush.Framework;

using StardewMods.Common.Services.Integrations.CustomBush;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.CustomBush.Framework.Services;
using StardewValley.GameData;
using StardewValley.TerrainFeatures;

/// <inheritdoc />
public sealed class CustomBushApi : ICustomBushApi
{
    private readonly BushManager bushManager;
    private readonly ILog log;
    private readonly IModInfo modInfo;

    /// <summary>Initializes a new instance of the <see cref="CustomBushApi" /> class.</summary>
    /// <param name="bushManager">Dependency for managing custom bushes.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    internal CustomBushApi(BushManager bushManager, IModInfo modInfo, ILog log)
    {
        this.bushManager = bushManager;
        this.modInfo = modInfo;
        this.log = log;
    }

    /// <inheritdoc />
    public bool IsCustomBush(Bush bush) => this.bushManager.IsCustomBush(bush);

    /// <inheritdoc />
    public bool TryGetCustomBush(Bush bush, out ICustomBush? customBush)
    {
        if (this.bushManager.TryGetCustomBush(bush, out var customBushInstance))
        {
            customBush = customBushInstance;
            return true;
        }

        customBush = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetDrops(
        Bush bush,
        out IEnumerable<(GenericSpawnItemDataWithCondition, Season? Season, float Chance)>? drops) =>
        this.bushManager.TryGetDrops(bush, out drops);
}