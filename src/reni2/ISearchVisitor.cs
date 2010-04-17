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
        ISearchVisitor Child(Reference target);
        ISearchVisitor Child(StructRef target);
        ISearchVisitor Child(Sequence target);
        ISearchVisitor Child(Type.Array target);
        ISearchVisitor Child(Type.Void target);
        ISearchVisitor Child(Bit target);
        ISearchVisitor Child(FullContextType target);
        ISearchVisitor Child(TypeType target);
    }
}