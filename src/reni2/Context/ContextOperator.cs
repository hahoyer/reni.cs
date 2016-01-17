using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Context
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ContextOperator : TerminalSyntaxToken
    {
        public const string TokenId = "^^";
        public override string Id => TokenId;

        protected override Result Result
            (ContextBase context, Category category, TerminalSyntax token)
        {
            Tracer.Assert(!category.HasCode);
            Tracer.Assert(!category.HasExts);
            return context.Type.Result(category);
        }
    }
}