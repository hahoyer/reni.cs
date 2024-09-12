using hw.Scanner;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Validation;

enum Stage
{
    Unexpected
    , Scanner
    , Parser
    , Syntax
    , Semantic
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class Setup : Attribute
{
    internal readonly System.Type[] AdditionalInformation;

    internal Setup(params System.Type[] additionalInformation) => AdditionalInformation = additionalInformation;
}

enum IssueId
{
    [UsedImplicitly]
    None

    , [Setup(typeof(TypeBase), typeof(SearchResult[]))]
    AmbiguousSymbol

    , ConsequenceError
    , EOFInComment
    , EOFInVerbatimText
    , EOLInText
    , [Setup(typeof(string))]
    ExpectationFailedException

    , [Setup(typeof(SourcePart))]
    ExtraLeftBracket

    , [Setup(typeof(SourcePart))]
    ExtraRightBracket

    , InvalidCharacter
    , InvalidDeclaration
    , InvalidDeclarationTag
    , InvalidExpression
    , InvalidInfixExpression
    , InvalidPrefixExpression
    , [Setup(typeof(string))]
    InvalidSuffixExpression

    , InvalidTerminalExpression
    , MissingDeclarationDeclarer
    , [Setup(typeof(TypeBase))]
    MissingDeclarationForType

    , [Setup(typeof(ContextBase))]
    MissingDeclarationInContext

    , MissingDeclarationValue
    , [Setup(typeof(SourcePart))]
    MissingMatchingRightBracket

    , MissingRightExpression
    , [Setup(typeof(string), typeof(string))]
    UnexpectedException
}