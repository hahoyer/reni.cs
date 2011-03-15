using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal sealed class CSharpCodeSnippet
    {
        internal readonly string Prerequisites;
        internal readonly string Result;

        internal CSharpCodeSnippet(string prerequisites, string result)
        {
            Prerequisites = prerequisites;
            Result = result;
        }

        internal string Flatten(string resultHeader)
        {
            if(Result == "")
                return Prerequisites;
            return Prerequisites + string.Format(resultHeader, Result);
        }
    }
}