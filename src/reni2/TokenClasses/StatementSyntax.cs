using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    class StatementSyntax : Syntax
    {
        [EnableDumpExcept(null)]
        internal readonly DeclarerSyntax Declarer;

        [EnableDumpExcept(null)]
        internal readonly ValueSyntax Value;

        public StatementSyntax(DeclarerSyntax declarer, BinaryTree target, ValueSyntax value)
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
        protected override int LeftDirectChildCount => 1;

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
    }
}