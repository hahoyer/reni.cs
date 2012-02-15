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
using HWClassLibrary.Helper;
using Reni.Basics;

namespace Reni.Code
{
    sealed class FrameData : ReniObject, IStackDataAddressBase
    {
        readonly StackData _data;
        internal readonly DictionaryEx<string, StackData> Locals = new DictionaryEx<string, StackData>();
        internal bool IsRepeatRequired;

        public FrameData(StackData data) { _data = data; }

        StackData IStackDataAddressBase.GetTop(Size offset, Size size) { return _data.DoPull(_data.Size + offset).DoGetTop(size); }
        void IStackDataAddressBase.SetTop(Size offset, StackData right) { NotImplementedMethod(offset, right); }
        string IStackDataAddressBase.Dump() { return "frame{" + _data.Dump() + "}"; }

        public StackData Data { get { return _data; } }
    }
}