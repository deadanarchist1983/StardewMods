namespace StardewMods.ToolbarIcons.Framework;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewMods.ToolbarIcons.Framework.Services;

/// <inheritdoc />
public sealed class ToolbarIconsApi : IToolbarIconsApi
{
    private readonly BaseEventManager eventManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly ILog log;
    private readonly IModInfo modInfo;
    private readonly string prefix;
    private readonly ToolbarManager toolbarManager;

    private EventHandler<string>? toolbarIconPressed;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsApi" /> class.</summary>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    /// <param name="toolbarManager">Dependency for managing the toolbar icons.</param>
    internal ToolbarIconsApi(
        IModInfo modInfo,
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        ToolbarManager toolbarManager)
    {
        // Init
        this.modInfo = modInfo;
        this.gameContentHelper = gameContentHelper;
        this.log = log;
        this.toolbarManager = toolbarManager;
        this.prefix = this.modInfo.Manifest.UniqueID + "/";
        this.eventManager = new BaseEventManager(log, modInfo.Manifest);

        // Events
        eventSubscriber.Subscribe<IIconPressedEventArgs>(this.OnIconPressed);
    }

    /// <inheritdoc />
    public event EventHandler<string> ToolbarIconPressed
    {
        add
        {
            this.log.WarnOnce(
                "{0} uses deprecated code. {1} event is deprecated. Please subscribe to the {2} event instead.",
                this.modInfo.Manifest.Name,
                nameof(this.ToolbarIconPressed),
                nameof(IIconPressedEventArgs));

            this.toolbarIconPressed += value;
        }
        remove => this.toolbarIconPressed -= value;
    }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText) =>
        this.toolbarManager.AddToolbarIcon(
            $"{this.prefix}{id}",
            () => this.gameContentHelper.Load<Texture2D>(texturePath),
            sourceRect,
            hoverText);

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id) => this.toolbarManager.RemoveToolbarIcon($"{this.prefix}{id}");

    /// <inheritdoc />
    public void Subscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc />
    public void Unsubscribe(Action<IIconPressedEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (!e.Id.StartsWith(this.prefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var id = e.Id[this.prefix.Length..];
        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(id, e.Button));

        if (this.toolbarIconPressed is null)
        {
            return;
        }

        foreach (var handler in this.toolbarIconPressed.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(this, id);
            }
            catch (Exception ex)
            {
                this.log.Error(
                    "{0} failed in {1}: {2}",
                    this.modInfo.Manifest.Name,
                    nameof(this.ToolbarIconPressed),
                    ex.Message);
            }
        }
    }
}