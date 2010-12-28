using System;

namespace Reni.Code
{
    internal abstract class StackData: ReniObject
    {
        internal virtual StackData Push(StackData stackData)
        {
            NotImplementedMethod(stackData);
            return null;
        }

        internal virtual StackData PushOnto(NonListStackData formerStack)
        {
            NotImplementedMethod(formerStack);
            return null;
        }

        internal virtual StackData GetTop(Size size)
        {
            if (size == Size)
                return this;
            NotImplementedMethod(size);
            return null;
        }

        internal virtual StackData Pull(Size size)
        {
            if (size.IsZero)
                return this;
            NotImplementedMethod(size);
            return null;
        }

        internal abstract Size Size { get; }
        
        internal virtual StackData Dereference(Size size)
        {
            NotImplementedMethod(size);
            return null;
        }

        internal virtual StackData PushOnto(NonListStackData [] formerStack)
        {
            NotImplementedMethod(formerStack);
            return null;
        }

    }
}