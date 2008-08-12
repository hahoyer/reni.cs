using System;
using System.Diagnostics;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Type;

namespace Reni.Syntax
{
    internal interface IResultProvider
    {
        [DebuggerHidden]
        Result Result(Category category);
    }

    internal static class ResultProvider
    {
        internal static Size Size(IResultProvider resultProvider) { return resultProvider.Result(Category.Size).Size; }
        internal static TypeBase Type(IResultProvider resultProvider) { return resultProvider.Result(Category.Type).Type; }
        internal static CodeBase Code(IResultProvider resultProvider) { return resultProvider.Result(Category.Code).Code; }
        internal static Refs Refs(IResultProvider resultProvider) { return resultProvider.Result(Category.Refs).Refs; }
    }
}