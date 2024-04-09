namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class Themer(FauxCoreIntegration fauxCore) : IThemeHelper
{
    private readonly IThemeHelper themeHelper = fauxCore.Api!.CreateThemeService();

    /// <inheritdoc />
    public void AddAsset(string path, IRawTextureData data) => this.themeHelper.AddAsset(path, data);
}