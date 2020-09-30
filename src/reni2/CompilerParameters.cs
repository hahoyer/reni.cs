using System;
using hw.DebugFormatter;

namespace Reni
{
    /// <summary>
    ///     Parameters for compilation
    /// </summary>
    public sealed class CompilerParameters : Attribute
    {
        public sealed class TraceOptionsClass
        {
            [Node]
            [EnableDump]
            public bool CodeSequence;

            [Node]
            [EnableDump]
            public bool ExecutedCode;

            [Node]
            [EnableDump]
            public bool Functions;

            [Node]
            [EnableDump]
            public bool GeneratorFilePosition;

            [Node]
            [EnableDump]
            public bool Parser;

            public void None()
            {
                Parser = false;
                CodeSequence = false;
                ExecutedCode = false;
                Functions = false;
            }

            public void UseOnModeErrorFocus()
            {
                CodeSequence = true;
                ExecutedCode = true;
                Functions = true;
                GeneratorFilePosition = true;
            }
        }

        internal static CompilerParameters CreateTraceAll()
        {
            var result = new CompilerParameters();
            result.TraceOptions.UseOnModeErrorFocus();
            return result;
        }

        /// <summary>
        ///     Shows or hides syntax tree
        /// </summary>
        [Node]
        [EnableDump]
        public readonly TraceOptionsClass TraceOptions = new TraceOptionsClass();

        public bool? ForceDebug;

        public IOutStream OutStream;

        public bool ParseOnly;

        [Node]
        [EnableDump]
        public bool ProcessErrors;

        public bool RunFromCode;
    }

    public class NodeAttribute : Attribute
    {
        string v;

        public NodeAttribute(string v = null) => this.v = v;
    }
}