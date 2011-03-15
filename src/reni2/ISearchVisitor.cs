using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Struct;
using Reni.Type;

namespace Reni
{
    internal interface ISearchVisitor
    {
        void Search();
        void ChildSearch<TType>(TType target) where TType : IDumpShortProvider;
        ISearchVisitor Child(Type.Sequence target);
        ISearchVisitor Child(Field target);
        ISearchVisitor Child(Reference target);
    }
}