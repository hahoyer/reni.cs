using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Syntax
{
    internal interface IResultProvider
    {
        [DebuggerHidden]
        Result Result(Category category);
    }
}