#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     General visitor template for code
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class Visitor<T> : DumpableObject
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

        internal virtual T BitArray(BitArray visitedObject)
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


        Visitor<T> AfterAny(Size size)
        {
            if(size.IsZero)
                return this;
            return After(size);
        }

        protected virtual Visitor<T> After(Size size) => this;

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

        internal abstract T Default(CodeBase codeBase);

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

        internal virtual FiberItem Call(Call visitedObject) => null;
    }
}