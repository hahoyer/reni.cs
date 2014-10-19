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
        readonly StructureType _structureType;

        public StructReferenceFeature(StructureType structureType) { _structureType = structureType; }

        Result ISimpleFeature.Result(Category category)
        {
            return _structureType
                .Structure
                .DumpPrintResultViaContextReference(category);
        }
        TypeBase ISimpleFeature.TargetType { get { return _structureType; } }
    }
}