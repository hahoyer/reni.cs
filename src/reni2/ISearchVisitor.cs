using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Sequence;
using Reni.Struct;
using Reni.Type;

namespace Reni
{
    internal interface ISearchVisitor
    {
        void Search();
        void ChildSearch<TType>(TType target) where TType : IDumpShortProvider;
        ISearchVisitor Child(Sequence.SequenceType target);
        ISearchVisitor Child(AutomaticReferenceType target);
        ISearchVisitor Child(AccessType target);
    }
}