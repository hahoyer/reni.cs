using hw.Scanner;
using Reni.Struct;

namespace ReniUI.CompilationView
{
    sealed class FunctionView : ChildView
    {
        public FunctionView(FunctionType item, SourceView master)
            : base(master, "Function: " + item.NodeDump)
        {
            Client = item.CreateView(Master);
            SourcePart = item.Body.BinaryTree.SourcePart;
        }

        protected override SourcePart SourcePart {get;}
    }
}