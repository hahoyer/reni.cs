using System;
using System.Diagnostics;
using HWClassLibrary.Debug;

namespace Reni.Syntax
{
    internal interface IResultProvider
    {
        [DebuggerHidden]
        Result Result(Category category);
    }
}