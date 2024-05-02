namespace StardewMods.BetterChests.Framework.Models.Terms;

using System.Collections.Immutable;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;

/// <summary>Represents a grouped expression.</summary>
internal sealed class GroupedExpression : ISearchExpression
{
    /// <summary>Initializes a new instance of the <see cref="GroupedExpression" /> class.</summary>
    /// <param name="expressions">The grouped expressions.</param>
    public GroupedExpression(IEnumerable<ISearchExpression> expressions) =>
        this.Expressions = expressions.ToImmutableArray();

    /// <summary>Gets the grouped expressions.</summary>
    public ImmutableArray<ISearchExpression> Expressions { get; }

    /// <inheritdoc />
    public bool ExactMatch(Item? item) =>
        item is not null && this.Expressions.Any(expression => expression.ExactMatch(item));

    /// <inheritdoc />
    public bool PartialMatch(Item? item) =>
        item is not null && this.Expressions.Any(expression => expression.PartialMatch(item));

    /// <inheritdoc />
    public bool ExactMatch(IStorageContainer container) =>
        this.Expressions.Any(expression => expression.ExactMatch(container));

    /// <inheritdoc />
    public bool PartialMatch(IStorageContainer container) =>
        this.Expressions.Any(expression => expression.PartialMatch(container));
}