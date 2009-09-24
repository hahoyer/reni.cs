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
        protected readonly IStructContext StructContext;

        [DumpData(true)]
        protected readonly int Position;

        protected PositionFeatureBase(IStructContext structContext, int position)
        {
            Position = position;
            StructContext = structContext;
        }

        IFeature ISearchPath<IFeature, Ref>.Convert(Ref type)
        {
            Tracer.Assert(type.RefAlignParam == StructContext.ForCode.RefAlignParam);
            return this;
        }

        Result IContextFeature.Apply(Category category) { return Apply(category); }

        protected abstract Result Apply(Category category);

        Result IFeature.Apply(Category category, TypeBase objectType)
        {
            NotImplementedMethod(category, objectType);
            return null;
        }

        protected virtual Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var rawResult = StructContext.NaturalRefType.AccessResultAsContextRef(category | Category.Type, Position);
            if(args != null)
                rawResult =
                    StructContext
                        .NaturalRefType
                        .AccessResultAsContextRef(category | Category.Type, Position).Type.ApplyFunction(
                        category, callContext, args);

            return PostProcessApplyResult(callContext, category, @object, rawResult);
        }

        private Result PostProcessApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, Result rawResult)
        {
            if(!category.HasCode && !category.HasRefs)
                return rawResult;

            var objectResult = StructContext.ObjectResult(callContext, category, @object);
            var replacedResult =
                new Result(
                    category,
                    () => rawResult.Size,
                    () => rawResult.Type,
                    () => rawResult.Code.ReplaceAbsoluteContextRef(StructContext.ForCode, objectResult.Code),
                    () => (rawResult.Refs - StructContext.ForCode) + objectResult.Refs
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

        internal PositionFeature(EmptyList emptyList, IStructContext structContext, int position)
            : base(structContext, position) { _property = new PropertyPositionFeature(emptyList, structContext, position); }

        public PositionFeatureBase ToProperty(bool isPoperty)
        {
            if(isPoperty)
                return _property;
            return this;
        }

        protected override Result Apply(Category category)
        {
            return StructContext.NaturalRefType.AccessResultAsArg(category, Position);
        }
    }

    internal class PropertyPositionFeature : PositionFeatureBase
    {
        private readonly EmptyList _emptyList;

        public PropertyPositionFeature(EmptyList emptyList, IStructContext structContext, int position)
            : base(structContext, position) { _emptyList = emptyList; }

        protected override Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                              ICompileSyntax args)
        {
            var result = base.ApplyResult(callContext, category, @object, _emptyList);
            if(args == null)
                return result;
            NotImplementedMethod(callContext, category, @object, args);
            return null;
        }

        protected override Result Apply(Category category)
        {
            NotImplementedMethod(category);
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