using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.CompilationView
{
    sealed class ResultCachesView : ChildView
    {
        public ResultCachesView(Value syntax, SourceView master)
            : base(master, "ResultCaches: " + syntax.GetType().PrettyName() + "-" + syntax.ObjectId)
        {
            Client = syntax.ResultCache.CreateClient(Master);
            Source = syntax.SourcePart;
        }

        protected override SourcePart Source { get; }
    }
}