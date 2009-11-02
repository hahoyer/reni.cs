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
        /// <summary>
        /// Shows or hides syntax tree
        /// </summary>
        [Node, DumpData(true)]
        public readonly TraceParamters Trace = new TraceParamters();

        public bool ParseOnly;

        [Serializable]
        public class TraceParamters
        {
            /// <summary>
            /// Shows or hides serialize code sequence
            /// </summary>
            [Node, DumpData(true)]
            public bool CodeSequence;

            /// <summary>
            /// Shows or hides code tree
            /// </summary>
            [Node, DumpData(true)]
            public bool CodeTree;

            /// <summary>
            /// Shows or hides code code to execute
            /// </summary>
            [Node, DumpData(true)]
            public bool ExecutedCode;

            [Node, DumpData(true)]
            public bool Functions;

            /// <summary>
            /// Shows or hides syntax tree
            /// </summary>
            [Node, DumpData(true)]
            public bool Source;

            /// <summary>
            /// Shows or hides syntax tree
            /// </summary>
            [Node, DumpData(true)]
            public bool Syntax;

            public void None()
            {
                Source = false;
                Syntax = false;
                CodeTree = false;
                CodeSequence = false;
                ExecutedCode = false;
                Functions = false;
            }

            public void All()
            {
                Source = true;
                Syntax = true;
                CodeTree = true;
                CodeSequence = true;
                ExecutedCode = true;
                Functions = true;
            }
        }
    }
}