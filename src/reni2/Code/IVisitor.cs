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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Struct;

namespace Reni.Code
{
    interface IVisitor
    {
        void Assign(Size targetSize);
        void BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize);
        void BitArrayPrefixOp(ISequenceOfBitPrefixOperation opToken, Size size, Size argSize);
        void BitCast(Size size, Size targetSize, Size significantSize);
        void BitsArray(Size size, BitsConst data);
        void Call(Size outputSize, FunctionId functionId, Size argsAndRefsSize);
        void Dereference(Size size, Size dataSize);
        void Drop(Size beforeSize, Size afterSize);
        void PrintNumber(Size leftSize, Size rightSize);
        void PrintText(Size leftSize, Size itemSize);
        void PrintText(string dumpPrintText);
        void Fiber(FiberHead fiberHead, FiberItem[] fiberItems);
        void List(CodeBase[] data);
        void LocalBlockEnd(Size size, Size intermediateSize);
        void LocalVariableAccess(string holder, Size offset, Size size, Size dataSize);
        void LocalVariableDefinition(string holderName, Size valueSize);
        void LocalVariableReference(string holder, Size offset);
        void RecursiveCall();
        void ReferenceCode(IContextReference context);
        void RefPlus(Size right);
        void ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode);
        void TopData(Size offset, Size size, Size dataSize);
        void TopFrameData(Size offset, Size size, Size dataSize);
        void TopRef(Size offset);
        void TopFrameRef(Size offset);
    }
}