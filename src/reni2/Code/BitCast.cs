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
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Expression to change size of an expression
    /// </summary>
    [Serializable]
    sealed class BitCast : FiberItem
    {
        static int _nextId;
        readonly Size _outputSize;
        readonly Size _inputSize;

        [Node]
        [DisableDump]
        internal readonly Size InputDataSize;

        internal BitCast(Size outputSize, Size inputSize, Size inputDataSize)
            : base(_nextId++)
        {
            Tracer.Assert(outputSize != inputSize || inputSize != inputDataSize);
            _outputSize = outputSize;
            _inputSize = inputSize;
            InputDataSize = inputDataSize;
            StopByObjectId(-35);
            StopByObjectId(-32);
        }

        [DisableDump]
        internal override Size OutputSize { get { return _outputSize; } }

        [DisableDump]
        internal override Size InputSize { get { return _inputSize; } }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override CodeBase TryToCombineBack(LocalVariableAccess precedingElement)
        {
            return new LocalVariableAccess
                (precedingElement.Holder
                 , precedingElement.Offset
                 , OutputSize
                 , InputDataSize.Min(precedingElement.DataSize)
                );
        }

        internal override FiberItem[] TryToCombineBack(BitCast preceding)
        {
            var inputDataSize = InputDataSize.Min(preceding.InputDataSize);
            if(OutputSize == InputSize && OutputSize == inputDataSize)
                return new FiberItem[0];
            if(OutputSize == preceding.InputSize && OutputSize == inputDataSize)
                return new FiberItem[0];
            return new[] {new BitCast(OutputSize, preceding.InputSize, inputDataSize)};
        }

        internal override CodeBase TryToCombineBack(BitArray precedingElement)
        {
            var bitsConst = precedingElement.Data;
            if(bitsConst.Size > InputDataSize)
                bitsConst = bitsConst.Resize(InputDataSize);
            return new BitArray(OutputSize, bitsConst);
        }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " InputSize=" + InputSize + " InputDataSize=" + InputDataSize; } }

        internal override void Visit(IVisitor visitor) { visitor.BitCast(OutputSize, InputSize, InputDataSize); }

        internal override CodeBase TryToCombineBack(TopData precedingElement)
        {
            if(precedingElement.Size == InputSize && OutputSize >= InputDataSize && OutputSize > InputSize)
            {
                var result = new TopData(precedingElement.Offset, OutputSize, InputDataSize);
                return result
                    .Add(new BitCast(OutputSize, OutputSize, InputDataSize));
            }
            return null;
        }

        internal override CodeBase TryToCombineBack(TopFrameData precedingElement)
        {
            if(precedingElement.Size == InputSize && OutputSize >= InputDataSize && OutputSize > InputSize)
                return new TopFrameData(precedingElement.Offset, OutputSize, InputDataSize)
                    .Add(new BitCast(OutputSize, OutputSize, InputDataSize));
            return null;
        }

        internal override FiberItem[] TryToCombineBack(BitArrayBinaryOp precedingElement)
        {
            if(InputSize == OutputSize)
                return null;

            FiberItem bitArrayOp = new BitArrayBinaryOp(
                precedingElement.OpToken,
                precedingElement.OutputSize + OutputSize - InputSize,
                precedingElement.LeftSize,
                precedingElement.RightSize);

            if(InputDataSize == OutputSize)
                return new[] {bitArrayOp};

            return new[] {bitArrayOp, new BitCast(OutputSize, OutputSize, InputDataSize)};
        }

        internal override FiberItem[] TryToCombineBack(BitArrayPrefixOp precedingElement)
        {
            if(InputSize == OutputSize)
                return null;

            var bitArrayOp = new BitArrayPrefixOp(
                precedingElement.OpToken,
                precedingElement.OutputSize + OutputSize - InputSize,
                precedingElement.ArgSize);

            if(InputDataSize == OutputSize)
                return new FiberItem[] {bitArrayOp};

            return new FiberItem[] {bitArrayOp, new BitCast(OutputSize, OutputSize, InputDataSize)};
        }

        internal override FiberItem[] TryToCombineBack(Dereference preceding)
        {
            if(InputSize == OutputSize && InputDataSize <= preceding.DataSize)
            {
                var dereference = new Dereference(OutputSize, InputDataSize);
                return new FiberItem[] {dereference};
            }

            if(InputSize < OutputSize)
            {
                var dereference = new Dereference(OutputSize, preceding.DataSize);
                if(OutputSize == InputDataSize)
                    return new FiberItem[] {dereference};
                return new FiberItem[] {dereference, new BitCast(OutputSize, OutputSize, InputDataSize)};
            }
            return null;
        }
    }
}