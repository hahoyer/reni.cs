using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni
{
    /// <summary>
    ///     Parameters for compilation
    /// </summary>
    [Serializable]
    public class CompilerParameters
    {
        public static CompilerParameters CreateTraceAll()
        {
            var result = new CompilerParameters();
            result.Trace.All();
            return result;
        }

        /// <summary>
        ///     Shows or hides syntax tree
        /// </summary>
        [Node, EnableDump]
        public readonly TraceParamters Trace = new TraceParamters();

        public bool ParseOnly;
        public bool RunFromCode;

        [Serializable]
        public class TraceParamters
        {
            [Node, EnableDump]
            public bool CodeSequence;

            [Node, EnableDump]
            public bool CodeTree;

            [Node, EnableDump]
            public bool ExecutedCode;

            [Node, EnableDump]
            public bool Functions;

            [Node, EnableDump]
            public bool Source;

            [Node, EnableDump]
            public bool Syntax;

            [Node, EnableDump]
            public bool CodeExecutor;

            public void None()
            {
                Source = false;
                Syntax = false;
                CodeTree = false;
                CodeSequence = false;
                ExecutedCode = false;
                Functions = false;
                CodeExecutor = false;
            }

            public void All()
            {
                Source = true;
                Syntax = true;
                CodeTree = true;
                CodeSequence = true;
                ExecutedCode = true;
                Functions = true;
                CodeExecutor = true;
            }
        }
    }
}