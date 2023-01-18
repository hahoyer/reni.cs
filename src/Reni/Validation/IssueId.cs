using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Validation;

enum IssueStage
{
    Unexpected
    , Parsing
    , Syntax
    , Semantic
}

sealed class IssueId : EnumEx, Match.IError
{
    public static readonly IssueId AmbiguousSymbol = new(IssueStage.Semantic, typeof(TypeBase), typeof(SearchResult[]));
    public static readonly IssueId EOFInComment = new(IssueStage.Parsing);
    public static readonly IssueId EOLInString = new(IssueStage.Parsing);
    public static readonly IssueId ExtraLeftBracket = new(IssueStage.Syntax, typeof(SourcePart));
    public static readonly IssueId ExtraRightBracket = new(IssueStage.Syntax, typeof(SourcePart));
    public static readonly IssueId InvalidCharacter = new(IssueStage.Parsing);
    public static readonly IssueId InvalidDeclaration = new(IssueStage.Syntax);
    public static readonly IssueId InvalidDeclarationTag = new(IssueStage.Syntax);
    public static readonly IssueId InvalidExpression = new(IssueStage.Syntax);
    public static readonly IssueId InvalidInfixExpression = new(IssueStage.Syntax);
    public static readonly IssueId InvalidPrefixExpression = new(IssueStage.Syntax);
    public static readonly IssueId InvalidSuffixExpression = new(IssueStage.Syntax, typeof(string));
    public static readonly IssueId InvalidTerminalExpression = new(IssueStage.Syntax);
    public static readonly IssueId MissingDeclarationDeclarer = new(IssueStage.Syntax);
    public static readonly IssueId MissingDeclarationForType = new(IssueStage.Semantic, typeof(TypeBase));
    public static readonly IssueId MissingDeclarationInContext = new(IssueStage.Semantic, typeof(ContextBase));
    public static readonly IssueId MissingDeclarationValue = new(IssueStage.Syntax);
    public static readonly IssueId MissingMatchingRightBracket = new(IssueStage.Syntax);
    public static readonly IssueId MissingRightExpression = new(IssueStage.Semantic);
    public static readonly IssueId StrangeDeclaration = new(IssueStage.Unexpected);
    readonly IssueStage Stage;
    readonly System.Type[] AdditionalInformation;

    public static IEnumerable<IssueId> All => AllInstances<IssueId>();

    IssueId(IssueStage stage, params System.Type[] additionalInformation)
    {
        Stage = stage;
        AdditionalInformation = additionalInformation;
    }

    internal Issue GetIssue(IToken position, params object[] additionalInformation)
        => GetIssue(position.Characters, additionalInformation);

    internal Issue GetIssue(SourcePart position, params object[] additionalInformation)
    {
        Validate(additionalInformation);
        return new(this, position, additionalInformation);
    }

    void Validate(object[] additionalInformation)
    {
        (AdditionalInformation.Length == additionalInformation.Length).Assert();
        additionalInformation
            .Select((value, index) => value.GetType().Is(AdditionalInformation[index]))
            .All(value => value)
            .Assert();
    }

    internal Issue GetIssue(BinaryTree[] anchors)
    {
        var sourceParts = anchors.Select(anchor => anchor.SourcePart).ToArray();
        return GetIssue(sourceParts.First(), sourceParts.Skip(1).ToArray());
    }

    internal Result GetResult
    (
        Category category
        , SourcePart token
        , object target
        , object[] results = null
        , Issue[] foundIssues = null
    )
    {
        var additionalInformation = results == null? T(target) : T(target, results);
        return new(category, T(foundIssues, T(GetIssue(token, additionalInformation))).ConcatMany().ToArray());
    }

    internal Result<BinaryTree> GetSyntax(BinaryTree binaryTree) => new(binaryTree, GetIssue(binaryTree.SourcePart));

    internal string GetMessage(object[] additionalInformation)
    {
        if(this == InvalidSuffixExpression)
            return $"Using {additionalInformation[0]} as suffix is invalid.";

        NotImplementedMethod(additionalInformation);
        return default;
    }
}