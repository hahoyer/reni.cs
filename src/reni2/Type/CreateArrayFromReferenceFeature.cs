using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    internal sealed class CreateArrayFromReferenceFeature : ReniObject
                                                            , ISearchPath<IPrefixFeature, ReferenceType>
                                                            , IPrefixFeature
                                                            , IFeature
    {
        [EnableDump]
        private readonly TypeBase _type;
        public CreateArrayFromReferenceFeature(TypeBase type) { _type = type; }
        IPrefixFeature ISearchPath<IPrefixFeature, ReferenceType>.Convert(ReferenceType type) { return this; }
        public IFeature Feature { get { return this; } }
        
        Result IFeature.ObtainResult(Category category, RefAlignParam refAlignParam) { return _type.CreateArray(category, refAlignParam); }
        TypeBase IFeature.ObjectType { get { return _type; } }
    }
}