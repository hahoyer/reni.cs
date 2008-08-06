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

        internal virtual T ContextRef(ContextRefCode visitedObject)
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
            var tempActual = After(pair.Left.Size);
            var right = pair.Right.Visit(tempActual);
            tempActual = tempActual.After(pair.Right.Size);
            return tempActual.Pair(pair, left, right);
        }

        internal virtual T ChildVisit(Child child)
        {
            var parent = child.Parent.Visit(this);
            var tempActual = After(child.Parent.Size);
            return tempActual.Child(parent, child.LeafElement);
        }

        internal virtual T ThenElseVisit(ThenElse This)
        {
            var condResult = This.CondCode.Visit(this);
            var tempActual = AfterCond(This.ThenElseObjectId);
            var thenResult = This.ThenCode.Visit(tempActual);
            tempActual = tempActual.AfterThen(This.ThenElseObjectId, This.ThenCode.Size);
            var elseResult = This.ElseCode.Visit(tempActual);
            tempActual = tempActual.AfterElse(This.ThenElseObjectId);
            return tempActual.ThenElse(This, condResult, thenResult, elseResult);
        }
    }
}