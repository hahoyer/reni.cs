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

        protected override Result Result
            (ContextBase context, Category category, TerminalSyntax token)
        {
            var trace = false;
            StartMethodDump(trace, context, category, token);
            try
            {
                var result = context
                    .FindRecentCompoundView
                    .ObjectPointerViaContext(category)
                    ;

                if(category.HasType)
                    result = result.Type.ConvertToStableReference(category).ReplaceArg(result);

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}