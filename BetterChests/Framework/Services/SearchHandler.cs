namespace StardewMods.BetterChests.Framework.Services;

using Pidgin;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Terms;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling search.</summary>
internal sealed class SearchHandler : BaseService<SearchHandler>
{
    private static readonly Parser<char, char> BeginGroup;
    private static readonly Parser<char, char> EndGroup;
    private static readonly Parser<char, char> Negation;
    private static readonly Parser<char, Unit> LogicalAnd;
    private static readonly Parser<char, Unit> LogicalOr;

    private static readonly Parser<char, ISearchExpression> GroupedExpressionParser;
    private static readonly Parser<char, ISearchExpression> NegatedParser;
    private static readonly Parser<char, ISearchExpression> AndParser;
    private static readonly Parser<char, ISearchExpression> OrParser;
    private static readonly Parser<char, ISearchExpression> TermParser;
    private static readonly Parser<char, ISearchExpression> SearchParser;

    static SearchHandler()
    {
        SearchHandler.BeginGroup = Parser.Char('(');
        SearchHandler.EndGroup = Parser.Char(')');
        SearchHandler.Negation = Parser.Char('!');
        SearchHandler.LogicalAnd = Parser.CIString("AND").Between(Parser.SkipWhitespaces).IgnoreResult();
        SearchHandler.LogicalOr = Parser.CIString("OR").Between(Parser.SkipWhitespaces).IgnoreResult();

        SearchHandler.SearchParser = null!;
        SearchHandler.AndParser = null!;
        SearchHandler.OrParser = null!;

        SearchHandler.TermParser = Parser
            .AnyCharExcept('(', ')', '!', ' ')
            .ManyString()
            .Between(Parser.SkipWhitespaces)
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Select(term => new SearchTerm(term))
            .OfType<ISearchExpression>();

        SearchHandler.GroupedExpressionParser = Parser
            .Rec(() => SearchHandler.SearchParser)
            .Between(SearchHandler.BeginGroup, SearchHandler.EndGroup);

        SearchHandler.NegatedParser = SearchHandler
            .Negation.Then(SearchHandler.GroupedExpressionParser.Or(SearchHandler.TermParser))
            .Between(Parser.SkipWhitespaces)
            .Select(term => new NegatedExpression(term))
            .OfType<ISearchExpression>();

        SearchHandler.AndParser = Parser
            .Try(
                Parser.Map(
                    (left, right) => new AndExpression(left, right),
                    SearchHandler
                        .NegatedParser.Or(SearchHandler.GroupedExpressionParser)
                        .Or(SearchHandler.TermParser)
                        .Before(SearchHandler.LogicalAnd),
                    SearchHandler.NegatedParser.Or(SearchHandler.GroupedExpressionParser).Or(SearchHandler.TermParser)))
            .OfType<ISearchExpression>();

        SearchHandler.OrParser = Parser
            .Try(
                Parser.Map(
                    (left, right) => new OrExpression(left, right),
                    SearchHandler
                        .NegatedParser.Or(SearchHandler.GroupedExpressionParser)
                        .Or(SearchHandler.TermParser)
                        .Before(SearchHandler.LogicalOr),
                    SearchHandler.NegatedParser.Or(SearchHandler.GroupedExpressionParser).Or(SearchHandler.TermParser)))
            .OfType<ISearchExpression>();

        SearchHandler.SearchParser = Parser
            .OneOf(
                SearchHandler.NegatedParser,
                SearchHandler.GroupedExpressionParser,
                SearchHandler.AndParser,
                SearchHandler.OrParser,
                SearchHandler.TermParser)
            .Many()
            .Select(expressions => new GroupedExpression(expressions))
            .OfType<ISearchExpression>();
    }

    /// <summary>Initializes a new instance of the <see cref="SearchHandler" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public SearchHandler(ILog log, IManifest manifest)
        : base(log, manifest) { }

    /// <summary>Attempts to parse the given search expression.</summary>
    /// <param name="expression">The search expression to be parsed.</param>
    /// <param name="searchExpression">The parsed search expression.</param>
    /// <returns>true if the search expression could be parsed; otherwise, false.</returns>
    public bool TryParseExpression(string expression, [NotNullWhen(true)] out ISearchExpression? searchExpression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            searchExpression = null;
            return false;
        }

        try
        {
            searchExpression = SearchHandler.SearchParser.ParseOrThrow(expression);
            return true;
        }
        catch (ParseException ex)
        {
            this.Log.Trace("Failed to parse search expression {0}.\n{1}", expression, ex);
            searchExpression = null;
            return false;
        }
    }
}