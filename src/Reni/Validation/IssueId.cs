using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Validation;

sealed class IssueId : EnumEx, Match.IError
{
    public static readonly IssueId AmbiguousSymbol = new();
    public static readonly IssueId EOFInComment = new();
    public static readonly IssueId EOLInString = new();
    public static readonly IssueId InvalidCharacter = new();
    public static readonly IssueId InvalidDeclaration = new();
    public static readonly IssueId InvalidDeclarationTag = new();
    public static readonly IssueId InvalidExpression = new();
    public static readonly IssueId InvalidInfixExpression = new();
    public static readonly IssueId InvalidPrefixExpression = new();
    public static readonly IssueId InvalidSuffixExpression = new();
    public static readonly IssueId InvalidTerminalExpression = new();
    public static readonly IssueId MissingDeclarationForType = new();
    public static readonly IssueId MissingDeclarationInContext = new();
    public static readonly IssueId MissingDeclarationValue = new();
    public static readonly IssueId MissingLeftBracket = new();
    public static readonly IssueId MissingMatchingRightBracket = new();
    public static readonly IssueId MissingRightBracket = new();
    public static readonly IssueId MissingRightExpression = new();
    public static readonly IssueId StrangeDeclaration = new();

    public static IEnumerable<IssueId> All => AllInstances<IssueId>();

    internal Issue Issue(IToken position, string message = null) => new(this, position.Characters, message);
    internal Issue Issue(SourcePart position, string message = null) => new(this, position, message);

    internal Issue Issue(BinaryTree[] anchors, string message = null)
    {
        anchors.Select(anchor => anchor.SourcePart).Aggregate();
        return new(this, anchors.Select(anchor => anchor.SourcePart).Aggregate(), message);
    }

    internal Result IssueResult(Category category, IToken token, string message = null)
        => new(category, Issue(token, message));

    internal Result<BinaryTree> Syntax(BinaryTree binaryTree) => new(binaryTree, Issue(binaryTree.SourcePart));
}