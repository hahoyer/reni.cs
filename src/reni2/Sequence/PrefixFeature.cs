#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    sealed class PrefixFeature : DumpableObject, ISimpleFeature
    {
        readonly SequenceType _objectType;
        readonly BitType.IPrefix _definable;

        internal PrefixFeature(SequenceType objectType, BitType.IPrefix definable)
        {
            _objectType = objectType;
            _definable = definable;
        }

        Result ISimpleFeature.Result(Category category)
        {
            return _objectType
                .Result(category, () => _objectType.BitSequenceOperation(_definable.Name), CodeArgs.Arg)
                .ReplaceArg
                (
                    category1
                        => _objectType
                            .UniquePointer
                            .ArgResult(category1.Typed).AutomaticDereferenceResult
                            .Align(Root.DefaultRefAlignParam.AlignBits));
        }
    }
}