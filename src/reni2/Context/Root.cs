using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Syntax;

namespace Reni.Context
{
    /// <summary>
    /// Root environment of compilation process
    /// </summary>
    [Serializable]
    internal sealed class Root : ContextBase
    {
        private readonly FunctionList _function = new FunctionList();

        internal override RefAlignParam RefAlignParam { get { return DefaultRefAlignParam; } }

        private static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32)); } }

        [DumpData(false)]
        internal override Root RootContext { get { return this; } }

        internal List<Container> CompileFunctions()
        {
            return _function.Compile();
        }

        [Node, DumpData(false)]
        public FunctionList Functions { get { return _function; } }

        public Result CreateFunctionCall(ContextBase context, Category category, ICompileSyntax body, Result argsResult)
        {
            return Functions.Find(body, context, argsResult.Type).CreateCall(category, argsResult);
        }
    }
}