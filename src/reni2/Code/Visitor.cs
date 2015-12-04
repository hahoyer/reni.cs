using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;

namespace Reni.Code
{
    abstract class Visitor<TCode, TFiber> : DumpableObject
    {
        protected Visitor(int objectId)
            : base(objectId)
        { }

        protected Visitor() { }

        internal virtual TCode Arg(Arg visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(TCode);
        }

        internal virtual TCode ContextRef(ReferenceCode visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(TCode);
        }

        internal virtual TCode LocalReference(LocalReference visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(TCode);
        }

        internal virtual TCode BitArray(BitArray visitedObject)
        {
            NotImplementedMethod(visitedObject);
            return default(TCode);
        }

        internal virtual TCode Fiber(Fiber visitedObject)
        {
            var newHead = visitedObject.FiberHead.Visit(this);
            var data = visitedObject.FiberItems;
            var newItems = new TFiber[data.Length];
            for (var index = 0; index < data.Length; index++)
                newItems[index] = data[index].Visit(this);
            return Fiber(visitedObject, newHead, newItems);
        }

        protected virtual TCode Fiber(Fiber visitedObject, TCode newHead, TFiber[] newItems)
        {
            NotImplementedMethod(visitedObject, newHead, newItems);
            return default(TCode);
        }


        Visitor<TCode,TFiber> AfterAny(Size size)
        {
            if (size.IsZero)
                return this;
            return After(size);
        }

        protected virtual Visitor<TCode,TFiber> After(Size size) => this;

        internal virtual TCode List(List visitedObject)
        {
            var visitor = this;
            var data = visitedObject.Data;
            var newList = new TCode[data.Length];
            for (var index = 0; index < data.Length; index++)
            {
                var codeBase = data[index];
                newList[index] = codeBase.Visit(visitor);
                visitor = visitor.AfterAny(codeBase.Size);
            }
            return visitor.List(visitedObject, newList);
        }

        protected virtual TCode List(List visitedObject, IEnumerable<TCode> newList)
        {
            NotImplementedMethod(visitedObject, newList);
            return default(TCode);
        }

        internal abstract TCode Default(CodeBase codeBase);

        internal virtual TFiber ThenElse(ThenElse visitedObject)
        {
            var newThen = visitedObject.ThenCode.Visit(this);
            var newElse = visitedObject.ElseCode.Visit(this);
            return ThenElse(visitedObject, newThen, newElse);
        }

        protected virtual TFiber ThenElse(ThenElse visitedObject, TCode newThen, TCode newElse)
        {
            NotImplementedMethod(visitedObject, newThen, newElse);
            return default(TFiber);
        }

        internal virtual TFiber Call(Call visitedObject) => default(TFiber);

        internal virtual TCode TopRef(TopRef visitedObject) => default(TCode);
        
    }
}