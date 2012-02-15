// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

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
    public sealed class CompilerParameters
    {
        internal static CompilerParameters CreateTraceAll()
        {
            var result = new CompilerParameters();
            result.Trace.All();
            return result;
        }

        /// <summary>
        ///     Shows or hides syntax tree
        /// </summary>
        [Node]
        [EnableDump]
        public readonly TraceParamters Trace = new TraceParamters();

        public bool ParseOnly;
        public bool RunFromCode;
        public IOutStream OutStream;

        [Serializable]
        public sealed class TraceParamters
        {
            [Node]
            [EnableDump]
            public bool CodeSequence;

            [Node]
            [EnableDump]
            public bool CodeTree;

            [Node]
            [EnableDump]
            public bool ExecutedCode;

            [Node]
            [EnableDump]
            public bool Functions;

            [Node]
            [EnableDump]
            public bool Source;

            [Node]
            [EnableDump]
            public bool Syntax;

            [Node]
            [EnableDump]
            public bool CodeExecutor;
            [Node]
            [EnableDump]
            public bool GeneratorFilePosn;

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
                GeneratorFilePosn = true;
            }
        }
    }
}