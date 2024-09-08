using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Validation;

static class Extension
{
    internal static string GetMessage(this IssueId issueId, object[] additionalInformation)
    {
        switch(issueId)
        {
            case IssueId.AmbiguousSymbol:
            {
                var variants = additionalInformation
                    .Skip(1)
                    .Select(item => "\n" + SearchResultFormatter(item))
                    .Stringify("")
                    .Indent();
                return $"Multiple definitions found at type {TypeFormatter(additionalInformation[0])}.{variants}";
            }

            case IssueId.MissingDeclarationForType:
                return $"No definition found at type {TypeFormatter(additionalInformation[0])}.";

            case IssueId.MissingDeclarationInContext:
                return $"No definition found in context {ContextFormatter(additionalInformation[0])}.";

            case IssueId.InvalidSuffixExpression:
                return $"Using {additionalInformation[0]} as suffix is invalid.";

            case IssueId.InvalidExpression:
                return "Invalid expression.";

            case IssueId.InvalidDeclarationTag:
                return "Invalid declaration tag.";
            case IssueId.EOFInComment:
                return "End of file in comment.";
            case IssueId.EOLInString:
                return "End of line in string.";
            case IssueId.InvalidCharacter:
                return "Invalid character.";
            case IssueId.InvalidDeclaration:
                return "Invalid declaration.";
            case IssueId.MissingDeclarationDeclarer:
                return "Missing declarer part in declaration.";
            case IssueId.MissingDeclarationValue:
                return "Missing value part in declaration.";

            case IssueId.ExtraLeftBracket:
                return $"No closing bracket found until {PositionFormatter(additionalInformation[0])}.";

            case IssueId.ExtraRightBracket:
                return $"No opening bracket found until {PositionFormatter(additionalInformation[0], true)}.";

            case IssueId.MissingMatchingRightBracket:
                return $"No closing bracket found until {PositionFormatter(additionalInformation[0])}.";

            case IssueId.UnexpectedException:
                return $"Exception {additionalInformation[0]}: {additionalInformation[1]}.";

            case IssueId.ExpectationFailedException:

                return $"Expected {((string)additionalInformation[0]).Quote()}.";
        }

        Dumpable.NotImplementedFunction(issueId, additionalInformation.Dump());
        return default;
    }


    internal static Issue GetIssue(this IssueId issueId, IToken position, params object[] additionalInformation)
        => issueId.GetIssue(position.Characters, additionalInformation);

    internal static Issue GetIssue
    (
        this IssueId issueId
        , SourcePart position, params object[] additionalInformation
    )
    {
        Validate(issueId, additionalInformation);
        return new(issueId, position, additionalInformation);
    }

    static void Validate(IssueId issueId, object[] currentInformation)
    {
        var information = issueId.GetAttribute<Setup>()?.AdditionalInformation ?? [];
        (information.Length == currentInformation.Length).Assert();
        currentInformation
            .Select((value, index) => value.GetType().Is(information[index]))
            .All(value => value)
            .Assert();
        GetMessage(issueId, currentInformation).AssertIsNotNull();
    }

    internal static Issue GetIssue(this IssueId issueId, BinaryTree[] anchors)
    {
        var sourceParts = anchors.Select(anchor => anchor.SourcePart).ToArray();
        return issueId.GetIssue(sourceParts.First(), sourceParts.Skip(1).ToArray());
    }

    internal static Result GetResult
    (
        this IssueId issueId
        , Category category
        , SourcePart token
        , object target
        , object[] results = null
        , Issue[] foundIssues = null
    )
    {
        var additionalInformation = results == null? T(target) : T(target, results);
        return new(category, T(foundIssues, T(issueId.GetIssue(token, additionalInformation))).ConcatMany().ToArray());
    }

    internal static Result<BinaryTree> GetSyntax(this IssueId issueId, BinaryTree binaryTree)
        => new(binaryTree, issueId.GetIssue(binaryTree.SourcePart));

    static string SearchResultFormatter(object rawTarget)
    {
        var target = (SearchResult)rawTarget;
        return target.NodeDump;
    }

    static string TypeFormatter(object rawTarget)
    {
        var target = (TypeBase)rawTarget;
        return target.NodeDump;
    }

    static string ContextFormatter(object rawTarget)
    {
        var target = (ContextBase)rawTarget;
        return target.NodeDump;
    }

    static string PositionFormatter(object rawTarget, bool isStart = false)
    {
        var target = (SourcePart)rawTarget;
        var sourcePosition = isStart? target.Start : target.End;
        var position = sourcePosition.TextPosition;
        return $"Line {position.LineNumber} Column {position.ColumnNumber1 - 1}";
    }

    static TValue[] T<TValue>(params TValue[] value) => value;
}