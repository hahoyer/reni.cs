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
    ///     Root environment of compilation process
    /// </summary>
    [Serializable]
    internal sealed class Root : ContextBase
    {
        private readonly FunctionList _functions = new FunctionList();

        internal override RefAlignParam RefAlignParam { get { return DefaultRefAlignParam; } }

        private static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32)); } }

        [IsDumpEnabled(false)]
        internal override Root RootContext { get { return this; } }

        internal List<Container> CompileFunctions() { return _functions.Compile(); }

        [Node, IsDumpEnabled(false)]
        public FunctionList Functions { get { return _functions; } }

        [Node, IsDumpEnabled(false)]
        internal CodeBase[] FunctionCode { get { return Functions.Code; } }

        public Result CreateFunctionCall(Struct.Context context, Category category, ICompileSyntax body, Result argsResult)
        {
            Tracer.Assert(argsResult.HasType);
            var functionInstance = Functions.Find(body, context, argsResult.Type);
            return functionInstance.CreateCall(category, argsResult);
        }
    }
}