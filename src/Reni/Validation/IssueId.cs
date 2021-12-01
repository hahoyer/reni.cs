using System.Collections.Generic;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Validation
{
    sealed class IssueId : EnumEx, Match.IError
    {
        public static readonly IssueId AmbiguousSymbol = new IssueId();
        public static readonly IssueId ConsequentialError = new IssueId();
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId InvalidCharacter = new IssueId();
        public static readonly IssueId InvalidDeclarationTag = new IssueId();
        public static readonly IssueId InvalidExpression = new IssueId();
        public static readonly IssueId InvalidInfixExpression = new IssueId();
        public static readonly IssueId InvalidNameForDeclaration = new IssueId();
        public static readonly IssueId InvalidLeftOperand = new IssueId();
        public static readonly IssueId InvalidListOperandSequence = new IssueId();
        public static readonly IssueId InvalidPrefixExpression = new IssueId();
        public static readonly IssueId InvalidRightOperand = new IssueId();
        public static readonly IssueId InvalidSuffixExpression = new IssueId();
        public static readonly IssueId InvalidTerminalExpression = new IssueId();
        public static readonly IssueId MissingDeclarationForType = new IssueId();
        public static readonly IssueId MissingDeclarationInContext = new IssueId();
        public static readonly IssueId MissingDeclarationTag = new IssueId();
        public static readonly IssueId MissingDeclarationValue = new IssueId();
        public static readonly IssueId MissingLeftBracket = new IssueId();
        public static readonly IssueId MissingLeftOperand = new IssueId();
        public static readonly IssueId MissingMatchingRightBracket = new IssueId();
        public static readonly IssueId MissingRightBracket = new IssueId();
        public static readonly IssueId MissingRightExpression = new IssueId();
        public static readonly IssueId MissingRightOperand = new IssueId();
        public static readonly IssueId StrangeDeclaration = new IssueId();
        public static readonly IssueId UnknownDeclarationTag = new IssueId();


        public static IEnumerable<IssueId> All => AllInstances<IssueId>();

        internal Issue Issue(IToken position, string message = null) => new Issue(this, position.Characters, message);
        internal Issue Issue(SourcePart position, string message = null) => new Issue(this, position, message);

        internal Result IssueResult(Category category, IToken token, string message = null)
            => new Result(category, Issue(token, message));

        internal Result<BinaryTree> Syntax(BinaryTree binaryTree)
            => new Result<BinaryTree>(binaryTree, Issue(binaryTree.SourcePart));
    }
}