using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    internal class PositionFeature :
        ReniObject,
        IContextFeature,
        IFeature
    {
        [DumpData(true)]
        private readonly Context _structContext;

        [DumpData(true)]
        private readonly int _position;

        [DumpData(true)]
        private readonly bool _isProperty;

        public PositionFeature(Context structContext, int position, bool isProperty)
        {
            _isProperty = isProperty;
            _structContext = structContext;
            _position = position;
        }

        TypeBase IFeature.DefiningType() { return _structContext.ContextReferenceType; }
        Result IFeature.Apply(Category category) { return PropertyApply(category, category1 => _structContext.CreateAtResultFromArg(category1, _position)); }
        Result IContextFeature.Apply(Category category) { return PropertyApply(category, category1 => _structContext.CreateAtResultFromContext(category1, _position)); }

        private Result PropertyApply(Category category, Func<Category, Result> getResult)
        {
            var result = getResult(_isProperty ? Category.Type : category);
            if (!_isProperty)
                return result;
            var propertyApply = result
                .Type
                .GetFunctionalFeature()
                .Apply(category, TypeBase.CreateVoid, _structContext.RefAlignParam)
                .ReplaceArg(TypeBase.CreateVoidResult(category));
            return propertyApply;

        }

    }
}