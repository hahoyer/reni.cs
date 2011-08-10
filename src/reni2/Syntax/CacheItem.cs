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
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;

namespace Reni.Syntax
{
    [Serializable]
    internal sealed class CacheItem : ReniObject, IIconKeyProvider, ITreeNodeSupport
    {
        private readonly ICompileSyntax _syntax;
        private readonly ContextBase _context;
        private readonly Result _data = new Result();

        [Node]
        public Result Data { get { return _data; } }

        public CacheItem(ICompileSyntax syntax, ContextBase context)
        {
            if(syntax == null)
                throw new NullReferenceException("parameter \"syntax\" must not be null");
            _syntax = syntax;
            _context = context;
        }

        [DebuggerHidden]
        public Result Result(Category category)
        {
            _data.AddCategories(_context, category, _syntax);
            Tracer.Assert(category <= Data.CompleteCategory);
            return Data & category;
        }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Cache"; } }
        TreeNode[] ITreeNodeSupport.CreateNodes() { return Data.CreateNodes(); }
    }
}