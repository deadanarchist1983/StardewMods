namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly IGameContentHelper gameContentHelper;
    private readonly string hslTexturePath;
    private readonly string iconsPath;
    private readonly IModConfig modConfig;
    private readonly IModContentHelper modContentHelper;
    private readonly Lazy<IManagedTexture> uiTextures;
    private HslColor[]? hslColors;
    private Texture2D? hslTexture;
    private Color[]? hslTextureData;

    private Dictionary<string, Icon>? icons;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(log, manifest)
    {
        // Init
        this.gameContentHelper = gameContentHelper;
        this.modConfig = modConfig;
        this.modContentHelper = modContentHelper;
        this.hslTexturePath = this.ModId + "/HueBar";
        this.iconsPath = this.ModId + "/Icons";

        this.uiTextures = new Lazy<IManagedTexture>(
            () => themeHelper.AddAsset(this.ModId + "/UI", modContentHelper.Load<IRawTextureData>("assets/ui.png")));

        // Events
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventSubscriber.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
    }

    /// <summary>Gets the hsl colors data.</summary>
    public HslColor[] HslColors
    {
        get
        {
            if (this.hslTextureData is not null)
            {
                return this.hslColors ??= this.hslTextureData.Select(HslColor.FromColor).Distinct().ToArray();
            }

            this.hslTextureData = new Color[this.HslTexture.Width * this.HslTexture.Height];
            this.HslTexture.GetData(this.hslTextureData);
            return this.hslColors ??= this.hslTextureData.Select(HslColor.FromColor).Distinct().ToArray();
        }
    }

    /// <summary>Gets the hsl texture.</summary>
    public Texture2D HslTexture => this.hslTexture ??= this.gameContentHelper.Load<Texture2D>(this.hslTexturePath);

    /// <summary>Gets the tab icons.</summary>
    public Dictionary<string, Icon> Icons
    {
        get
        {
            if (this.icons is not null)
            {
                return this.icons;
            }

            this.icons = this.gameContentHelper.Load<Dictionary<string, Icon>>(this.iconsPath);
            foreach (var (id, icon) in this.icons)
            {
                icon.Id = id;
            }

            return this.icons;
        }
    }

    /// <summary>Gets the managed icons texture.</summary>
    public IManagedTexture UiTextures => this.uiTextures.Value;

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.hslTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/hue.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.iconsPath))
        {
            e.LoadFrom(
                () => new Dictionary<string, Icon>
                {
                    {
                        this.ModId + "/Clothing",
                        new Icon
                        {
                            Path = this.modContentHelper.GetInternalAssetName("assets/icons.png").Name,
                            Area = new Rectangle(0, 0, 16, 16),
                        }
                    },
                    {
                        this.ModId + "/Cooking",
                        new Icon
                        {
                            Path = this.modContentHelper.GetInternalAssetName("assets/icons.png").Name,
                            Area = new Rectangle(16, 0, 16, 16),
                        }
                    },
                    {
                        this.ModId + "/Crops",
                        new Icon
                        {
                            Path = this.modContentHelper.GetInternalAssetName("assets/icons.png").Name,
                            Area = new Rectangle(32, 0, 16, 16),
                        }
                    },
                    {
                        this.ModId + "/Equipment",
                        new Icon
                        {
                            Path = this.modContentHelper.GetInternalAssetName("assets/icons.png").Name,
                            Area = new Rectangle(48, 0, 16, 16),
                        }
                    },
                    {
                        this.ModId + "/Fishing",
                        new Icon
                        {
                            Path = this.modContentHelper.GetInternalAssetName("assets/icons.png").Name,
                            Area = new Rectangle(64, 0, 16, 16),
                        }
                    },
                    {
                        this.ModId + "/Materials",
                        new Icon
                        {
                            Path = this.modContentHelper.GetInternalAssetName("assets/icons.png").Name,
                            Area = new Rectangle(80, 0, 16, 16),
                        }
                    },
                    {
                        this.ModId + "/Miscellaneous",
                        new Icon
                        {
                            Path = this.modContentHelper.GetInternalAssetName("assets/icons.png").Name,
                            Area = new Rectangle(96, 0, 16, 16),
                        }
                    },
                    {
                        this.ModId + "/Seeds",
                        new Icon
                        {
                            Path = this.modContentHelper.GetInternalAssetName("assets/icons.png").Name,
                            Area = new Rectangle(112, 0, 16, 16),
                        }
                    },
                },
                AssetLoadPriority.Exclusive);

            return;
        }

        if (e.Name.IsEquivalentTo("Data/BigCraftables")
            && this.modConfig.StorageOptions.TryGetValue("BigCraftables", out var storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var bigCraftableData))
                        {
                            continue;
                        }

                        bigCraftableData.CustomFields ??= new Dictionary<string, string>();
                        var typeOptions = new CustomFieldsStorageOptions(() => bigCraftableData.CustomFields);
                        storageOptions.CopyTo(typeOptions);
                    }
                });

            return;
        }

        if (e.Name.IsEquivalentTo("Data/Buildings")
            && this.modConfig.StorageOptions.TryGetValue("Buildings", out storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, BuildingData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var buildingData))
                        {
                            continue;
                        }

                        buildingData.CustomFields ??= new Dictionary<string, string>();
                        var typeOptions = new CustomFieldsStorageOptions(() => buildingData.CustomFields);
                        storageOptions.CopyTo(typeOptions);
                    }
                });

            return;
        }

        if (e.Name.IsEquivalentTo("Data/Locations")
            && this.modConfig.StorageOptions.TryGetValue("Locations", out storageTypes))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, LocationData>().Data;
                    foreach (var (storageId, storageOptions) in storageTypes)
                    {
                        if (!data.TryGetValue(storageId, out var locationData))
                        {
                            continue;
                        }

                        locationData.CustomFields ??= new Dictionary<string, string>();
                        var typeOptions = new CustomFieldsStorageOptions(() => locationData.CustomFields);
                        storageOptions.CopyTo(typeOptions);
                    }
                });
        }
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> e)
    {
        foreach (var dataType in e.Config.StorageOptions.Keys)
        {
            this.gameContentHelper.InvalidateCache($"Data/{dataType}");
        }
    }
}