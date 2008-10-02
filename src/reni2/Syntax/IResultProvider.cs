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
        internal static Size Size(this IResultProvider resultProvider) { return resultProvider.Result(Category.Size).Size; }
        internal static TypeBase Type(this IResultProvider resultProvider) { return resultProvider.Result(Category.Type).Type; }
        internal static CodeBase Code(this IResultProvider resultProvider) { return resultProvider.Result(Category.Code).Code; }
        internal static Refs Refs(this IResultProvider resultProvider) { return resultProvider.Result(Category.Refs).Refs; }
    }
}