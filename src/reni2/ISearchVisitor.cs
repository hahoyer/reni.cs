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
        ISearchVisitor Child(AssignableRef target);
        ISearchVisitor Child(Sequence target);
        ISearchVisitor Child(Type.Array target);
        ISearchVisitor Child(Type.Void target);
        ISearchVisitor Child(Bit target);
    }
}