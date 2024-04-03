namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>Shop using items from placed chests and chests in the farmer's inventory.</summary>
internal sealed class ShopFromChest : BaseFeature<ShopFromChest>
{
    private static ShopFromChest instance = null!;

    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;

    /// <summary>Initializes a new instance of the <see cref="ShopFromChest" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public ShopFromChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        Harmony harmony,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        ShopFromChest.instance = this;
        this.containerFactory = containerFactory;
        this.harmony = harmony;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.ShopFromChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(CarpenterMenu), nameof(CarpenterMenu.ConsumeResources)),
            new HarmonyMethod(typeof(ShopFromChest), nameof(ShopFromChest.CarpenterMenu_ConsumeResources_prefix)));

        this.harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(CarpenterMenu),
                nameof(CarpenterMenu.DoesFarmerHaveEnoughResourcesToBuild)),
            postfix: new HarmonyMethod(
                typeof(ShopFromChest),
                nameof(ShopFromChest.CarpenterMenu_DoesFarmerHaveEnoughResourcesToBuild_postfix)));

        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(CarpenterMenu), nameof(CarpenterMenu.draw)),
            transpiler: new HarmonyMethod(typeof(ShopFromChest), nameof(ShopFromChest.CarpenterMenu_draw_transpiler)));

        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ShopMenu), nameof(ShopMenu.ConsumeTradeItem)),
            new HarmonyMethod(typeof(ShopFromChest), nameof(ShopFromChest.ShopMenu_ConsumeTradeItem_prefix)));

        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ShopMenu), nameof(ShopMenu.HasTradeItem)),
            postfix: new HarmonyMethod(typeof(ShopFromChest), nameof(ShopFromChest.ShopMenu_HasTradeItem_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(CarpenterMenu), nameof(CarpenterMenu.ConsumeResources)),
            AccessTools.DeclaredMethod(
                typeof(ShopFromChest),
                nameof(ShopFromChest.CarpenterMenu_ConsumeResources_prefix)));

        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(
                typeof(CarpenterMenu),
                nameof(CarpenterMenu.DoesFarmerHaveEnoughResourcesToBuild)),
            AccessTools.DeclaredMethod(
                typeof(ShopFromChest),
                nameof(ShopFromChest.CarpenterMenu_DoesFarmerHaveEnoughResourcesToBuild_postfix)));

        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(CarpenterMenu), nameof(CarpenterMenu.draw)),
            AccessTools.DeclaredMethod(typeof(ShopFromChest), nameof(ShopFromChest.CarpenterMenu_draw_transpiler)));

        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(ShopMenu), nameof(ShopMenu.ConsumeTradeItem)),
            AccessTools.DeclaredMethod(typeof(ShopFromChest), nameof(ShopFromChest.ShopMenu_ConsumeTradeItem_prefix)));

        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(ShopMenu), nameof(ShopMenu.HasTradeItem)),
            AccessTools.DeclaredMethod(typeof(ShopFromChest), nameof(ShopFromChest.ShopMenu_HasTradeItem_postfix)));
    }

    private static bool ContainsId(Inventory items, string itemId, int minimum)
    {
        var amount = Game1.player.Items.CountId(itemId);
        var remaining = minimum - amount;
        if (remaining <= 0)
        {
            return true;
        }

        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();
        foreach (var container in containers)
        {
            amount = container.Items.CountId(itemId);
            remaining -= amount;
            if (remaining < 0)
            {
                return true;
            }
        }

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool CarpenterMenu_ConsumeResources_prefix(CarpenterMenu __instance)
    {
        var blueprint = __instance.Blueprint;
        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();

        foreach (var item in __instance.ingredients)
        {
            var amount = Game1.player.Items.ReduceId(item.QualifiedItemId, item.Stack);
            if (amount == item.Stack)
            {
                continue;
            }

            var remaining = item.Stack - amount;
            foreach (var container in containers)
            {
                amount = container.Items.ReduceId(item.QualifiedItemId, remaining);
                remaining -= amount;
                if (remaining <= 0)
                {
                    break;
                }
            }
        }

        Game1.player.Money -= blueprint.BuildCost;
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void CarpenterMenu_DoesFarmerHaveEnoughResourcesToBuild_postfix(
        CarpenterMenu __instance,
        ref bool __result)
    {
        if (__result)
        {
            return;
        }

        var blueprint = __instance.Blueprint;
        if (blueprint.BuildCost < 0 || Game1.player.Money < blueprint.BuildCost)
        {
            return;
        }

        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();
        foreach (var item in __instance.ingredients)
        {
            var amount = Game1.player.Items.CountId(item.QualifiedItemId);
            var remaining = item.Stack - amount;
            if (remaining <= 0)
            {
                continue;
            }

            foreach (var container in containers)
            {
                amount = container.Items.CountId(item.QualifiedItemId);
                remaining -= amount;
                if (remaining < 0)
                {
                    break;
                }
            }

            if (remaining > 0)
            {
                return;
            }
        }

        __result = true;
    }

    private static IEnumerable<CodeInstruction> CarpenterMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(
                AccessTools.DeclaredMethod(
                    typeof(Inventory),
                    nameof(Inventory.ContainsId),
                    [typeof(string), typeof(int)])))
            {
                yield return CodeInstruction.Call(typeof(ShopFromChest), nameof(ShopFromChest.ContainsId));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static bool ShopMenu_ConsumeTradeItem_prefix(string itemId, int count)
    {
        itemId = ItemRegistry.QualifyItemId(itemId);
        if (itemId is "(O)858" or "(O)73" || Game1.player.Items.ContainsId(itemId, count))
        {
            return true;
        }

        var amount = Game1.player.Items.ReduceId(itemId, count);
        if (amount == count)
        {
            return false;
        }

        var remaining = count - amount;
        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();
        foreach (var container in containers)
        {
            amount = container.Items.ReduceId(itemId, remaining);
            remaining -= amount;
            if (remaining <= 0)
            {
                return false;
            }
        }

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ShopMenu_HasTradeItem_postfix(ref bool __result, string itemId, int count)
    {
        if (__result)
        {
            return;
        }

        var amount = Game1.player.Items.CountId(itemId);
        var remaining = count - amount;
        if (remaining <= 0)
        {
            __result = true;
            return;
        }

        var containers = ShopFromChest.instance.containerFactory.GetAll(ShopFromChest.DefaultPredicate).ToList();
        foreach (var container in containers)
        {
            amount = container.Items.CountId(itemId);
            remaining -= amount;
            if (remaining > 0)
            {
                continue;
            }

            __result = true;
            return;
        }
    }

    private static bool DefaultPredicate(IStorageContainer container) =>
        container is not FarmerContainer
        && container.Options.ShopFromChest is not (FeatureOption.Disabled or FeatureOption.Default)
        && container.Items.Count > 0
        && !ShopFromChest.instance.Config.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
        && !(ShopFromChest.instance.Config.CraftFromChestDisableLocations.Contains("UndergroundMine")
            && Game1.player.currentLocation is MineShaft)
        && container.Options.CraftFromChest.WithinRange(
            container.Options.CraftFromChestDistance,
            container.Location,
            container.TileLocation);
}