using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    abstract class FunctionalFeature : DumpableObject
    {
        static int _nextObjectId;
        readonly ValueCache<TypeBase> _functionalTypesCache;

        protected FunctionalFeature()
            : base(_nextObjectId++)
        {
            _functionalTypesCache
                = new ValueCache<TypeBase>(() => new FunctionFeatureType(this));
        }

        [DisableDump]
        internal abstract IContextReference ObjectReference { get; }
        [DisableDump]
        internal abstract Root RootContext { get; }

        internal TypeBase FunctionalType() => _functionalTypesCache.Value;
        internal abstract Result ApplyResult(Category category, TypeBase argsType);
    }
}