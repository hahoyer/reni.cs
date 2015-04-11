using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Debug;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class Main : DumpableObject, ITreeItem
    {
        public static readonly ITreeItemFactory FactoryInstance = new Factory();

        internal readonly ITreeItem Target;
        internal readonly WhiteSpaceToken[] Tail;

        Main(ITreeItem target, WhiteSpaceToken[] tail)
        {
            Target = target;
            Tail = tail;
        }

        sealed class Factory : DumpableObject, ITreeItemFactory
        {
            ITreeItem ITreeItemFactory.Create(ITreeItem left, TokenItem token, ITreeItem right)
            {
                Tracer.Assert(right == null);
                Tracer.Assert(token.Tail.SourcePart() == null);
                return new Main(left, token.Head);
            }
        }

        ITreeItem ITreeItem.List(List level, ListItem left)
        {
            NotImplementedMethod(left, left);
            return null;
        }

        IAssessment ITreeItem.Assess(IAssessor assessor)
            => assessor.Length(Target.Length + (Tail.SourcePart()?.Length ?? 0));

        int ITreeItem.Length => (Target?.Length ?? 0) + Tail.SourcePart().Length;

        string ITreeItem.Reformat(ISubConfiguration configuration)
            => configuration.Parent.Reformat(Target) + Tail.Id();
    }
}