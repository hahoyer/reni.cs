using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;
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
            Tracer.ConditionalBreak(
                value == null && (declarer == null || declarer.Tags == null && declarer.Name == null)
            );
            Declarer = declarer;
            Value = value;
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Binary?.Token.Characters.GetDumpAroundCurrent(5);

        [DisableDump]
        internal string NameOrNull => Declarer?.Name?.Value;

        [DisableDump]
        internal bool IsConverterSyntax => Declarer?.IsConverterSyntax ?? false;

        [DisableDump]
        internal bool IsMutableSyntax => Declarer?.IsMutableSyntax ?? false;

        [DisableDump]
        internal override int LeftDirectChildCount => 1;

        [DisableDump]
        protected override int DirectChildCount => 2;

        DeclarerSyntax IStatementSyntax.Declarer => Declarer;

        ValueSyntax IStatementSyntax.Value => Value;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 => Declarer
                , 1 => Value
                , _ => null
            };

        internal static Result<IStatementSyntax> Create(DeclarerSyntax declarer, BinaryTree target, ValueSyntax value)
        {
            var issue = (Issue)null;
            if(value == null)
            {
                issue = IssueId.MissingRightExpression.Issue(target.Token.Characters);
                value = new EmptyList(null);
            }

            var result = new Result<IStatementSyntax>((new DeclarationSyntax(declarer, target, value)));
            if(issue != null)
                result = result.With(issue);

            return result;
        }
    }
}