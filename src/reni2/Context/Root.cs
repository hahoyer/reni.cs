using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Feature;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Context
{
    internal sealed class Root : ReniObject, IContextItem
    {
        private readonly FunctionList _functions;

        internal Root(FunctionList functions) { _functions = functions; }

        RefAlignParam IContextItem.RefAlignParam { get { return DefaultRefAlignParam; } }
        public Result CreateArgsReferenceResult(ContextBase contextBase, Category category) { return null; }
        void IContextItem.Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent) { }

        private static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32)); } }

        [DisableDump]
        internal FunctionList Functions { get { return _functions; } }

        [DisableDump]
        internal CodeBase[] FunctionCode { get { return Functions.Code; } }

        internal Result CreateFunctionCall(AccessPoint context, Category category, ICompileSyntax body, Result argsResult)
        {
            Tracer.Assert(argsResult.HasType);
            var functionInstance = Functions.Find(body, context, argsResult.Type);
            return functionInstance.CreateCall(category, argsResult);
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }
    }
}