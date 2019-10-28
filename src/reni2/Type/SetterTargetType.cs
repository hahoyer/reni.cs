using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    abstract class SetterTargetType
        : TypeBase, IProxyType, IConversion, IValue, IReference, ISymbolProvider<ReassignToken>
    {
        readonly int _order;

        protected SetterTargetType() => _order = CodeArgs.NextOrder++;

        TypeBase IConversion.Source => this;

        Result IConversion.Execute(Category category)
            => GetterResult(category).ConvertToConverter(this);

        IConversion IProxyType.Converter => this;

        int IContextReference.Order => _order;
        bool IReference.IsWeak => true;
        IConversion IReference.Converter => this;

        IImplementation ISymbolProvider<ReassignToken>.Feature(ReassignToken tokenClass)
            => IsMutable ? Feature.Extension.FunctionFeature(ReassignResult) : null;

        Result IValue.Execute(Category category) => GetterResult(category);

        [DisableDump]
        internal override bool IsAligningPossible => false;

        [DisableDump]
        internal override bool IsPointerPossible => false;

        [EnableDumpExcept(false)]
        protected abstract bool IsMutable {get;}

        [DisableDump]
        internal abstract TypeBase ValueType {get;}

        [DisableDump]
        internal override Root Root => ValueType.Root;

        [DisableDump]
        internal override TypeBase TypeForTypeOperator => ValueType.TypeForTypeOperator;

        [DisableDump]
        internal override TypeBase ElementTypeForReference => ValueType.ElementTypeForReference;

        protected override IEnumerable<IConversion> RawSymmetricConversions {get {yield break;}}

        [DisableDump]
        protected override IEnumerable<IConversion> StripConversions
        {
            get {yield return Feature.Extension.Conversion(GetterResult);}
        }

        Result ReassignResult(Category category, TypeBase right)
        {
            if(category == Category.Type)
                return Root.VoidType.Result(category);

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

                var result = SetterResult(category, null);
                Dump("result", result);
                BreakExecution();

                return ReturnMethodDump(result.ReplaceArg(resultForArg));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal virtual Result DestinationResult(Category category) => ArgResult(category);
        protected abstract Result SetterResult(Category category, SourcePart position);
        protected abstract Result GetterResult(Category category);
    }
}