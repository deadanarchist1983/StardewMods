namespace StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Represents a search expression.</summary>
internal interface ISearchExpression
{
    /// <summary>Determines if the expression exactly matches the specified item.</summary>
    /// <param name="item">The item to matched.</param>
    /// <returns>true if the items matches; otherwise, false.</returns>
    bool ExactMatch(Item item);

    /// <summary>Determines if the expression partially matches the specified item.</summary>
    /// <param name="item">The item to be matched.</param>
    /// <returns>true if the item matches; otherwise, false.</returns>
    bool PartialMatch(Item item);

    /// <summary>Determines if the expression exactly matches the specified term.</summary>
    /// <param name="term">The term to be matched.</param>
    /// <returns>true if the term matches; otherwise, false.</returns>
    bool ExactMatch(string term);

    /// <summary>Determines if the expression partially matches the specified term.</summary>
    /// <param name="term">The term to be matched.</param>
    /// <returns>true if the term matches; otherwise, false.</returns>
    bool PartialMatch(string term);
}