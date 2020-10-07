using System;
using hw.DebugFormatter;

namespace Reni
{
    /// <summary>
    ///     Parameters for compilation
    /// </summary>
    public sealed class CompilerParameters : Attribute
    {
        public enum Level
        {
            Scanner = 0
            , Parser = 1
            , Syntax = 2
            , Code = 3
            , Run = 4
        }

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

        public Level CompilationLevel = Level.Run;
        public bool? DebuggableGeneratedCode = false;

        public IOutStream OutStream;

        [Node]
        [EnableDump]
        public bool ProcessErrors;

        public bool RunFromCode;

        /// <summary>
        ///     Shows or hides syntax tree
        /// </summary>
        [Node]
        [EnableDump]
        public readonly TraceOptionsClass TraceOptions = new TraceOptionsClass();

        public bool IsParserRequired => CompilationLevel >= Level.Parser;
        public bool IsSyntaxRequired => CompilationLevel >= Level.Syntax;
        public bool IsCodeRequired => CompilationLevel >= Level.Code;
        public bool IsRunRequired => CompilationLevel >= Level.Run;

        public bool ParseOnly
        {
            set
            {
                Tracer.Assert(value);
                CompilationLevel = Level.Scanner;
            }
        }

        internal static CompilerParameters CreateTraceAll()
        {
            var result = new CompilerParameters();
            result.TraceOptions.UseOnModeErrorFocus();
            return result;
        }
    }

    public class NodeAttribute : Attribute
    {
        string v;

        public NodeAttribute(string v = null) => this.v = v;
    }
}