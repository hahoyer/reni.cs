using hw.Helper;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.CompilationView
{
    sealed class ResultCachesView : ChildView
    {
        public ResultCachesView(ValueSyntax syntax, SourceView master)
            : base(master, "ResultCaches: " + syntax.GetType().PrettyName() + "-" + syntax.ObjectId)
        {
            Client = syntax.ResultCache.CreateClient(Master);
            SourcePart = syntax.BinaryTree.SourcePart;
        }

        protected override SourcePart SourcePart {get;}
    }
}