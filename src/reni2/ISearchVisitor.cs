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
        void ChildSearch<TType>(TType target) where TType : IDumpShortProvider;
        ISearchVisitor Child(Type.Sequence target);
        ISearchVisitor Child(Struct.Reference target);
        ISearchVisitor Child(Type.Reference target);
    }
}