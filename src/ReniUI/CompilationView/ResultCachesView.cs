using hw.Helper;
using hw.Scanner;
using Reni.SyntaxTree;

namespace ReniUI.CompilationView
{
    sealed class ResultCachesView : ChildView
    {
        protected override SourcePart SourcePart { get; }

        public ResultCachesView(ValueSyntax syntax, SourceView master)
            : base(master, "ResultCaches: " + syntax.GetType().PrettyName() + "-" + syntax.ObjectId)
        {
            Client = syntax.ResultCache.CreateClient(Master);
            SourcePart = syntax.Anchor.SourcePart;
        }
    }
}