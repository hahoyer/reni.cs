using Reni.Basics;

namespace Reni.Code
{
    sealed class InvalidConversionCode : FiberItem
    {
        static int _nextObjectId;

        internal InvalidConversionCode(Size sourceSize, Size destinationsize)
            : base(_nextObjectId++)
        {
            InputSize = sourceSize;
            OutputSize = destinationsize;
        }

        internal override Size InputSize { get; }
        internal override Size OutputSize { get; }
        internal override void Visit(IVisitor visitor) => NotImplementedMethod(visitor);
    }
}