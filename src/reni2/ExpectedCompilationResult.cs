using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni
{
    /// <summary>
    ///     Class for additional checks after compilation
    /// </summary>
    internal sealed class ExpectedCompilationResult
    {
        private readonly Compiler _compiler;

        public ExpectedCompilationResult(Compiler compiler) { _compiler = compiler; }

        public int FunctionCount() { return _compiler.Functions.Count; }

        public int FunctionCount(Func<FunctionInstance, bool> fd)
        {
            var result = 0;
            for(var i = 0; i < _compiler.Functions.Count; i++)
            {
                if(fd(_compiler.Functions[i]))
                    result++;
            }
            return result;
        }
    }
}