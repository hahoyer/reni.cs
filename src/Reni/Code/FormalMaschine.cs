using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.Struct;

namespace Reni.Code
{
    [UsedImplicitly]
    sealed class FormalMachine : DumpableObject, IVisitor
    {
        readonly Size StartAddress;

        readonly FormalValueAccess[] Data;
        FormalValueAccess[] FrameData = new FormalValueAccess[0];
        readonly FormalPointer[] Points;
        FormalPointer[] FramePoints = new FormalPointer[1];
        int NextValue;
        internal const string Names =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
        static Size RefSize => Root.DefaultRefAlignParam.RefSize;

        internal FormalMachine(Size dataSize)
        {
            StartAddress = dataSize;
            Data = new FormalValueAccess[dataSize.ToInt()];
            Points = new FormalPointer[dataSize.ToInt() + 1];
        }

        internal string CreateGraph()
        {
            return Data.Aggregate("", (current, t) => current + (t == null ? " ?" : t.Dump()))
                + "  |" +
                FrameData.Aggregate("", (current, t) => current + (t == null ? " ?" : t.Dump()))
                + "\n" +
                Points.Aggregate("", (current, t) => current + (t == null ? "  " : t.Dump())) + "|"
                +
                FramePoints.Aggregate("", (current, t) => current + (t == null ? "  " : t.Dump()))
                + "\n";
        }

        void IVisitor.BitsArray(Size size, BitsConst data)
        {
            var startAddress = (StartAddress - size).ToInt();
            var element = FormalValueAccess.BitsArray(data);
            SetFormalValues(element, startAddress, size);
        }

        void IVisitor.TopRef(Size offset)
        {
            var index = (StartAddress + offset).ToInt();
            FormalPointer.Ensure(Points, index);
            var startAddress = (StartAddress - RefSize).ToInt();
            SetFormalValues(Points[index], startAddress, RefSize);
        }

        void IVisitor.Call(Size size, FunctionId functionId, Size argsAndRefsSize)
        {
            var formalSubValues = PullInputValuesFromData(argsAndRefsSize);
            var startAddress = (StartAddress + argsAndRefsSize - size).ToInt();
            var element = FormalValueAccess.Call(formalSubValues, functionId);
            SetFormalValues(element, startAddress, size);
        }

        void IVisitor.BitCast(Size size, Size targetSize, Size significantSize)
        {
            var formalSubValue = GetInputValuesFromData(significantSize).Single();
            var startAddress = (StartAddress + targetSize - size).ToInt();
            var element = FormalValueAccess.BitCast
                (formalSubValue, (size - significantSize).ToInt());
            SetFormalValues(element, startAddress, size);
        }

        void IVisitor.PrintNumber(Size leftSize, Size rightSize)
        {
            rightSize.IsZero.Assert();
            ResetInputValuesOfData(leftSize);
        }

        void IVisitor.TopFrameData(Size offset, Size size, Size dataSize)
        {
            AlignFrame(offset);
            var access = GetInputValuesFromFrame(offset, size).Single()
                ?? CreateValuesInFrame(size, offset);
            var startAddress = (StartAddress - size).ToInt();
            SetFormalValues(access, startAddress, dataSize);
        }

        void IVisitor.TopData(Size offset, Size size, Size dataSize)
        {
            var source = GetInputValuesFromData(offset, dataSize).Single();
            var startAddress = (StartAddress - size).ToInt();
            SetFormalValues(source, startAddress, dataSize);
        }

        void IVisitor.Drop(Size beforeSize, Size afterSize) => ResetInputValuesOfData(beforeSize - afterSize);

        void IVisitor.ReferencePlus(Size right)
        {
            var formalSubValue = PullInputValuesFromData(RefSize).Single();
            var startAddress = StartAddress.ToInt();
            var element = FormalValueAccess.RefPlus(formalSubValue, right.ToInt());
            SetFormalValues(element, startAddress, RefSize);
        }

        void IVisitor.DePointer(Size size, Size dataSize)
        {
            var formalSubValue = PullInputValuesFromData(RefSize).Single();
            var startAddress = (StartAddress + RefSize - size).ToInt();
            var element = FormalValueAccess.Dereference(formalSubValue);
            SetFormalValues(element, startAddress, dataSize);
        }

        void IVisitor.BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize)
        {
            var formalLeftSubValue = PullInputValuesFromData(leftSize).Single();
            var formalRightSubValue = PullInputValuesFromData(leftSize, rightSize).Single();
            var startAddress = (StartAddress + leftSize + rightSize - size).ToInt();
            var element = FormalValueAccess.BitArrayBinaryOp
                (opToken, formalLeftSubValue, formalRightSubValue);
            SetFormalValues(element, startAddress, size);
        }

        void IVisitor.Assign(Size targetSize) => ResetInputValuesOfData(RefSize * 2);
        void IVisitor.BitArrayPrefixOp(string operation, Size size, Size argSize) => NotImplementedMethod(operation, size, argSize);
        void IVisitor.PrintText(string dumpPrintText) => NotImplementedMethod(dumpPrintText);
        void IVisitor.List(CodeBase[] data) => NotImplementedMethod(data,"","");
        void IVisitor.Fiber(FiberHead fiberHead, FiberItem[] fiberItems) => NotImplementedMethod(fiberHead, fiberItems);
        void IVisitor.RecursiveCall() => throw new NotImplementedException();
        void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode) => NotImplementedMethod(condSize, thenCode, elseCode);
        void IVisitor.TopFrameRef(Size offset) => NotImplementedMethod(offset);
        void IVisitor.ArrayGetter(Size elementSize, Size indexSize)
            => NotImplementedMethod(elementSize, indexSize);

        void IVisitor.ArraySetter(Size elementSize, Size indexSize)
            => NotImplementedMethod(elementSize, indexSize);

        void IVisitor.RecursiveCallCandidate() => throw new NotImplementedException();
        void IVisitor.PrintText(Size leftSize, Size itemSize) => NotImplementedMethod(leftSize, itemSize);

        IFormalValue CreateValuesInFrame(Size size, Size offset)
        {
            var element = FormalValueAccess.Variable(Names[NextValue++]);
            var size1 = size.ToInt();
            var start = FrameData.Length + offset.ToInt();
            for(var i = 0; i < size1; i++)
                FrameData[i + start] = new FormalValueAccess(element, i, size1);
            return element;
        }

        void AlignFrame(Size offset)
        {
            var minSize = -offset.ToInt();
            if(FrameData.Length >= minSize)
                return;

            var frameData = FrameData;
            var framePoints = FramePoints;

            FrameData = new FormalValueAccess[minSize];
            FramePoints = new FormalPointer[minSize + 1];

            var delta = FrameData.Length - frameData.Length;

            for(var i = 0; i < frameData.Length; i++)
                FrameData[i + delta] = frameData[i];
            for(var i = 0; i < framePoints.Length; i++)
                FramePoints[i + delta] = framePoints[i];
        }

        IFormalValue[] GetInputValuesFromFrame(Size offset, Size size)
        {
            var accesses = new List<FormalValueAccess>();
            var start = FrameData.Length + offset.ToInt();
            for(var i = 0; i < size.ToInt(); i++)
                accesses.Add(FrameData[i + start]);
            return FormalValueAccess.Transpose(accesses.ToArray());
        }

        IFormalValue[] PullInputValuesFromData(Size offset, Size inputSize)
        {
            var accesses = new List<FormalValueAccess>();
            var start = (StartAddress + offset).ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
            {
                accesses.Add(Data[i + start]);
                Data[i + start] = null;
            }
            return FormalValueAccess.Transpose(accesses.ToArray());
        }

        IFormalValue[] GetInputValuesFromData(Size inputSize) => GetInputValuesFromData(Size.Zero, inputSize);

        IFormalValue[] GetInputValuesFromData(Size offset, Size inputSize)
        {
            var accesses = new List<FormalValueAccess>();
            var start = (StartAddress + offset).ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
                accesses.Add(Data[i + start]);
            return FormalValueAccess.Transpose(accesses.ToArray());
        }

        IFormalValue[] PullInputValuesFromData(Size inputSize) => PullInputValuesFromData(Size.Zero, inputSize);

        void ResetInputValuesOfData(Size inputSize)
        {
            var start = StartAddress.ToInt();
            for(var i = 0; i < inputSize.ToInt(); i++)
                Data[i + start] = null;
        }

        void SetFormalValues(IFormalValue element, int startAddress, Size size)
        {
            var size1 = size.ToInt();
            for(var i = 0; i < size1; i++)
                Data[i + startAddress] = new FormalValueAccess(element, i, size1);
        }
    }
}