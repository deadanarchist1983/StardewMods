namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly IDataHelper dataHelper;
    private readonly IGameContentHelper gameContentHelper;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="dataHelper">Dependency used for storing and retrieving data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(
        IDataHelper dataHelper,
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        IModContentHelper modContentHelper,
        IThemeHelper themeHelper)
        : base(log, manifest)
    {
        // Init
        this.dataHelper = dataHelper;
        this.gameContentHelper = gameContentHelper;
        this.HslTexturePath = this.ModId + "/HueBar";
        this.IconTexturePath = this.ModId + "/Icons";
        this.TabTexturePath = this.ModId + "/Tabs/Texture";
        this.TabDataPath = this.ModId + "/Tabs";

        themeHelper.AddAsset(this.IconTexturePath, modContentHelper.Load<IRawTextureData>("assets/icons.png"));
        themeHelper.AddAsset(this.TabTexturePath, modContentHelper.Load<IRawTextureData>("assets/tabs.png"));

        // Events
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    /// <summary>Gets the game path to the hsl texture.</summary>
    private string HslTexturePath { get; }

    /// <summary>Gets the game path to the icon texture.</summary>
    public string IconTexturePath { get; }

    /// <summary>Gets the game path to the tab texture.</summary>
    private string TabTexturePath { get; }

    /// <summary>Gets the game path to tab data.</summary>
    public string TabDataPath { get; }

    /// <summary>Returns a Texture2D object representing the HSL texture.</summary>
    /// <returns>A Texture2D object.</returns>
    public Texture2D GetHslTexture() => this.gameContentHelper.Load<Texture2D>(this.HslTexturePath);

    /// <summary>Retrieves inventory tab data.</summary>
    /// <returns>A dictionary with inventory tab names as keys and corresponding InventoryTabData objects as values.</returns>
    public Dictionary<string, InventoryTabData> GetInventoryTabData() =>
        this.gameContentHelper.Load<Dictionary<string, InventoryTabData>>(this.TabDataPath);

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.HslTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/hue.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.TabDataPath))
        {
            e.LoadFrom(this.GetTabData, AssetLoadPriority.Exclusive);
        }
    }

    private Dictionary<string, InventoryTabData> GetTabData()
    {
        Dictionary<string, InventoryTabData>? tabData;
        try
        {
            tabData = this.dataHelper.ReadJsonFile<Dictionary<string, InventoryTabData>>("assets/tabs.json");
        }
        catch (Exception)
        {
            tabData = null;
        }

        if (tabData is not null && tabData.Any())
        {
            return tabData;
        }

        tabData = new Dictionary<string, InventoryTabData>
        {
            {
                "Clothing",
                new InventoryTabData(
                    "Clothing",
                    this.TabTexturePath,
                    2,
                    ["category_clothing", "category_boots", "category_hat"])
            },
            {
                "Cooking",
                new InventoryTabData(
                    "Cooking",
                    this.TabTexturePath,
                    3,
                    [
                        "category_syrup",
                        "category_artisan_goods",
                        "category_ingredients",
                        "category_sell_at_pierres_and_marnies",
                        "category_sell_at_pierres",
                        "category_meat",
                        "category_cooking",
                        "category_milk",
                        "category_egg",
                    ])
            },
            {
                "Crops",
                new InventoryTabData(
                    "Crops",
                    this.TabTexturePath,
                    4,
                    ["category_greens", "category_flowers", "category_fruits", "category_vegetable"])
            },
            {
                "Equipment",
                new InventoryTabData(
                    "Equipment",
                    this.TabTexturePath,
                    5,
                    ["category_equipment", "category_ring", "category_tool", "category_weapon"])
            },
            {
                "Fishing",
                new InventoryTabData(
                    "Fishing",
                    this.TabTexturePath,
                    6,
                    ["category_bait", "category_fish", "category_tackle", "category_sell_at_fish_shop"])
            },
            {
                "Materials",
                new InventoryTabData(
                    "Materials",
                    this.TabTexturePath,
                    7,
                    [
                        "category_monster_loot",
                        "category_metal_resources",
                        "category_building_resources",
                        "category_minerals",
                        "category_crafting",
                        "category_gem",
                    ])
            },
            {
                "Misc",
                new InventoryTabData(
                    "Misc",
                    this.TabTexturePath,
                    8,
                    ["category_big_craftable", "category_furniture", "category_junk"])
            },
            {
                "Seeds",
                new InventoryTabData("Seeds", this.TabTexturePath, 9, ["category_seeds", "category_fertilizer"])
            },
        };

        this.dataHelper.WriteJsonFile("assets/tabs.json", tabData);
        return tabData;
    }
}