using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    /// <summary>
    /// General visitor template for code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    internal abstract class Visitor<T> : ReniObject
    {
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
            return Fiber(visitedObject, newHead);
        }

        protected virtual T Fiber(Fiber visitedObject, T head)
        {
            NotImplementedMethod(visitedObject, head);
            return default(T);
        }

        private Visitor<T> AfterAny(Size size)
        {
            if(size.IsZero)
                return this;
            return After(size);
        }

        protected virtual Visitor<T> After(Size size) { return this; }

        protected virtual Visitor<T> AfterCond()
        {
            NotImplementedMethod();
            return null;
        }

        protected virtual Visitor<T> AfterThen(Size theSize)
        {
            NotImplementedMethod(theSize);
            return null;
        }

        protected virtual Visitor<T> AfterElse()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual T List(List visitedObject)
        {
            var visitor = this;
            var data = visitedObject.Data;
            var newList = new List<T>();
            foreach (var element in data)
            {
                var newElement = element.Visit(visitor);
                newList.Add(newElement);
                visitor = visitor.AfterAny(element.Size);
            }
            return visitor.List(visitedObject, newList);
        }

        protected virtual T List(List visitedObject, List<T> newList) 
        {
            NotImplementedMethod(visitedObject, newList);
            return default(T);
        }

        internal abstract T Default();
    }
}