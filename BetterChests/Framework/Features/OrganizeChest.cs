namespace StardewMods.BetterChests.Framework.Features;

using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Menus;

/// <summary>
///     Sort items in a chest using a customized criteria.
/// </summary>
internal sealed class OrganizeChest : IFeature
{
    private const string Id = "furyx639.BetterChests/OrganizeChest";

#nullable disable
    private static IFeature Instance;
#nullable enable

    private readonly IModHelper _helper;

    private bool _isActivated;

    private OrganizeChest(IModHelper helper)
    {
        this._helper = helper;
        HarmonyHelper.AddPatches(
            OrganizeChest.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeItemsInList)),
                    typeof(OrganizeChest),
                    nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix),
                    PatchType.Prefix),
            });
    }

    /// <summary>
    ///     Initializes <see cref="OrganizeChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="OrganizeChest" /> class.</returns>
    public static IFeature Init(IModHelper helper)
    {
        return OrganizeChest.Instance ??= new OrganizeChest(helper);
    }

    /// <inheritdoc />
    public void SetActivated(bool value)
    {
        if (this._isActivated == value)
        {
            return;
        }

        this._isActivated = value;
        if (this._isActivated)
        {
            this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            return;
        }

        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool ItemGrabMenu_organizeItemsInList_prefix(ItemGrabMenu __instance, IList<Item> items)
    {
        if (!ReferenceEquals(__instance.ItemsToGrabMenu.actualInventory, items)
         || BetterItemGrabMenu.Context?.OrganizeChest is not FeatureOption.Enabled)
        {
            return true;
        }

        BetterItemGrabMenu.Context.OrganizeItems();
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        return false;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseRight
         || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu
         || BetterItemGrabMenu.Context?.OrganizeChest is not FeatureOption.Enabled)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.organizeButton?.containsPoint(x, y) != true)
        {
            return;
        }

        BetterItemGrabMenu.Context.OrganizeItems(true);
        this._helper.Input.Suppress(e.Button);
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        Game1.playSound("Ship");
    }
}