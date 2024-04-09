namespace StardewMods.ToolbarIcons.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService<AssetHandler>
{
    /// <summary>Represents the width of the default icon texture.</summary>
    public const int IconTextureWidth = 128;

    private readonly string dataPath;

    private readonly IGameContentHelper gameContentHelper;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(log, manifest)
    {
        // Init
        this.gameContentHelper = gameContentHelper;
        this.dataPath = this.ModId + "/Data";

        this.Arrows = themeHelper.AddAsset(
            this.ModId + "/Arrows",
            modContentHelper.Load<IRawTextureData>("assets/arrows.png"));

        this.Icons = themeHelper.AddAsset(
            this.ModId + "/Icons",
            modContentHelper.Load<IRawTextureData>("assets/icons.png"));

        // Events
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    /// <summary>Gets the managed arrows texture.</summary>
    public IManagedTexture Arrows { get; }

    /// <summary>Gets the toolbar icons data model.</summary>
    public Dictionary<string, ToolbarIconData> Data =>
        this.gameContentHelper.Load<Dictionary<string, ToolbarIconData>>(this.dataPath);

    /// <summary>Gets the game path to the icons texture.</summary>
    public IManagedTexture Icons { get; }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.dataPath))
        {
            e.LoadFrom(static () => new Dictionary<string, ToolbarIconData>(), AssetLoadPriority.Exclusive);
        }
    }
}