using hw.Scanner;
using JetBrains.Annotations;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Validation;

enum Stage
{
    Unexpected
    , Parsing
    , Syntax
    , Semantic
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class Setup : Attribute
{
    internal readonly System.Type[] AdditionalInformation;
    readonly Stage Stage;

    internal Setup(Stage stage, params System.Type[] additionalInformation)
    {
        Stage = stage;
        AdditionalInformation = additionalInformation;
    }
}

enum IssueId
{
    [UsedImplicitly]
    None

    , [Setup(Stage.Semantic, typeof(TypeBase), typeof(SearchResult[]))]
    AmbiguousSymbol

    , [Setup(Stage.Parsing)]
    EOFInComment

    , [Setup(Stage.Parsing)]
    EOLInString

    , [Setup(Stage.Syntax, typeof(SourcePart))]
    ExtraLeftBracket

    , [Setup(Stage.Syntax, typeof(SourcePart))]
    ExtraRightBracket

    , [Setup(Stage.Parsing)]
    InvalidCharacter

    , [Setup(Stage.Syntax)]
    InvalidDeclaration

    , [Setup(Stage.Syntax)]
    InvalidDeclarationTag

    , [Setup(Stage.Syntax)]
    InvalidExpression

    , [Setup(Stage.Syntax)]
    InvalidInfixExpression

    , [Setup(Stage.Syntax)]
    InvalidPrefixExpression

    , [Setup(Stage.Syntax, typeof(string))]
    InvalidSuffixExpression

    , [Setup(Stage.Syntax)]
    InvalidTerminalExpression

    , [Setup(Stage.Syntax)]
    MissingDeclarationDeclarer

    , [Setup(Stage.Semantic, typeof(TypeBase))]
    MissingDeclarationForType

    , [Setup(Stage.Semantic, typeof(ContextBase))]
    MissingDeclarationInContext

    , [Setup(Stage.Syntax)]
    MissingDeclarationValue

    , [Setup(Stage.Syntax)]
    MissingMatchingRightBracket

    , [Setup(Stage.Semantic)]
    MissingRightExpression

    , [Setup(Stage.Unexpected)]
    [PublicAPI]
    StrangeDeclaration
}