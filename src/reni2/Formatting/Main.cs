using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Debug;
using Reni.Parser;

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
        {}

        IAssessment ITreeItem.Assess(IAssessor assessor)
            => assessor.Length(Target.Length + (Tail.SourcePart()?.Length ?? 0));

        int ITreeItem.Length => (Target?.Length ?? 0) + Tail.SourcePart().Length;

        string ITreeItem.Reformat(ISubConfiguration configuration)
            => configuration.Parent.Reformat(Target) + Tail.Id();
    }
}