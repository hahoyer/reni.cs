using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    class StatementSyntax : Syntax
    {
        [EnableDumpExcept(null)]
        internal readonly DeclarerSyntax Declarer;

        [EnableDumpExcept(null)]
        internal readonly ValueSyntax Value;

        StatementSyntax(DeclarerSyntax declarer, BinaryTree target, ValueSyntax value)
            : base(target)
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

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 => Declarer
                , 1 => Value
                , _ => null
            };

        internal override Result<ValueSyntax> ToValueSyntax()
            => new CompoundSyntax(T(this));

        protected override Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree target)
            => new CompoundSyntax(T(this));

        internal bool IsDefining(string name, bool publicOnly)
            => name != null && NameOrNull == name && (!publicOnly || Declarer.IsPublic);

        internal StatementSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }

        internal static Result<StatementSyntax[]> Create(DeclarerSyntax declarer, BinaryTree target, ValueSyntax value)
        {
            var issue = (Issue)null;
            if(value == null)
            {
                issue = IssueId.MissingRightExpression.Issue(target.Token.Characters);
                value = new EmptyList(null);
            }

            var result = new Result<StatementSyntax[]>(T(new StatementSyntax(declarer, target, value)));
            if(issue != null)
                result = result.With(issue);

            return result;
        }
    }
}