using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
        : ContextBase, ISymbolProviderForPointer<Minus>, ISymbolProviderForPointer<ConcatArrays>
    {
        internal interface IParent
        {
            bool ProcessErrors { get; }
            IExecutionContext ExecutionContext { get; }
            IEnumerable<Definable> DefinedNames { get; }
            Result<ValueSyntax> ParsePredefinedItem(string source);
        }

        class CacheContainer
        {
            public ValueCache<BitType> Bit;
            public FunctionCache<bool, IImplementation> CreateArrayFeature;
            public ValueCache<IImplementation> MinusFeature;

            public ValueCache<RecursionType> RecursionType;
            public ValueCache<VoidType> Void;
        }


        [DisableDump]
        [Node]
        readonly FunctionList Functions = new FunctionList();

        new readonly CacheContainer Cache = new CacheContainer();

        [DisableDump]
        [Node]
        readonly IParent Parent;

        internal Root(IParent parent)
        {
            Parent = parent;
            Cache.RecursionType = new ValueCache<RecursionType>(() => new RecursionType(this));
            var metaDictionary = new FunctionCache<string, ValueSyntax>(CreateMetaDictionary);
            Cache.Bit = new ValueCache<BitType>(() => new BitType(this));
            Cache.Void = new ValueCache<VoidType>(() => new VoidType(this));
            Cache.MinusFeature = new ValueCache<IImplementation>
            (
                () =>
                    new ContextMetaFunctionFromSyntax
                        (metaDictionary[ArgToken.TokenId + " " + Negate.TokenId])
            );
            Cache.CreateArrayFeature = new FunctionCache<bool, IImplementation>
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

        public IExecutionContext ExecutionContext => Parent.ExecutionContext;

        [DisableDump]
        internal override Root RootContext => this;

        [DisableDump]
        internal override bool IsRecursionMode => false;

        [DisableDump]
        protected override string LevelFormat => "root context";

        [DisableDump]
        [Node]
        internal BitType BitType => Cache.Bit.Value;

        [DisableDump]
        [Node]
        internal RecursionType RecursionType => Cache.RecursionType.Value;

        [DisableDump]
        [Node]
        internal VoidType VoidType => Cache.Void.Value;

        [DisableDump]
        internal int FunctionCount => Functions.Count;

        internal static RefAlignParam DefaultRefAlignParam
            => new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32));

        [DisableDump]
        public bool ProcessErrors => Parent.ProcessErrors;

        [DisableDump]
        internal IEnumerable<Definable> DefinedNames => Parent.DefinedNames;

        IImplementation ISymbolProviderForPointer<ConcatArrays>.
            Feature(ConcatArrays tokenClass)
            => Cache.CreateArrayFeature[tokenClass.IsMutable];

        IImplementation ISymbolProviderForPointer<Minus>.Feature(Minus tokenClass) => Cache.MinusFeature.Value;

        ValueSyntax CreateMetaDictionary(string source)
        {
            var result = Parent.ParsePredefinedItem(source);
            Tracer.Assert(!result.Issues.Any());
            return result.Target;
        }

        public override string GetContextIdentificationDump() => "r";

        static Result CreateArrayResult(ContextBase context, Category category, ValueSyntax argsType, bool isMutable)
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
            var functionInstance = Functions.Find(body, compoundView, alignedArgsType);
            return functionInstance;
        }

        internal IEnumerable<FunctionType> FunctionInstances
            (CompoundView compoundView, FunctionSyntax body) => Functions.Find(body, compoundView);

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

        internal FunctionContainer FunctionContainer(int index) => Functions.Container(index);
        internal FunctionType Function(int index) => Functions.Item(index);

        internal Container MainContainer(ValueSyntax syntax, string description)
        {
            var rawResult = syntax.Result(this);

            var result = rawResult
                .Code?
                .LocalBlock(rawResult.Type.Copier(Category.Code).Code)
                .Align();

            return new Container(result, rawResult.Issues.ToArray(), description);
        }
    }
}