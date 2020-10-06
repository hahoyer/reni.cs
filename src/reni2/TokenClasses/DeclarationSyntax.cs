using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    class DeclarationSyntax : Syntax
    {
        internal readonly IDefaultScopeProvider Container;

        internal readonly DeclarerSyntax Declarer;
        internal readonly ValueSyntax Value;

        public DeclarationSyntax
        (
            [NotNull] DeclarerSyntax declarer, [NotNull] BinaryTree root, [NotNull] ValueSyntax value
            , [NotNull] IDefaultScopeProvider container
        )
            : base(root)
        {
            Declarer = declarer;
            Value = value;
            Container = container;
        }

        [DisableDump]
        internal bool IsMixInSyntax => Declarer.Tags.Any(item => item.Value is MixInDeclarationToken);

        [DisableDump]
        internal bool IsConverterSyntax => Declarer.Tags.Any(item => item.Value is ConverterToken);

        [DisableDump]
        internal bool IsMutableSyntax => Declarer.Tags.Any(item => item.Value is MutableDeclarationToken);

        [DisableDump]
        internal bool IsPublicSyntax
        {
            get
            {
                if(Declarer.Tags.Any(item => item.Value is PublicDeclarationToken))
                    return true;

                if(Declarer.Tags.Any(item => item.Value is NonPublicDeclarationToken))
                    return false;

                return Container.MeansPublic;
            }
        }

        [DisableDump]
        internal string NameOrNull => Declarer.Name.Value;

        protected override IEnumerable<Syntax> GetChildren() => T((Syntax)Declarer, (Syntax)Value);

        internal override Result<ValueSyntax> ToValueSyntax(BinaryTree target)
            => CompoundSyntax.Create(this, target);

        internal bool IsDefining(string name, bool publicOnly) 
            => name != null && NameOrNull == name && (IsPublicSyntax || !publicOnly);

        internal DeclarationSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return default;
        }
    }
}