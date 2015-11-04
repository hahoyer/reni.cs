using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using Reni.Parser;

namespace ReniTest.CompilationView
{
    sealed class DetailView : View
    {
        readonly CompileSyntax Syntax;

        public DetailView(CompileSyntax syntax)
            : base(syntax.GetType().PrettyName() + "-" + syntax.ObjectId) { Syntax = syntax; }
    }
}