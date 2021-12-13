namespace Reni.TokenClasses
{
    static class Whitespace
    {
        internal interface ISpace { }
        internal interface IComment { }
        internal interface ILineBreak { }
        internal interface IStableLineBreak : ILineBreak { }
        internal interface IVolatileLineBreak : ILineBreak { }
    }
}