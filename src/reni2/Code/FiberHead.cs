using System;
using System.Collections.Generic;

namespace Reni.Code
{
    internal abstract class FiberHead : CodeBase
    {
        protected FiberHead(int objectId)
            : base(objectId) { }

        protected FiberHead() { }

        internal virtual string Format(StorageDescriptor start)
        {
            NotImplementedMethod(start);
            return "";
        }

        internal virtual void Execute(IFormalMaschine formalMaschine) { NotImplementedMethod(formalMaschine); }

        protected virtual CodeBase TryToCombine(FiberItem subsequentElement) { return null; }

        internal override CodeBase CreateFiber(FiberItem subsequentElement)
        {
            var newResult = TryToCombine(subsequentElement);
            if(newResult == null)
                return new Fiber(this, subsequentElement);
            
            return newResult;
        }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.FiberHead(this); }
        
    }
}