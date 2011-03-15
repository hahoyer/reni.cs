using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    /// <summary>
    ///     General visitor template for code
    /// </summary>
    /// <typeparam name = "T"></typeparam>
    [Serializable]
    internal abstract class Visitor<T> : ReniObject
    {
        protected Visitor(int objectId)
            : base(objectId) { }

        protected Visitor() { }

        internal virtual T Arg(Arg visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(T);
        }

        internal virtual T ContextRef(ReferenceCode visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(T);
        }

        internal virtual T LocalReference(LocalReference visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(T);
        }

        internal virtual T Fiber(Fiber visitedObject)
        {
            var newHead = visitedObject.FiberHead.Visit(this);
            var data = visitedObject.FiberItems;
            var newItems = new FiberItem[data.Length];
            for(var index = 0; index < data.Length; index++)
                newItems[index] = data[index].Visit(this);
            return Fiber(visitedObject, newHead, newItems);
        }

        protected virtual T Fiber(Fiber visitedObject, T newHead, FiberItem[] newItems)
        {
            NotImplementedMethod(visitedObject, newHead, newItems);
            return default(T);
        }

        private Visitor<T> AfterAny(Size size)
        {
            if(size.IsZero)
                return this;
            return After(size);
        }

        protected virtual Visitor<T> After(Size size) { return this; }

        internal virtual T List(List visitedObject)
        {
            var visitor = this;
            var data = visitedObject.Data;
            var newList = new T[data.Length];
            for(var index = 0; index < data.Length; index++)
            {
                var codeBase = data[index];
                newList[index] = codeBase.Visit(visitor);
                visitor = visitor.AfterAny(codeBase.Size);
            }
            return visitor.List(visitedObject, newList);
        }

        protected virtual T List(List visitedObject, IEnumerable<T> newList)
        {
            NotImplementedMethod(visitedObject, newList);
            return default(T);
        }

        internal abstract T Default();

        internal virtual FiberItem ThenElse(ThenElse visitedObject)
        {
            var newThen = visitedObject.ThenCode.Visit(this);
            var newElse = visitedObject.ElseCode.Visit(this);
            return ThenElse(visitedObject, newThen, newElse);
        }

        protected virtual FiberItem ThenElse(ThenElse visitedObject, T newThen, T newElse)
        {
            NotImplementedMethod(visitedObject, newThen, newElse);
            return null;
        }
    }
}