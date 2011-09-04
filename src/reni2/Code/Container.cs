//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     base class for all compiled code items
    /// </summary>
    sealed class Container : ReniObject
    {
        static readonly Container _unexpectedVisitOfPending = new Container("UnexpectedVisitOfPending");
        readonly string _description;

        [EnableDump]
        readonly Size _frameSize;

        [DisableDump]
        readonly CodeBase _data;

        public Container(CodeBase data, string description, Size frameSize = null)
        {
            _frameSize = frameSize ?? Size.Zero;
            _description = description;
            _data = data;
        }

        Container(string errorText)
        {
            _description = errorText;
        }

        [Node]
        [EnableDump]
        internal CodeBase Data { get { return _data; } }

        [Node]
        [EnableDump]
        internal string Description { get { return _description; } }

        [Node]
        [DisableDump]
        internal bool IsError { get { return _frameSize == null; } }

        [Node]
        [DisableDump]
        public Size MaxSize { get { return _data.TemporarySize; } }

        [Node]
        [DisableDump]
        public static Container UnexpectedVisitOfPending { get { return _unexpectedVisitOfPending; } }

        internal BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
        }
        public string GetCSharpStatements()
        {
            var generator = new CSharpGenerator(_data.TemporarySize.SaveByteCount);
            _data.Visit(generator);
            return generator.Data;
        }
    }

    [Serializable]
    class ErrorElement : FiberHead
    {
        [Node]
        internal readonly CodeBase CodeBase;

        public ErrorElement(CodeBase codeBase) { CodeBase = codeBase; }
        protected override Size GetSize() { return Size.Zero; }
    }

    /// <summary>
    ///     Nothing, since void cannot be used for this purpose
    /// </summary>
    class none
    {
        static readonly none _instance = new none();

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        /// created 03.10.2006 01:24
        public static none Instance { get { return _instance; } }
    }
}