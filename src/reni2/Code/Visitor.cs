using System;
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

        internal virtual T ContextRef(RefCode visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(T);
        }

        internal virtual T InternalRef(InternalRef visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(T);
        }
        internal virtual T Child(T parent, LeafElement leafElement)
        {
            NotImplementedMethod(parent, leafElement);
            return default(T);
        }

        internal virtual T Leaf(LeafElement leafElement)
        {
            NotImplementedMethod(leafElement);
            return default(T);
        }

        internal virtual T Pair(Pair visitedObject, T left, T right)
        {
            NotImplementedMethod(visitedObject, left, right);
            return default(T);
        }

        internal virtual T ThenElse(ThenElse visitedObject, T condResult, T thenResult, T elseResult)
        {
            NotImplementedMethod(visitedObject, condResult, thenResult, elseResult);
            return default(T);
        }

        private Visitor<T> AfterAny(Size size)
        {
            if (size.IsZero)
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

        internal virtual T PairVisit(Pair pair)
        {
            var left = pair.Left.Visit(this);
            var tempActual = AfterAny(pair.Left.Size);
            var right = pair.Right.Visit(tempActual);
            tempActual = tempActual.AfterAny(pair.Right.Size);
            return tempActual.Pair(pair, left, right);
        }

        internal virtual T ChildVisit(Child child)
        {
            var parent = child.Parent.Visit(this);
            var tempActual = AfterAny(child.Parent.Size);
            return tempActual.Child(parent, child.LeafElement);
        }

        internal virtual T ThenElseVisit(ThenElse @this)
        {
            var condResult = @this.CondCode.Visit(this);
            var tempActual = AfterCond();
            var thenResult = @this.ThenCode.Visit(tempActual);
            tempActual = tempActual.AfterThen(@this.ThenCode.Size);
            var elseResult = @this.ElseCode.Visit(tempActual);
            tempActual = tempActual.AfterElse();
            return tempActual.ThenElse(@this, condResult, thenResult, elseResult);
        }

    }
}