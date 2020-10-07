using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    class DeclarationSyntax : Syntax
    {
        internal readonly DeclarerSyntax Declarer;
        internal readonly ValueSyntax Value;

        public DeclarationSyntax
            (DeclarerSyntax declarer, BinaryTree root, ValueSyntax value)
            : base(root)
        {
            Declarer = declarer;
            Value = value;
        }

        [DisableDump]
        internal bool IsMixInSyntax => Declarer.Tags.Any(item => item.Value is MixInDeclarationToken);

        [DisableDump]
        internal bool IsConverterSyntax => Declarer.Tags.Any(item => item.Value is ConverterToken);

        [DisableDump]
        internal bool IsMutableSyntax => Declarer.Tags.Any(item => item.Value is MutableDeclarationToken);

        [DisableDump]
        internal bool IsPublic => Declarer.IsPublic;

        [DisableDump]
        internal string NameOrNull => Declarer.Name.Value;

        protected override IEnumerable<Syntax> GetChildren() => T((Syntax)Declarer, (Syntax)Value);

        internal override Result<ValueSyntax> ToValueSyntax(BinaryTree target)
            => ToCompoundSyntax(target).Apply(syntax => syntax.ToValueSyntax());

        internal override Result<CompoundSyntax> ToCompoundSyntax(BinaryTree target)
            => CompoundSyntax.Create(T(this), target);

        internal bool IsDefining(string name, bool publicOnly)
            => name != null && NameOrNull == name && (IsPublic || !publicOnly);

        internal DeclarationSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }
    }
}