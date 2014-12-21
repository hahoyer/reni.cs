using System.Collections.Generic;
using System.Linq;
using System;
using hw.Scanner;
using Reni.Basics;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.Context
{
    sealed class ContextOperator : NonPrefix
    {
        public override Result Result(ContextBase context, Category category, SourcePart token)
        {
            return context
                .FindRecentContainerView
                .StructReferenceViaContextReference(category);
        }

        public override Result Result(ContextBase context, Category category, CompileSyntax left)
        {
            StartMethodDump(false, context, category, left);
            try
            {
                BreakExecution();
                var leftResult = left.Type(context);
                Dump("leftResult", leftResult);
                BreakExecution();

                var structure = leftResult.FindRecentContainerView;
                Dump("ContainerView", structure);
                BreakExecution();
                if(structure.Hllw)
                {
                    NotImplementedMethod(context, category, left);
                    return null;
                }
                var result = structure.PointerKind.Result(category, structure.Container);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}