using System;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// General visitor template for code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Visitor<T> : ReniObject
    {
        /// <summary>
        /// Visitor exit when an object of type Arg has been found.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 24.09.2006 20:17
        virtual public T Arg(Arg visitedObject)
        {
            NotImplementedMethod(visitedObject);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contexts the ref.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 17.10.2006 00:04
        virtual public T ContextRef<C>(ContextRef<C> visitedObject) where C : Context.Base
        {
            NotImplementedMethod(visitedObject);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Childs the specified parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="leafElement">The leaf element.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:18
        virtual public T Child(T parent, LeafElement leafElement)
        {
            NotImplementedMethod(parent,leafElement);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Leafs the specified leaf element.
        /// </summary>
        /// <param name="leafElement">The leaf element.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:22
        virtual public T Leaf(LeafElement leafElement)
        {
            NotImplementedMethod(leafElement);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sequences the specified left.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:58
        virtual public T Pair(Pair visitedObject, T left, T right)
        {
            NotImplementedMethod(visitedObject, left, right);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Thens the else.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <param name="condResult">The cond result.</param>
        /// <param name="thenResult">The then result.</param>
        /// <param name="elseResult">The else result.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:54
        virtual public T ThenElse(ThenElse visitedObject, T condResult, T thenResult, T elseResult)
        {
            NotImplementedMethod(visitedObject, condResult,thenResult,elseResult);
            throw new NotImplementedException();
        }
        /// <summary>
        /// Afters the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// created 15.10.2006 18:32
        virtual public Visitor<T> After(Size size)
        {
            return this;
        }

        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        virtual public Visitor<T> AfterCond(int objectId)
        {
            NotImplementedMethod(objectId);
            throw new NotImplementedException();
        }
        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <param name="theSize">The size.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        virtual public Visitor<T> AfterThen(int objectId, Size theSize)
        {
            NotImplementedMethod(objectId, theSize);
            throw new NotImplementedException();
        }
        /// <summary>
        /// Afters the cond.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:52
        virtual public Visitor<T> AfterElse(int objectId)
        {
            NotImplementedMethod(objectId);
            throw new NotImplementedException();
        }

        virtual public T PairVisit(Pair pair)
        {
            T left = pair.Left.Visit(this);
            Visitor<T> tempActual = After(pair.Left.Size);
            T right = pair.Right.Visit(tempActual);
            tempActual = tempActual.After(pair.Right.Size);
            return tempActual.Pair(pair, left, right);
        }

        virtual public T ChildVisit(Child child)
        {
            T parent = child.Parent.Visit(this);
            Visitor<T> tempActual = After(child.Parent.Size);
            return tempActual.Child(parent, child.LeafElement);
        }

        virtual public T ThenElseVisit(ThenElse This)
        {
            T condResult = This.CondCode.Visit(this);
            Visitor<T> tempActual = AfterCond(This.ThenElseObjectId);
            T thenResult = This.ThenCode.Visit(tempActual);
            tempActual = tempActual.AfterThen(This.ThenElseObjectId, This.ThenCode.Size);
            T elseResult = This.ElseCode.Visit(tempActual);
            tempActual = tempActual.AfterElse(This.ThenElseObjectId);
            return tempActual.ThenElse(This, condResult, thenResult, elseResult);
        }

    }
}
