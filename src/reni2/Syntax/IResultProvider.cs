using System.Diagnostics;
using HWClassLibrary.Debug;
using System;

namespace Reni.Syntax
{
    internal interface IResultProvider
    {
        [DebuggerHidden]
        Result Result(Category category);
    }
}