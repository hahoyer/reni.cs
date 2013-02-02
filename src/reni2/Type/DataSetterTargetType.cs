#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2013 - 2013 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    abstract class DataSetterTargetType : SetterTargetType
    {
        protected override Result SetterResult(Category category)
        {
            return new Result
                (
                category,
                getCode: SetterCode,
                getArgs: CodeArgs.Arg
                );
        }

        protected override Result GetterResult(Category category)
        {
            return ValueType
                .PointerKind
                .Result(category, GetterCode);
        }

        protected abstract CodeBase SetterCode();
        protected abstract CodeBase GetterCode();
    }
}