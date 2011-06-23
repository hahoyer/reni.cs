using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    ///     Expression to change size of an expression
    /// </summary>
    [Serializable]
    internal sealed class BitCast : FiberItem
    {
        private static int _nextId;
        private readonly Size _outputSize;
        private readonly Size _inputSize;

        [Node]
        private readonly Size _inputDataSize;

        internal BitCast(Size outputSize, Size inputSize, Size inputDataSize)
            : base(_nextId++)
        {
            Tracer.Assert(outputSize != inputSize || inputSize != inputDataSize);
            _outputSize = outputSize;
            _inputSize = inputSize;
            _inputDataSize = inputDataSize;
            StopByObjectId(-5);
        }

        [DisableDump]
        internal override Size OutputSize { get { return _outputSize; } }

        [DisableDump]
        internal override Size InputSize { get { return _inputSize; } }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override FiberItem[] TryToCombineBack(BitCast preceding)
        {
            var inputDataSize = _inputDataSize.Min(preceding._inputDataSize);
            if(OutputSize == InputSize && OutputSize == inputDataSize)
                return new FiberItem[0];
            if(OutputSize == preceding.InputSize && OutputSize == inputDataSize)
                return new FiberItem[0];
            return new[] {new BitCast(OutputSize, preceding.InputSize, inputDataSize)};
        }

        internal override CodeBase TryToCombineBack(BitArray precedingElement)
        {
            var bitsConst = precedingElement.Data;
            if(bitsConst.Size > _inputDataSize)
                bitsConst = bitsConst.Resize(_inputDataSize);
            return new BitArray(OutputSize, bitsConst);
        }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " InputSize=" + InputSize + " InputDataSize=" + _inputDataSize; } }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.BitCast(top, OutputSize, _inputDataSize); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.BitCast(OutputSize, InputSize, _inputDataSize); }

        internal override CodeBase TryToCombineBack(TopData precedingElement)
        {
            if(precedingElement.Size == InputSize && OutputSize >= _inputDataSize && OutputSize > InputSize)
            {
                var result = new TopData(precedingElement.RefAlignParam, precedingElement.Offset, OutputSize, _inputDataSize);
                return result
                    .CreateFiber(new BitCast(OutputSize, OutputSize, _inputDataSize));
            }
            return null;
        }

        internal override CodeBase TryToCombineBack(TopFrameData precedingElement)
        {
            if(precedingElement.Size == InputSize && OutputSize >= _inputDataSize && OutputSize > InputSize)
            {
                return new TopFrameData(precedingElement.RefAlignParam, precedingElement.Offset, OutputSize, _inputDataSize)
                    .CreateFiber(new BitCast(OutputSize, OutputSize, _inputDataSize));
            }
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

            if(_inputDataSize == OutputSize)
                return new[] {bitArrayOp};

            return new[] {bitArrayOp, new BitCast(OutputSize, OutputSize, _inputDataSize)};
        }

        internal override FiberItem[] TryToCombineBack(BitArrayPrefixOp precedingElement)
        {
            if(InputSize == OutputSize)
                return null;

            var bitArrayOp = new BitArrayPrefixOp(
                precedingElement.OpToken,
                precedingElement.OutputSize + OutputSize - InputSize,
                precedingElement.ArgSize);

            if(_inputDataSize == OutputSize)
                return new FiberItem[] {bitArrayOp};

            return new FiberItem[] {bitArrayOp, new BitCast(OutputSize, OutputSize, _inputDataSize)};
        }

        internal override FiberItem[] TryToCombineBack(Dereference preceding)
        {
            if(InputSize == OutputSize && _inputDataSize <= preceding.DataSize)
            {
                var dereference = new Dereference(preceding.RefAlignParam, OutputSize, _inputDataSize);
                return new FiberItem[] {dereference};
            }

            if(InputSize < OutputSize)
            {
                var dereference = new Dereference(preceding.RefAlignParam, OutputSize, preceding.DataSize);
                if(OutputSize == _inputDataSize)
                    return new FiberItem[] {dereference};
                return new FiberItem[] {dereference, new BitCast(OutputSize, OutputSize, _inputDataSize)};
            }
            return null;
        }
    }
}