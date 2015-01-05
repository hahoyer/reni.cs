using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    sealed class StructReferenceFeature
        : DumpableObject
            , ISimpleFeature
    {
        [EnableDump]
        readonly CompoundType _compoundType;

        public StructReferenceFeature(CompoundType compoundType) { _compoundType = compoundType; }

        Result ISimpleFeature.Result(Category category)
        {
            return _compoundType
                .View
                .DumpPrintResultViaContextReference(category);
        }
        TypeBase ISimpleFeature.TargetType { get { return _compoundType; } }
    }
}