using hw.Helper;
using hw.Scanner;
using Reni.SyntaxTree;

namespace ReniUI.CompilationView
{
    sealed class ResultCachesView : ChildView
    {
        protected override SourcePart[] SourceParts { get; }

        public ResultCachesView(ValueSyntax syntax, SourceView master)
            : base(master, "ResultCaches: " + syntax.GetType().PrettyName() + "-" + syntax.ObjectId)
        {
            Client = syntax.ResultCache.CreateClient(Master);
            SourceParts = syntax.Anchor.SourceParts;
        }
    }
}