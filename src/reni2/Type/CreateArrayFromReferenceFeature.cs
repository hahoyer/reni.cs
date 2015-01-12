using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Basics;

namespace Reni.Type
{
    sealed class CreateArrayFromReferenceFeature : DumpableObject
    {
        [EnableDump]
        readonly TypeBase _type;
        public CreateArrayFromReferenceFeature(TypeBase type) { _type = type; }
        Result Result(Category category) { return _type.CreateArray(category, false); }
    }
}