using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni
{
    /// <summary>
    /// Parameters for compilation
    /// </summary>
    [Serializable]
    public class CompilerParameters
    {
        static public CompilerParameters CreateTraceAll()
        {
            var result = new CompilerParameters();
            result.Trace.All();
            return result;
        }

        /// <summary>
        /// Shows or hides syntax tree
        /// </summary>
        [Node, IsDumpEnabled(true)]
        public readonly TraceParamters Trace = new TraceParamters();

        public bool ParseOnly;

        [Serializable]
        public class TraceParamters
        {
            /// <summary>
            /// Shows or hides serialize code sequence
            /// </summary>
            [Node, IsDumpEnabled(true)]
            public bool CodeSequence;

            /// <summary>
            /// Shows or hides code tree
            /// </summary>
            [Node, IsDumpEnabled(true)]
            public bool CodeTree;

            /// <summary>
            /// Shows or hides code code to execute
            /// </summary>
            [Node, IsDumpEnabled(true)]
            public bool ExecutedCode;

            [Node, IsDumpEnabled(true)]
            public bool Functions;

            /// <summary>
            /// Shows or hides syntax tree
            /// </summary>
            [Node, IsDumpEnabled(true)]
            public bool Source;

            /// <summary>
            /// Shows or hides syntax tree
            /// </summary>
            [Node, IsDumpEnabled(true)]
            public bool Syntax;

            /// <summary>
            /// Shows or hides a graphic repesentation of generated code
            /// </summary>
            [Node, IsDumpEnabled(true)]
            public bool CodeGraph;

            public void None()
            {
                Source = false;
                Syntax = false;
                CodeTree = false;
                CodeSequence = false;
                CodeGraph = false;
                ExecutedCode = false;
                Functions = false;
            }

            public void All()
            {
                Source = true;
                Syntax = true;
                CodeTree = true;
                CodeSequence = true;
                CodeGraph = false;
                ExecutedCode = true;
                Functions = true;
            }
        }
    }
}