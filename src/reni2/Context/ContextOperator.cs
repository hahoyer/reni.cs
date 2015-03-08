using System.Collections.Generic;
using System.Linq;
using System;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Context
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ContextOperator : NonPrefix, ITokenClassWithId
    {
        public const string Id = "^^";
        string ITokenClassWithId.Id => Id;

        public override Result Result(ContextBase context, Category category, Token token) => context
            .FindRecentCompoundView
            .ObjectPointerViaContext(category);

        public override Result Result(ContextBase context, Category category, CompileSyntax left)
        {
            StartMethodDump(false, context, category, left);
            try
            {
                BreakExecution();
                var leftResult = left.Type(context);
                Dump("leftResult", leftResult);
                BreakExecution();

                var structure = leftResult.FindRecentCompoundView;
                Dump("CompoundView", structure);
                BreakExecution();
                if(structure.Hllw)
                {
                    NotImplementedMethod(context, category, left);
                    return null;
                }
                var result = structure.Type.SmartPointer.Result(category, structure.Compound);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

    }
}