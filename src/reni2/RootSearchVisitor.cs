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
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni
{
    class RootSearchVisitor<TFeature> : SearchVisitor<TFeature>
        where TFeature : class
    {
        readonly Defineable _defineable;
        internal override sealed IFoundItem[] FoundPath { get; set; }
        TFeature Result { get; set; }

        internal RootSearchVisitor(Defineable defineable) { _defineable = defineable;FoundPath = new IFoundItem[0]; }

        internal override bool IsSuccessFull { get { return Result != null; } }

        internal override TFeature InternalResult
        {
            set
            {
                Tracer.Assert(Result == null || Result == value);
                Result = value;
            }
        }

        internal override Defineable Defineable { get { return _defineable; } }

        internal override Result SearchConverter(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal SearchResult<TFeature> SearchResult
        {
            get
            {
                if(IsSuccessFull)
                    return new SearchResult<TFeature>(Result, FoundPath);
                return null;
            }
        }
    }
}