using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Numeric;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    sealed class Root
        : ContextBase
            , ISymbolProviderForPointer<Minus>
            , ISymbolProviderForPointer<ConcatArrays>
    {
        [DisableDump]
        [Node]
        readonly FunctionList _functions = new FunctionList();
        [DisableDump]
        [Node]
        internal readonly IExecutionContext ExecutionContext;

        readonly ValueCache<RecursionType> _recursionTypeCache;
        readonly ValueCache<BitType> _bitCache;
        readonly ValueCache<VoidType> _voidCache;
        readonly ValueCache<IImplementation> _minusFeatureCache;
        readonly FunctionCache<string, CompileSyntax> _metaDictionary;
        readonly FunctionCache<bool, IImplementation> _createArrayFeatureCache;

        internal Root(IExecutionContext executionContext)
        {
            _recursionTypeCache = new ValueCache<RecursionType>(() => new RecursionType(this));
            _metaDictionary = new FunctionCache<string, CompileSyntax>(CreateMetaDictionary);
            ExecutionContext = executionContext;
            _bitCache = new ValueCache<BitType>(() => new BitType(this));
            _voidCache = new ValueCache<VoidType>(() => new VoidType(this));
            _minusFeatureCache = new ValueCache<IImplementation>
                (
                () =>
                    new ContextMetaFunctionFromSyntax
                        (_metaDictionary[ArgToken.TokenId + " " + Negate.TokenId])
                );
            _createArrayFeatureCache = new FunctionCache<bool, IImplementation>
                (
                isMutable =>
                    new ContextMetaFunction
                        (
                        (context, category, argsType) =>
                            CreateArrayResult
                                (context, category, argsType, isMutable)
                        )
                );
        }

        CompileSyntax CreateMetaDictionary(string source)
        {
            var result = ExecutionContext.Parse(source);
            Tracer.Assert(!result.Issues.Any());
            return result.Value;
        }

        public override string GetContextIdentificationDump() => "r";
        [DisableDump]
        internal override Root RootContext => this;

        [DisableDump]
        internal override bool IsRecursionMode => false;

        [DisableDump]
        [Node]
        internal BitType BitType => _bitCache.Value;

        [DisableDump]
        [Node]
        internal RecursionType RecursionType => _recursionTypeCache.Value;

        [DisableDump]
        [Node]
        internal VoidType VoidType => _voidCache.Value;

        [DisableDump]
        internal int FunctionCount => _functions.Count;

        internal static RefAlignParam DefaultRefAlignParam
            => new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32));

        [DisableDump]
        public bool ProcessErrors => ExecutionContext.ProcessErrors;

        IImplementation ISymbolProviderForPointer<Minus>.Feature
            (Minus tokenClass) => _minusFeatureCache.Value;

        IImplementation ISymbolProviderForPointer<ConcatArrays>.
            Feature(ConcatArrays tokenClass)
            => _createArrayFeatureCache[tokenClass.IsMutable];

        static Result CreateArrayResult
            (ContextBase context, Category category, CompileSyntax argsType, bool isMutable)
        {
            var target = context.Result(category.Typed, argsType).SmartUn<PointerType>().Align;
            return target
                .Type
                .Array(1, ArrayType.Options.Create().IsMutable.SetTo(isMutable))
                .Result(category.Typed, target)
                .LocalReferenceResult
                & category;
        }

        internal FunctionType FunctionInstance
            (CompoundView compoundView, FunctionSyntax body, TypeBase argsType)
        {
            var alignedArgsType = argsType.Align;
            var functionInstance = _functions.Find(body, compoundView, alignedArgsType);
            return functionInstance;
        }

        internal Result ConcatPrintResult
            (Category category, int count, Func<Category, int, Result> elemResults)
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

        internal Container MainContainer(Syntax syntax, string description)
        {
            var compoundSyntax = syntax
                .ToListSyntax
                .ToContainer
                .SaveValue;

            if(Debugger.IsAttached)
            {
                var exts = Result(Category.Exts, compoundSyntax).Exts;
                if(!exts.IsNone)
                {
                    var s = syntax
                        .Closure
                        .OfType<CompileSyntax>()
                        .SelectMany
                        (
                            item =>
                                item.ResultCache.Select
                                    (
                                        r => new
                                        {
                                            syntax = item,
                                            context = r.Key,
                                            exts = r.Value.Exts.Data
                                        }))
                        .Where(item => item.exts.Intersect(exts.Data).Any())
                        .ToArray();

                    var ss = s.GroupBy(item => item.syntax, item => item.context)
                        .Select
                        (
                            item => new
                            {
                                syntax = item.Key,
                                context = item.ToArray()
                            })
                        .ToArray();
                    var sc = s.GroupBy(item => item.context, item => item.syntax)
                        .Select
                        (
                            item => new
                            {
                                context = item.Key,
                                syntax = item.ToArray()
                            })
                        .ToArray();

                    var syntaxData=
                        ss.Select
                        (
                            item =>
                                "==== "
                                    + item.syntax.NodeDump
                                    + " ====\n"
                                    + item.syntax.SourcePart.Id
                        )
                        .Stringify("\n-------------------\n\n\n");

                    Trace.Write(syntaxData);
                    NotImplementedMethod(syntax, description, nameof(exts), exts);
                }
            }

            return compoundSyntax
                .Code(this)
                .Container(description);
        }
    }
}