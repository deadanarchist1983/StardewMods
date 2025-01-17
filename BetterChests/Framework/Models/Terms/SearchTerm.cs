namespace StardewMods.BetterChests.Framework.Models.Terms;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a search term.</summary>
internal sealed class SearchTerm : ISearchExpression
{
    /// <summary>Initializes a new instance of the <see cref="SearchTerm" /> class.</summary>
    /// <param name="term">The search value.</param>
    public SearchTerm(string term) => this.Term = term;

    /// <summary>Gets the value.</summary>
    public string Term { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item? item) =>
        item is not null
        && ((item.Name is not null && this.ExactMatch(item.Name))
            || (item.DisplayName is not null && this.ExactMatch(item.DisplayName))
            || item.GetContextTags().Any(this.ExactMatch));

    /// <inheritdoc />
    public bool PartialMatch(Item? item) =>
        item is not null
        && ((item.Name is not null && this.PartialMatch(item.Name))
            || (item.DisplayName is not null && this.PartialMatch(item.DisplayName))
            || item.GetContextTags().Any(this.PartialMatch));

    /// <inheritdoc />
    public bool ExactMatch(IStorageContainer container) =>
        this.ExactMatch(container.ToString()!) || container.Items.Any(this.ExactMatch);

    /// <inheritdoc />
    public bool PartialMatch(IStorageContainer container) =>
        this.PartialMatch(container.ToString()!) || container.Items.Any(this.PartialMatch);

    private bool ExactMatch(string term) =>
        !string.IsNullOrWhiteSpace(term) && term.Equals(this.Term, StringComparison.OrdinalIgnoreCase);

    private bool PartialMatch(string term) =>
        !string.IsNullOrWhiteSpace(term) && term.Contains(this.Term, StringComparison.OrdinalIgnoreCase);
}