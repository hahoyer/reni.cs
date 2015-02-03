using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Numeric;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    sealed class Root
        : ContextBase
            , ISymbolProviderForPointer<Minus, IFeatureImplementation>
            , ISymbolProviderForPointer<ConcatArrays, IFeatureImplementation>
    {
        [DisableDump]
        [Node]
        readonly FunctionList _functions = new FunctionList();
        [DisableDump]
        [Node]
        internal readonly IExecutionContext ExecutionContext;

        readonly ValueCache<BitType> _bitCache;
        readonly ValueCache<VoidType> _voidCache;
        readonly ValueCache<IFeatureImplementation> _minusFeatureCache;
        readonly FunctionCache<string, CompileSyntax> _metaDictionary;
        readonly FunctionCache<bool, IFeatureImplementation> _createArrayFeatureCache;

        internal Root(IExecutionContext executionContext)
        {
            _metaDictionary = new FunctionCache<string, CompileSyntax>(CreateMetaDictionary);
            ExecutionContext = executionContext;
            _bitCache = new ValueCache<BitType>(() => new BitType(this));
            _voidCache = new ValueCache<VoidType>(() => new VoidType(this));
            _minusFeatureCache = new ValueCache<IFeatureImplementation>
                (() => new ContextMetaFunctionFromSyntax(_metaDictionary[ArgToken.Id + " " + Negate.Id]));
            _createArrayFeatureCache = new FunctionCache<bool, IFeatureImplementation>
                (
                isMutable =>
                    new ContextMetaFunction((context, category, argsType) => CreateArrayResult(context, category, argsType, isMutable)));
        }

        CompileSyntax CreateMetaDictionary(string source) => ExecutionContext.Parse(source);

        public override string GetContextIdentificationDump() => "r";
        [DisableDump]
        internal override Root RootContext => this;
        public override string DumpPrintText => "root";

        [DisableDump]
        [Node]
        internal BitType BitType => _bitCache.Value;

        [DisableDump]
        [Node]
        internal VoidType VoidType => _voidCache.Value;

        [DisableDump]
        internal int FunctionCount => _functions.Count;

        internal static RefAlignParam DefaultRefAlignParam => new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32));

        public bool ProcessErrors => ExecutionContext.ProcessErrors;

        IFeatureImplementation ISymbolProviderForPointer<Minus, IFeatureImplementation>.Feature(Minus tokenClass) => _minusFeatureCache.Value;

        IFeatureImplementation ISymbolProviderForPointer<ConcatArrays, IFeatureImplementation>.Feature(ConcatArrays tokenClass)
            => _createArrayFeatureCache[tokenClass.IsMutable];

        static Result CreateArrayResult(ContextBase context, Category category, CompileSyntax argsType, bool isMutable)
        {
            var target = context.Result(category.Typed, argsType).SmartUn<PointerType>().Align;
            return target
                .Type
                .Array(1, ArrayType.Options.Create().IsMutable.SetTo(isMutable))
                .Result(category.Typed, target)
                .LocalReferenceResult
                   & category;
        }

        internal FunctionType FunctionInstance(CompoundView compoundView, FunctionSyntax body, TypeBase argsType)
        {
            var alignedArgsType = argsType.Align;
            var functionInstance = _functions.Find(body, compoundView, alignedArgsType);
            return functionInstance;
        }

        internal Result ConcatPrintResult(Category category, int count, Func<Category, int, Result> elemResults)
        {
            var result = VoidType.Result(category);
            if(!(category.HasCode || category.HasExts))
                return result;

            StartMethodDump(false, category, count, "elemResults");
            try
            {
                if(category.HasCode)
                    result.Code = CodeBase.DumpPrintText("(");

                for(var i = 0; i < count; i++)
                {
                    Dump("i", i);

                    var elemResult = elemResults(category, i);

                    Dump("elemResult", elemResult);
                    BreakExecution();

                    result.IsDirty = true;
                    if(category.HasCode)
                    {
                        if(i > 0)
                            result.Code = result.Code + CodeBase.DumpPrintText(", ");
                        result.Code = result.Code + elemResult.Code;
                    }
                    if(category.HasExts)
                        result.Exts = result.Exts.Sequence(elemResult.Exts);
                    result.IsDirty = false;

                    Dump("result", result);
                    BreakExecution();
                }
                if(category.HasCode)
                    result.Code = result.Code + CodeBase.DumpPrintText(")");
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal FunctionContainer FunctionContainer(int index) => _functions.Container(index);

        internal Container MainContainer(Syntax syntax, string description) => ListSyntax
            .Spread(syntax)
            .ToContainer
            .Code(this)
            .Container(description);
    }
}