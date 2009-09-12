using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal abstract class PositionFeatureBase : 
        ReniObject, 
        IContextFeature,
        IFeature, ISearchPath<IFeature, Ref>
    {
        private readonly IStructContext _structContext;

        [DumpData(true)]
        private readonly int _index;

        protected PositionFeatureBase(IStructContext structContext, int index)
        {
            _index = index;
            _structContext = structContext;
        }

        IFeature ISearchPath<IFeature, Ref>.Convert(Ref type)
        {
            Tracer.Assert(type.RefAlignParam == _structContext.ForCode.RefAlignParam);
            return this;
        }

        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, TypeBase objectType)
        {
            NotImplementedMethod(category, objectType);
            return null;
        }

        protected virtual Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var rawResult = _structContext.NaturalRefType.AccessResultAsContextRef(category | Category.Type, _index);
            if(args != null)
                rawResult =
                    _structContext
                        .NaturalRefType
                        .AccessResultAsContextRef(category | Category.Type, _index).Type.ApplyFunction(
                        category, callContext, args);

            return PostProcessApplyResult(callContext, category, @object, rawResult);
        }

        private Result PostProcessApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, Result rawResult)
        {
            if(!category.HasCode && !category.HasRefs)
                return rawResult;

            var objectResult = _structContext.ObjectResult(callContext, category, @object);
            var replacedResult =
                new Result(
                    category,
                    () => rawResult.Size,
                    () => rawResult.Type,
                    () => rawResult.Code.ReplaceAbsoluteContextRef(_structContext.ForCode, objectResult.Code),
                    () => (rawResult.Refs - _structContext.ForCode) + objectResult.Refs
                    );
            return replacedResult;
        }

        internal static Result AccessResult(ContextBase callContext, Category category, ICompileSyntax left, int position)
        {
            var objectResult = callContext.ResultAsRef(category | Category.Type, left);
            return objectResult.Type.AccessResultAsArg(category, position).UseWithArg(objectResult);
        }

    }

    [Serializable]
    internal sealed class PositionFeature : PositionFeatureBase
    {
        private readonly PropertyPositionFeature _property;

        internal PositionFeature(EmptyList emptyList, IStructContext structContext, int index)
            : base(structContext, index) { _property = new PropertyPositionFeature(emptyList, structContext, index); }

        public PositionFeatureBase ToProperty(bool isPoperty)
        {
            if(isPoperty)
                return _property;
            return this;
        }
    }

    internal class PropertyPositionFeature : PositionFeatureBase
    {
        private readonly EmptyList _emptyList;

        public PropertyPositionFeature(EmptyList emptyList, IStructContext structContext, int index)
            : base(structContext, index) { _emptyList = emptyList; }

        protected override Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                              ICompileSyntax args)
        {
            var result = base.ApplyResult(callContext, category, @object, _emptyList);
            if(args == null)
                return result;
            NotImplementedMethod(callContext, category, @object, args);
            return null;
        }
    }

    internal interface IStructContext
    {
        Ref NaturalRefType { get; }
        IRefInCode ForCode { get; }
        Result ObjectResult(ContextBase callContext, Category category, ICompileSyntax @object);
    }
}