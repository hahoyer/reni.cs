using System;
using System.Linq;
using System.Collections.Generic;
using Reni.Type;

namespace Reni
{
    internal interface ISearchVisitor
    {
        void Search();
        ISearchVisitor Child(Ref target);
        ISearchVisitor Child(StructRef target);
        ISearchVisitor Child(Sequence target);
        ISearchVisitor Child(Type.Array target);
        ISearchVisitor Child(Type.Void target);
        ISearchVisitor Child(Bit target);
        ISearchVisitor Child(Struct.Type target);
        ISearchVisitor Child(TypeType target);
    }
}