using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    sealed class FunctionFeature : FunctionalFeature, IFunctionFeature
    {
        internal interface ISequenceFeature
        {
            TypeBase ResultType(int objSize, int argsSize);
            NumberType.IOperation Definable { get; }
        }

        readonly SequenceType _objectType;
        ISequenceFeature _feature;

        internal FunctionFeature(SequenceType objectType)
        {
            _objectType = objectType;
            Tracer.Assert(_objectType.Element == _objectType.BitType);
        }

        [DisableDump]
        internal override IContextReference ObjectReference { get { return GetObjectReference(); } }

        internal override Root RootContext { get { return _objectType.RootContext; } }

        [DisableDump]
        IContextReference IFunctionFeature.ObjectReference { get { return ObjectReference; } }

        [DisableDump]
        bool IFunctionFeature.IsImplicit { get { return false; } }

        Result Result(Category category)
        {
            return UniqueFunctionalType()
                .Result(category, _objectType.PointerArgResult(category.Typed));
        }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            return ApplyResult(category, argsType);
        }

        internal override Result ApplyResult(Category category, TypeBase argsType)
        {
            var typedCategory = category.Typed;
            var result = Apply(category, _objectType.Count, argsType.SequenceLength(_objectType.Element));
            var objectResult = GetObjectReference().Result(typedCategory);
            var convertedObjectResult = objectResult.BitSequenceOperandConversion(typedCategory);
            var convertedArgsResult = argsType.BitSequenceOperandConversion(typedCategory);
            return result.ReplaceArg(convertedObjectResult + convertedArgsResult);
        }

        ObjectReference GetObjectReference() { return _objectType.UniqueObjectReference(Root.DefaultRefAlignParam); }

        Result Apply(Category category, int objSize, int argsSize)
        {
            var type = _feature.ResultType(objSize, argsSize);
            return type.Result
                (
                    category,
                    () => _objectType.BitType.ApplyCode(type.Size, _feature.Definable.Name, objSize, argsSize),
                    CodeArgs.Arg);
        }
    }
}