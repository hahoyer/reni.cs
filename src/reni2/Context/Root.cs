using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
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
        : ContextBase,
            ISymbolProviderForPointer<Minus>,
            ISymbolProviderForPointer<ConcatArrays>
    {
        [DisableDump]
        [Node]
        readonly FunctionList _functions = new FunctionList();
        [DisableDump]
        [Node]
        readonly IParent Parent;

        readonly ValueCache<RecursionType> _recursionTypeCache;
        readonly ValueCache<BitType> _bitCache;
        readonly ValueCache<VoidType> _voidCache;
        readonly ValueCache<IImplementation> _minusFeatureCache;
        readonly FunctionCache<string, Parser.Value> _metaDictionary;
        readonly FunctionCache<bool, IImplementation> _createArrayFeatureCache;
        public IExecutionContext ExecutionContext => Parent.ExecutionContext;

        internal Root(IParent parent)
        {
            Parent = parent;
            _recursionTypeCache = new ValueCache<RecursionType>(() => new RecursionType(this));
            _metaDictionary = new FunctionCache<string, Parser.Value>(CreateMetaDictionary);
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

        Parser.Value CreateMetaDictionary(string source)
        {
            var result = Parent.ParsePredefinedItem(source);
            Tracer.Assert(!result.Issues.Any());
            return result.Target;
        }

        public override string GetContextIdentificationDump() => "r";
        [DisableDump]
        internal override Root RootContext => this;

        [DisableDump]
        internal override bool IsRecursionMode => false;
        [DisableDump]
        protected override string LevelFormat => "root context";

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
        public bool ProcessErrors => Parent.ProcessErrors;

        [DisableDump]
        internal IEnumerable<Definable> AllDefinables => Parent.AllDefinables;

        IImplementation ISymbolProviderForPointer<Minus>.Feature
            (Minus tokenClass) => _minusFeatureCache.Value;

        IImplementation ISymbolProviderForPointer<ConcatArrays>.
            Feature(ConcatArrays tokenClass)
            => _createArrayFeatureCache[tokenClass.IsMutable];

        static Result CreateArrayResult
            (ContextBase context, Category category, Parser.Value argsType, bool isMutable)
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

        internal IEnumerable<FunctionType> FunctionInstances
            (CompoundView compoundView, FunctionSyntax body) => _functions.Find(body, compoundView);

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
        internal FunctionType Function(int index) => _functions.Item(index);

        internal Container MainContainer(Syntax syntax, string description)
        {
            var compoundSyntax = CompoundSyntax.Create(syntax.ForceStatements, syntax);

            var rawResult = compoundSyntax.Target.Result(this);

            var result = rawResult
                .Code?
                .LocalBlock(rawResult.Type.Copier(Category.Code).Code)
                .Align();

            return new Container(result, rawResult.Issues, description);
        }

        static string Combine(SourcePart fullSource, SourcePart source)
        {
            Tracer.Assert(fullSource.Contains(source));

            return fullSource.Start.Span(source.Start).Id
                + "["
                + source.Id
                + "]"
                + source.End.Span(fullSource.End).Id;
        }

        internal interface IParent
        {
            Result<Parser.Value> ParsePredefinedItem(string source);
            bool ProcessErrors { get; }
            IExecutionContext ExecutionContext { get; }
            IEnumerable<Definable> AllDefinables { get; }
        }
    }
}