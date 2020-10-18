using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.Validation;

namespace Reni.TokenClasses
{
    class DeclarationSyntax : Syntax, IStatementSyntax
    {
        [EnableDumpExcept(null)]
        internal readonly DeclarerSyntax Declarer;

        [EnableDumpExcept(null)]
        internal readonly ValueSyntax Value;

        DeclarationSyntax(DeclarerSyntax declarer, BinaryTree anchor, ValueSyntax value)
            : base(anchor)
        {
            Declarer = declarer;
            Value = value;

            Anchor.AssertIsNotNull();
            Declarer.AssertIsNotNull();
            Value.AssertIsNotNull();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor.Token.Characters.GetDumpAroundCurrent(5);

        [DisableDump]
        internal string NameOrNull => Declarer.Name?.Value;

        [DisableDump]
        internal bool IsConverterSyntax => Declarer.IsConverterSyntax;

        [DisableDump]
        internal bool IsMutableSyntax => Declarer.IsMutableSyntax;

        [DisableDump]
        internal override int LeftDirectChildCount => Declarer.DirectChildCount;

        [DisableDump]
        protected override int DirectChildCount => LeftDirectChildCount + 1;

        [DisableDump]
        DeclarerSyntax IStatementSyntax.Declarer => Declarer;

        ValueSyntax IStatementSyntax.ToValueSyntax(BinaryTree leftAnchor, BinaryTree rightAnchor)
            => new CompoundSyntax(StatementSyntax.Create(leftAnchor, this), null, rightAnchor);

        [DisableDump]
        ValueSyntax IStatementSyntax.Value => Value;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                { } when index < LeftDirectChildCount => Declarer.GetDirectChild(index)
                , { } when index == LeftDirectChildCount => Value
                , _ => null
            };

        internal static Result<IStatementSyntax> Create(DeclarerSyntax declarer, BinaryTree target, ValueSyntax value)
        {
            var issue = (Issue)null;
            if(value == null)
            {
                issue = IssueId.MissingDeclarationValue.Issue(target.Token.Characters);
                value = new EmptyList(null);
            }

            var result = new Result<IStatementSyntax>(new DeclarationSyntax(declarer, target, value));
            if(issue != null)
                result = result.With(issue);

            return result;
        }
    }
}