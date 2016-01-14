using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override Result Result(ContextBase context, Category category, TerminalSyntax token)
        {
            var trace = true;
            StartMethodDump(trace, context,category,token);
            try
            {
                return ReturnMethodDump(context
                    .FindRecentCompoundView
                    .ObjectPointerViaContext(category));
            }
            finally
            {
                EndMethodDump();
            }
        }

    }
}