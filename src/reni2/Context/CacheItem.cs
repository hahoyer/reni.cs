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
using Reni.Syntax;

namespace Reni.Context
{
    internal partial class ContextBase
    {
        internal sealed class CacheItem : ReniObject, IIconKeyProvider, ITreeNodeSupport
        {
            private readonly CompileSyntax _syntax;
            private readonly ContextBase _context;
            private readonly Result _data = new Result();

            [Node]
            public Result Data { get { return _data; } }

            public CacheItem(CompileSyntax syntax, ContextBase context)
            {
                if(syntax == null)
                    throw new NullReferenceException("parameter \"syntax\" must not be null");
                _syntax = syntax;
                _context = context;
            }

            //[DebuggerHidden]
            internal void Update(Category category)
            {
                var trace = _context.ObjectId == -6 && category.HasArgs && _syntax.GetObjectId() == 39;
                StartMethodDump(trace, category);
                try
                {
                    BreakExecution();
                    var localCategory = category - _data.CompleteCategory - _data.PendingCategory;

                    if(localCategory.HasIsDataLess && _data.FindIsDataLess != null)
                    {
                        _data.IsDataLess = _data.FindIsDataLess;
                        localCategory -= Category.IsDataLess;
                    }

                    if(localCategory.HasSize && _data.FindSize != null)
                    {
                        _data.Size = _data.FindSize;
                        localCategory -= Category.Size;
                    }

                    if(localCategory.HasArgs && _data.FindArgs != null)
                    {
                        _data.CodeArgs = _data.FindArgs;
                        localCategory -= Category.Args;
                    }

                    if(localCategory.HasAny)
                    {
                        var oldPendingCategory = _data.PendingCategory;
                        try
                        {
                            _data.PendingCategory |= localCategory;
                            var result = _context.ObtainResult(localCategory, _syntax);
                            result.AssertComplete(localCategory, _syntax);
                            _data.Update(result);
                        }
                        finally
                        {
                            _data.PendingCategory = oldPendingCategory;
                        }
                    }

                    var pendingCategory = category - _data.CompleteCategory;
                    if(pendingCategory.HasAny)
                    {
                        var pendingResult = _context.PendingResult(pendingCategory, _syntax);
                        Tracer.Assert(pendingResult.CompleteCategory == pendingCategory);
                        _data.Update(pendingResult);
                    }

                    ReturnVoidMethodDump(true);
                }
                finally
                {
                    EndMethodDump();
                }
            }

            /// <summary>
            ///     Gets the icon key.
            /// </summary>
            /// <value>The icon key.</value>
            string IIconKeyProvider.IconKey { get { return "Cache"; } }
            TreeNode[] ITreeNodeSupport.CreateNodes() { return Data.CreateNodes(); }
        }
    }
}