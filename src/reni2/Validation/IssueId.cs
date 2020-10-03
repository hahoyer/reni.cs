using System.Collections.Generic;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Validation
{
    sealed class IssueId : EnumEx, Match.IError
    {
        public static readonly IssueId AmbigousSymbol = new IssueId();
        public static readonly IssueId EOFInComment = new IssueId();
        public static readonly IssueId EOFInLineComment = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId ExtraLeftBracket = new IssueId();
        public static readonly IssueId ExtraRightBracket = new IssueId();
        public static readonly IssueId InvalidCharacter = new IssueId();
        public static readonly IssueId InvalidDeclarationTag = new IssueId();
        public static readonly IssueId InvalidExpression = new IssueId();
        public static readonly IssueId InvalidListOperandSequence = new IssueId();
        public static readonly IssueId MissingDeclarationForType = new IssueId();
        public static readonly IssueId MissingDeclarationInContext = new IssueId();
        public static readonly IssueId MissingDeclarationTag = new IssueId();
        public static readonly IssueId MissingRightExpression = new IssueId();
        public static readonly IssueId TerminalUsedAsSuffix = new IssueId();
        public static readonly IssueId UnknownDeclarationTag = new IssueId();

        public static IEnumerable<IssueId> All => AllInstances<IssueId>();

        internal Issue Issue(SourcePart position, string message = null) => new Issue(this, position, message);

        internal Result IssueResult(Category category, SourcePart position, string message = null)
            => new Result(category, Issue(position, message));

        internal Result<Value> Value(BinaryTree binaryTree)
            => new Result<Value>(new EmptyList(binaryTree), Issue(binaryTree.SourcePart));

        internal Result<Value> Value(Value value)
            => new Result<Value>(value, Issue(value.BinaryTree.SourcePart));

        internal Result<BinaryTree> Syntax(BinaryTree binaryTree)
            => new Result<BinaryTree>(binaryTree, Issue(binaryTree.SourcePart));
    }
}