using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class DeclarerSyntax : Syntax
    {
        internal class TagSyntax : NoChildren
        {
            internal readonly SyntaxFactory.IDeclarerToken Value;

            internal TagSyntax(SyntaxFactory.IDeclarerToken value, BinaryTree target)
                : base(target)
                => Value = value;
        }

        internal class NameSyntax : NoChildren
        {
            internal readonly string Value;

            internal NameSyntax([NotNull] BinaryTree target, [NotNull] string name)
                : base(target)
                => Value = name;
        }

        internal static readonly Result<DeclarerSyntax> Empty
            = new Result<DeclarerSyntax>(new DeclarerSyntax(new TagSyntax[0], null, null, null));

        internal readonly IDefaultScopeProvider Container;

        internal readonly NameSyntax Name;
        internal readonly TagSyntax[] Tags;

        DeclarerSyntax(TagSyntax[] tags, BinaryTree target, NameSyntax name, IDefaultScopeProvider container)
            : base(target)
        {
            Tags = tags;
            Name = name;
            Container = container;
        }

        public bool IsPublic
        {
            get
            {
                if(Tags.Any(item => item.Value is PublicDeclarationToken))
                    return true;

                if(Tags.Any(item => item.Value is NonPublicDeclarationToken))
                    return false;

                return Container.MeansPublic;
            }
        }

        [DisableDump]
        internal bool IsMixInSyntax => Tags.Any(item => item.Value is MixInDeclarationToken);

        [DisableDump]
        internal bool IsConverterSyntax => Tags.Any(item => item.Value is ConverterToken);

        [DisableDump]
        internal bool IsMutableSyntax => Tags.Any(item => item.Value is MutableDeclarationToken);

        protected override IEnumerable<Syntax> GetDirectChildren()
        {
            yield return Name;
            foreach(var tag in Tags)
                yield return tag;
        }

        internal static DeclarerSyntax FromTag
            (DeclarationTagToken tag, BinaryTree target, IDefaultScopeProvider container)
            => new DeclarerSyntax(new[] {new TagSyntax(tag, target)}, target, null, container);

        internal static DeclarerSyntax FromName(BinaryTree target, string name, IDefaultScopeProvider container)
            => new DeclarerSyntax(new TagSyntax[0], target, new NameSyntax(target, name), container);

        internal DeclarerSyntax Combine(DeclarerSyntax other, BinaryTree root)
        {
            if(other == null)
                return this;
            Tracer.Assert(Name == null || other.Name == null);
            Tracer.Assert(Container == null || other.Container == null || Container == other.Container);
            return new DeclarerSyntax(Tags.Concat(other.Tags).ToArray(), root, Name ?? other.Name
                , Container ?? other.Container);
        }
    }



}