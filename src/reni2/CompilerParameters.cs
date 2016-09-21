﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;


namespace Reni
{
    /// <summary>
    ///     Parameters for compilation
    /// </summary>
    public sealed class CompilerParameters : Attribute
    {
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

        public bool ParseOnly;
        public bool RunFromCode;
        [Node]
        [EnableDump]
        public bool ProcessErrors = false;

        public IOutStream OutStream;

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
            public bool Parser;

            [Node]
            [EnableDump]
            public bool Syntax;
            [Node]
            [EnableDump]
            public bool GeneratorFilePosn;

            public void None()
            {
                Syntax = false;
                Parser = false;
                CodeSequence = false;
                ExecutedCode = false;
                Functions = false;
            }

            public void UseOnModeErrorFocus()
            {
                Syntax = true;
                CodeSequence = true;
                ExecutedCode = true;
                Functions = true;
                GeneratorFilePosn = true;
            }
        }
    }

    public class NodeAttribute : Attribute
    {
        private string v;

        public NodeAttribute(string v=null)
        {
            this.v = v;
        }
    }
}