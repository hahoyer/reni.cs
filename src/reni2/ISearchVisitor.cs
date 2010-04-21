using System;
using System.Linq;
using System.Collections.Generic;
using Reni.Struct;
using Reni.Type;

namespace Reni
{
    internal interface ISearchVisitor
    {
        void Search();
        ISearchVisitor Child(Type.Reference target);
        ISearchVisitor Child(Sequence target);
        ISearchVisitor Child(Type.Array target);
        ISearchVisitor Child(Type.Void target);
        ISearchVisitor Child(Bit target);
        ISearchVisitor Child(TypeType target);
        ISearchVisitor Child(Struct.Reference target);
        ISearchVisitor Child(Struct.Type target);
    }
}