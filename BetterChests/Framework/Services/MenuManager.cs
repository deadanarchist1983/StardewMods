namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Manages the item grab menu in the game.</summary>
internal sealed class MenuManager : BaseService<MenuManager>
{
    private static MenuManager instance = null!;

    private readonly PerScreen<InventoryMenuManager> bottomMenu;
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<IClickableMenu?> currentMenu = new();
    private readonly IEventManager eventManager;
    private readonly PerScreen<ServiceLock?> focus = new();
    private readonly PerScreen<InventoryMenuManager> topMenu;

    /// <summary>Initializes a new instance of the <see cref="MenuManager" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public MenuManager(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IInputHelper inputHelper,
        IPatchManager patchManager)
        : base(log, manifest)
    {
        // Init
        MenuManager.instance = this;
        this.containerFactory = containerFactory;
        this.eventManager = eventManager;

        this.topMenu = new PerScreen<InventoryMenuManager>(
            () => new InventoryMenuManager(eventManager, inputHelper, log, manifest, modConfig));

        this.bottomMenu = new PerScreen<InventoryMenuManager>(
            () => new InventoryMenuManager(eventManager, inputHelper, log, manifest, modConfig));

        // Events
        eventManager.Subscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        eventManager.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        eventManager.Subscribe<UpdateTickingEventArgs>(this.OnUpdateTicking);
        eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        eventManager.Subscribe<WindowResizedEventArgs>(this.OnWindowResized);

        // Patches
        patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(IClickableMenu), nameof(IClickableMenu.SetChildMenu)),
                AccessTools.DeclaredMethod(
                    typeof(MenuManager),
                    nameof(MenuManager.IClickableMenu_SetChildMenu_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.draw), [typeof(SpriteBatch)]),
                AccessTools.DeclaredMethod(typeof(MenuManager), nameof(MenuManager.InventoryMenu_draw_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.draw), [typeof(SpriteBatch)]),
                AccessTools.DeclaredMethod(typeof(MenuManager), nameof(MenuManager.InventoryMenu_draw_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools
                    .GetDeclaredConstructors(typeof(ItemGrabMenu))
                    .Single(ctor => ctor.GetParameters().Length > 5),
                AccessTools.DeclaredMethod(
                    typeof(MenuManager),
                    nameof(MenuManager.ItemGrabMenu_constructor_transpiler)),
                PatchType.Transpiler));

        patchManager.Patch(this.UniqueId);
    }

    /// <summary>Gets the current item grab menu.</summary>
    public ItemGrabMenu? CurrentMenu =>
        Game1.activeClickableMenu?.Equals(this.currentMenu.Value) == true
            ? this.currentMenu.Value as ItemGrabMenu
            : null;

    /// <summary>Gets the inventory menu manager for the top inventory menu.</summary>
    public IInventoryMenuManager Top => this.topMenu.Value;

    /// <summary>Gets the inventory menu manager for the bottom inventory menu.</summary>
    public IInventoryMenuManager Bottom => this.bottomMenu.Value;

    /// <summary>Determines if the specified source object can receive focus.</summary>
    /// <param name="source">The object to check if it can receive focus.</param>
    /// <returns>true if the source object can receive focus; otherwise, false.</returns>
    public bool CanFocus(object source) => this.focus.Value is null || this.focus.Value.Source == source;

    /// <summary>Tries to request focus for a specific object.</summary>
    /// <param name="source">The object that needs focus.</param>
    /// <param name="serviceLock">
    /// An optional output parameter representing the acquired service lock, or null if failed to
    /// acquire.
    /// </param>
    /// <returns>true if focus was successfully acquired; otherwise, false.</returns>
    public bool TryGetFocus(object source, [NotNullWhen(true)] out IServiceLock? serviceLock)
    {
        serviceLock = null;
        if (this.focus.Value is not null && this.focus.Value.Source != source)
        {
            return false;
        }

        if (this.focus.Value is not null && this.focus.Value.Source == source)
        {
            serviceLock = this.focus.Value;
            return true;
        }

        this.focus.Value = new ServiceLock(source, this);
        serviceLock = this.focus.Value;
        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void IClickableMenu_SetChildMenu_postfix() => MenuManager.instance.UpdateMenu();

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_prefix(InventoryMenu __instance, ref InventoryMenuManager? __state)
    {
        __state = __instance.Equals(MenuManager.instance.topMenu.Value.Menu)
            ? MenuManager.instance.topMenu.Value
            : __instance.Equals(MenuManager.instance.bottomMenu.Value.Menu)
                ? MenuManager.instance.bottomMenu.Value
                : null;

        if (__state?.Container is null)
        {
            return;
        }

        // Apply operations
        var itemsDisplayingEventArgs = new ItemsDisplayingEventArgs(__state.Container);
        MenuManager.instance.eventManager.Publish(itemsDisplayingEventArgs);
        __instance.actualInventory = itemsDisplayingEventArgs.Items.ToList();

        var defaultName = int.MaxValue.ToString(CultureInfo.InvariantCulture);
        for (var index = 0; index < __instance.inventory.Count; ++index)
        {
            if (index >= __instance.actualInventory.Count)
            {
                __instance.inventory[index].name = defaultName;
                continue;
            }

            var actualIndex = __state.Container.Items.IndexOf(__instance.actualInventory[index]);
            __instance.inventory[index].name =
                actualIndex > -1 ? actualIndex.ToString(CultureInfo.InvariantCulture) : defaultName;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_postfix(InventoryMenu __instance, ref InventoryMenuManager? __state)
    {
        __state = __instance.Equals(MenuManager.instance.topMenu.Value.Menu)
            ? MenuManager.instance.topMenu.Value
            : __instance.Equals(MenuManager.instance.bottomMenu.Value.Menu)
                ? MenuManager.instance.bottomMenu.Value
                : null;

        if (__state?.Container is null)
        {
            return;
        }

        // Restore original
        __instance.actualInventory = __state.Container.Items;
    }

    private static IEnumerable<CodeInstruction>
        ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchStartForward(new CodeMatch(OpCodes.Stloc_1))
            .Advance(-1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuManager), nameof(MenuManager.GetChestContext))))
            .MatchStartForward(
                new CodeMatch(
                    instruction => instruction.Calls(
                        AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)))))
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuManager), nameof(MenuManager.GetMenuCapacity))))
            .MatchStartForward(
                new CodeMatch(
                    instruction => instruction.Calls(
                        AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)))))
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuManager), nameof(MenuManager.GetMenuCapacity))))
            .InstructionEnumeration();

    private static object? GetChestContext(Item? sourceItem, object? context) =>
        context switch
        {
            Chest chest => chest,
            SObject
            {
                heldObject.Value: Chest heldChest,
            } => heldChest,
            Building building when building.buildingChests.Any() => building.buildingChests.First(),
            GameLocation location when location.GetFridge() is Chest fridge => fridge,
            _ => sourceItem,
        };

    private static int GetMenuCapacity(int capacity, object? context)
    {
        switch (context)
        {
            case Item item when MenuManager.instance.containerFactory.TryGetOne(item, out var container):
            case Building building when MenuManager.instance.containerFactory.TryGetOne(building, out container):
                return container.Options.ResizeChest switch
                {
                    ChestMenuOption.Small => 9,
                    ChestMenuOption.Medium => 36,
                    ChestMenuOption.Large => 70,
                    _ when capacity is > 70 or -1 => 70,
                    _ => capacity,
                };

            default: return capacity > 70 ? 70 : capacity;
        }
    }

    private void OnUpdateTicking(UpdateTickingEventArgs e) => this.UpdateMenu();

    private void OnUpdateTicked(UpdateTickedEventArgs e) => this.UpdateMenu();

    private void UpdateHighlightMethods()
    {
        switch (this.CurrentMenu)
        {
            case ItemGrabMenu itemGrabMenu:
                if (itemGrabMenu.ItemsToGrabMenu.highlightMethod != this.topMenu.Value.HighlightMethod)
                {
                    this.topMenu.Value.OriginalHighlightMethod = this.CurrentMenu.ItemsToGrabMenu.highlightMethod;
                    itemGrabMenu.ItemsToGrabMenu.highlightMethod = this.topMenu.Value.HighlightMethod;
                }

                if (itemGrabMenu.inventory.highlightMethod != this.bottomMenu.Value.HighlightMethod)
                {
                    this.bottomMenu.Value.OriginalHighlightMethod = this.CurrentMenu.inventory.highlightMethod;
                    itemGrabMenu.inventory.highlightMethod = this.bottomMenu.Value.HighlightMethod;
                }

                break;
        }
    }

    private void OnWindowResized(WindowResizedEventArgs e) => this.Top.Container?.ShowMenu();

    private void UpdateMenu()
    {
        var menu = Game1.activeClickableMenu?.GetChildMenu() ?? Game1.activeClickableMenu;
        if (menu == this.currentMenu.Value)
        {
            this.UpdateHighlightMethods();
            return;
        }

        this.currentMenu.Value = menu;
        this.focus.Value = null;
        if (menu is not ItemGrabMenu itemGrabMenu)
        {
            this.topMenu.Value.Reset(null, null);
            this.bottomMenu.Value.Reset(null, null);
            this.eventManager.Publish(new ItemGrabMenuChangedEventArgs());
            return;
        }

        // Update top menu
        this.topMenu.Value.Reset(itemGrabMenu, itemGrabMenu.ItemsToGrabMenu);
        if (this.containerFactory.TryGetOne(out var topContainer))
        {
            // Relaunch shipping bin menu
            if (itemGrabMenu.shippingBin
                && topContainer is BuildingContainer
                {
                    Options.ResizeChest: not (ChestMenuOption.Default or ChestMenuOption.Disabled),
                })
            {
                topContainer.ShowMenu();
                return;
            }

            itemGrabMenu.behaviorFunction = topContainer.GrabItemFromInventory;
            itemGrabMenu.behaviorOnItemGrab = topContainer.GrabItemFromChest;
        }

        this.topMenu.Value.Container = topContainer;

        // Update bottom menu
        this.bottomMenu.Value.Reset(itemGrabMenu, itemGrabMenu.inventory);
        if (!itemGrabMenu.inventory.actualInventory.Equals(Game1.player.Items)
            || !this.containerFactory.TryGetOne(Game1.player, out var bottomContainer))
        {
            bottomContainer = null;
        }

        this.bottomMenu.Value.Container = bottomContainer;

        // Reset filters
        this.UpdateHighlightMethods();
        this.eventManager.Publish(new ItemGrabMenuChangedEventArgs());

        // Disable background fade
        itemGrabMenu.setBackgroundTransparency(false);
    }

    [Priority(int.MaxValue)]
    private void OnRenderingActiveMenu(RenderingActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null || Game1.options.showClearBackgrounds)
        {
            return;
        }

        // Redraw background
        e.SpriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);

        Game1.mouseCursorTransparency = 0f;
    }

    [Priority(int.MinValue)]
    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        // Draw overlay
        this.topMenu.Value.Draw(e.SpriteBatch);
        this.bottomMenu.Value.Draw(e.SpriteBatch);

        // Redraw foreground
        if (this.focus.Value is null)
        {
            if (this.CurrentMenu.hoverText != null
                && (this.CurrentMenu.hoveredItem == null || this.CurrentMenu.ItemsToGrabMenu == null))
            {
                if (this.CurrentMenu.hoverAmount > 0)
                {
                    IClickableMenu.drawToolTip(
                        e.SpriteBatch,
                        this.CurrentMenu.hoverText,
                        string.Empty,
                        null,
                        true,
                        -1,
                        0,
                        null,
                        -1,
                        null,
                        this.CurrentMenu.hoverAmount);
                }
                else
                {
                    IClickableMenu.drawHoverText(e.SpriteBatch, this.CurrentMenu.hoverText, Game1.smallFont);
                }
            }

            if (this.CurrentMenu.hoveredItem != null)
            {
                IClickableMenu.drawToolTip(
                    e.SpriteBatch,
                    this.CurrentMenu.hoveredItem.getDescription(),
                    this.CurrentMenu.hoveredItem.DisplayName,
                    this.CurrentMenu.hoveredItem,
                    this.CurrentMenu.heldItem != null);
            }
            else if (this.CurrentMenu.hoveredItem != null && this.CurrentMenu.ItemsToGrabMenu != null)
            {
                IClickableMenu.drawToolTip(
                    e.SpriteBatch,
                    this.CurrentMenu.ItemsToGrabMenu.descriptionText,
                    this.CurrentMenu.ItemsToGrabMenu.descriptionTitle,
                    this.CurrentMenu.hoveredItem,
                    this.CurrentMenu.heldItem != null);
            }

            this.CurrentMenu.heldItem?.drawInMenu(
                e.SpriteBatch,
                new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8),
                1f);
        }

        Game1.mouseCursorTransparency = 1f;
        this.CurrentMenu.drawMouse(e.SpriteBatch);
    }

    private sealed class ServiceLock(object source, MenuManager menuManager) : IServiceLock
    {
        public object Source => source;

        public void Release()
        {
            if (menuManager.focus.Value == this)
            {
                menuManager.focus.Value = null;
            }
        }
    }
}