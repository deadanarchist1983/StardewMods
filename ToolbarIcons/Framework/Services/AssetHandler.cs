namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService<AssetHandler>
{
    private readonly string arrowsPath;
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
        this.arrowsPath = this.ModId + "/Arrows";
        this.IconPath = this.ModId + "/Icons";
        this.dataPath = this.ModId + "/Data";

        themeHelper.AddAsset(this.IconPath, modContentHelper.Load<IRawTextureData>("assets/icons.png"));
        themeHelper.AddAsset(this.arrowsPath, modContentHelper.Load<IRawTextureData>("assets/arrows.png"));

        // Events
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    /// <summary>Gets the arrows texture.</summary>
    public Texture2D Arrows => this.gameContentHelper.Load<Texture2D>(this.arrowsPath);

    /// <summary>Gets the toolbar icons data model.</summary>
    public Dictionary<string, ToolbarIconData> Data =>
        this.gameContentHelper.Load<Dictionary<string, ToolbarIconData>>(this.dataPath);

    /// <summary>Gets the game path to the icons texture.</summary>
    public string IconPath { get; }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.dataPath))
        {
            e.LoadFrom(static () => new Dictionary<string, ToolbarIconData>(), AssetLoadPriority.Exclusive);
        }
    }
}