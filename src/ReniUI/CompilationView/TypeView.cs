using hw.Scanner;
using Reni.Type;

namespace ReniUI.CompilationView
{
    sealed class TypeView : ChildView
    {
        protected override SourcePart[] SourceParts { get; }

        internal TypeView(TypeBase item, SourceView master)
            : base(master, "Type: " + item.NodeDump)
        {
            Client = item.CreateView(Master);
            SourceParts = T(item.GetSource());
        }
    }
}