using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    class DeclarationSyntax : Syntax
    {
        internal readonly DeclarerSyntax Declarer;
        internal readonly ValueSyntax Value;

        public DeclarationSyntax(DeclarerSyntax declarer, BinaryTree target, ValueSyntax value)
            : base(target)
        {
            Tracer.ConditionalBreak(value == null &&
                                    (declarer == null || declarer.Tags == null && declarer.Name == null));
            Declarer = declarer;
            Value = value;
        }

        [DisableDump]
        internal string NameOrNull => Declarer?.Name?.Value;

        [DisableDump]
        internal bool IsConverterSyntax => Declarer?.IsConverterSyntax ?? false;

        [DisableDump]
        internal bool IsMutableSyntax => Declarer?.IsMutableSyntax ?? false;

        protected override int LeftChildCount => 1;
        protected override int DirectChildCount => 2;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 => Declarer
                , 1 => Value
                , _ => null
            };

        internal override Result<ValueSyntax> ToValueSyntax(BinaryTree target)
            => ToCompoundSyntax(target).Apply(syntax => syntax.ToValueSyntax());

        protected override Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree target)
            => new CompoundSyntax(T(this), null, target);

        internal bool IsDefining(string name, bool publicOnly)
            => name != null && NameOrNull == name && (!publicOnly || Declarer.IsPublic);

        internal DeclarationSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }
    }
}