#region Copyright (C) 2012

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Struct;

namespace Reni
{
    sealed class ExpectedCompilationResult
    {
        readonly Compiler _compiler;

        public ExpectedCompilationResult(Compiler compiler) { _compiler = compiler; }

        public int FunctionCount() { return _compiler.Functions.Count; }

        public int FunctionCount(Func<Function, bool> fd)
        {
            var result = 0;
            for(var i = 0; i < _compiler.Functions.Count; i++)
                if(fd(_compiler.Functions[i]))
                    result++;
            return result;
        }
    }
}