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
            throw new NotImplementedException();
        }

        internal virtual T ContextRef(RefCode visitedObject)
        {
            NotImplementedMethod(visitedObject);
            throw new NotImplementedException();
        }

        internal virtual T InternalRef(InternalRef visitedObject)
        {
            NotImplementedMethod(visitedObject);
            throw new NotImplementedException();
        }
        internal virtual T Child(T parent, LeafElement leafElement)
        {
            NotImplementedMethod(parent, leafElement);
            throw new NotImplementedException();
        }

        internal virtual T Leaf(LeafElement leafElement)
        {
            NotImplementedMethod(leafElement);
            throw new NotImplementedException();
        }

        internal virtual T Pair(Pair visitedObject, T left, T right)
        {
            NotImplementedMethod(visitedObject, left, right);
            throw new NotImplementedException();
        }

        internal virtual T ThenElse(ThenElse visitedObject, T condResult, T thenResult, T elseResult)
        {
            NotImplementedMethod(visitedObject, condResult, thenResult, elseResult);
            throw new NotImplementedException();
        }

        internal Visitor<T> AfterAny(Size size)
        {
            if (size.IsZero)
                return this;
            return After(size);
        }

        internal virtual Visitor<T> After(Size size) { return this; }

        internal virtual Visitor<T> AfterCond(int objectId)
        {
            NotImplementedMethod(objectId);
            throw new NotImplementedException();
        }

        internal virtual Visitor<T> AfterThen(int objectId, Size theSize)
        {
            NotImplementedMethod(objectId, theSize);
            throw new NotImplementedException();
        }

        internal virtual Visitor<T> AfterElse(int objectId)
        {
            NotImplementedMethod(objectId);
            throw new NotImplementedException();
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
            var tempActual = AfterCond(@this.ThenElseObjectId);
            var thenResult = @this.ThenCode.Visit(tempActual);
            tempActual = tempActual.AfterThen(@this.ThenElseObjectId, @this.ThenCode.Size);
            var elseResult = @this.ElseCode.Visit(tempActual);
            tempActual = tempActual.AfterElse(@this.ThenElseObjectId);
            return tempActual.ThenElse(@this, condResult, thenResult, elseResult);
        }

    }
}