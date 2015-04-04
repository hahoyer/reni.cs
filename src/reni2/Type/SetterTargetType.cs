using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    abstract class SetterTargetType
        : TypeBase
            , IProxyType
            , ISimpleFeature
            , IReference
            , ISymbolProvider<ReassignToken, IFeatureImplementation>
    {
        readonly int _order;

        protected SetterTargetType() { _order = CodeArgs.NextOrder++; }

        Size IContextReference.Size => Size;
        int IContextReference.Order => _order;
        ISimpleFeature IProxyType.Converter => this;
        bool IReference.IsWeak => true;
        ISimpleFeature IReference.Converter => this;
        TypeBase ISimpleFeature.TargetType => TargetType;
        Result ISimpleFeature.Result(Category category) => GetterResult(category);
        [DisableDump]
        internal override bool IsAligningPossible => false;
        [DisableDump]
        internal override bool IsPointerPossible => false;

        IFeatureImplementation ISymbolProvider<ReassignToken, IFeatureImplementation>.Feature(ReassignToken tokenClass)
            => IsMutable ? Feature.Extension.FunctionFeature(ReassignResult) : null;

        [EnableDumpExcept(false)]
        protected abstract bool IsMutable { get; }

        protected abstract TypeBase TargetType { get; }

        Result ReassignResult(Category category, TypeBase right)
        {
            if(category == Category.Type)
                return RootContext.VoidType.Result(category);

            var trace = ObjectId == -97 && category.HasCode;
            StartMethodDump(trace, category, right);
            try
            {
                BreakExecution();
                var sourceResult = right
                    .Conversion(category.Typed, ValueType.ForcedPointer);
                Dump("sourceResult", sourceResult);
                BreakExecution();

                var destinationResult = DestinationResult(category.Typed)
                    .ReplaceArg(Result(category.Typed, this));
                Dump("destinationResult", destinationResult);
                BreakExecution();

                var resultForArg = destinationResult + sourceResult;
                Dump("resultForArg", resultForArg);

                var result = SetterResult(category);
                Dump("result", result);
                BreakExecution();

                return ReturnMethodDump(result.ReplaceArg(resultForArg));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal abstract TypeBase ValueType { get; }
        internal virtual Result DestinationResult(Category category) => ArgResult(category);
        protected abstract Result SetterResult(Category category);
        protected abstract Result GetterResult(Category category);

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;

        [DisableDump]
        internal override TypeBase TypeForTypeOperator => ValueType.TypeForTypeOperator;

        [DisableDump]
        internal override TypeBase ElementTypeForReference => ValueType.ElementTypeForReference;

        protected override IEnumerable<ISimpleFeature> RawSymmetricConversions { get { yield break; } }

        [DisableDump]
        internal override IEnumerable<ISimpleFeature> StripConversions
        {
            get { yield return Feature.Extension.SimpleFeature(GetterResult); }
        }
    }
}