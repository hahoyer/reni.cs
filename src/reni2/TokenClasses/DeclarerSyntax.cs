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
            internal readonly IDeclarationTag Value;

            internal TagSyntax(IDeclarationTag value, BinaryTree target)
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

        internal readonly NameSyntax Name;
        internal readonly TagSyntax[] Tags;
        internal readonly IDefaultScopeProvider Container;

        DeclarerSyntax(TagSyntax[] tags, BinaryTree target, NameSyntax name, IDefaultScopeProvider container)
            : base(target)
        {
            Tags = tags;
            Name = name;
            Container = container;
        }

        protected override IEnumerable<Syntax> GetChildren()
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
            Tracer.Assert(Container == other.Container);
            return new DeclarerSyntax(Tags.Concat(other.Tags).ToArray(), root, Name ?? other.Name, Container);
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
    }
}