#region Copyright (C) 2012

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Runtime;
using Reni.Struct;

namespace Reni.Code
{
    sealed class CSharpGenerator : ReniObject, IVisitor
    {
        readonly int _temporaryByteCount;
        readonly List<string> _data = new List<string>();
        int _indent;

        public CSharpGenerator(int temporaryByteCount) { _temporaryByteCount = temporaryByteCount; }

        public string Data
        {
            get
            {
                var start = String.Format("\nvar data = Data.Create({0})", _temporaryByteCount);
                return _data
                           .Aggregate(start, (x, y) => x + ";\n" + y)
                       + ";\n";
            }
        }

        [StringFormatMethod("pattern")]
        void AddCode(string pattern, params object[] data)
        {
            var c = String.Format(pattern, data);
            _data.Add("    ".Repeat(_indent) + c);
        }

        string BitCast(Size size, Size dataSize)
        {
            if(size == dataSize)
                return "";
            return String.Format(".BitCast({0}).BitCast({1})", dataSize.ToInt(), size.ToInt());
        }

        static int RefBytes { get { return DataHandler.RefBytes; } }

        void IVisitor.Drop(Size beforeSize, Size afterSize) { throw new NotImplementedException(); }
        void IVisitor.LocalBlockEnd(Size size, Size intermediateSize) { throw new NotImplementedException(); }
        void IVisitor.TopData(Size offset, Size size, Size dataSize) { throw new NotImplementedException(); }

        void IVisitor.BitsArray(Size size, BitsConst data) { AddCode("data.Push({0})", data.ByteSequence(size)); }
        void IVisitor.LocalVariableDefinition(string holderName, Size valueSize) { AddCode("var {0} = data.Pull({1})", holderName, valueSize.SaveByteCount); }
        void IVisitor.LocalVariableReference(string holder, Size offset) { AddCode("data.Push({0}.Address({1}))", holder, offset.SaveByteCount); }
        void IVisitor.RefPlus(Size size) { AddCode("data.RefPlus({0})", size.SaveByteCount); }
        void IVisitor.PrintNumber(Size leftSize, Size rightSize) { AddCode("data.Pull({0}).PrintNumber()", leftSize.SaveByteCount); }
        void IVisitor.PrintText(string dumpPrintText) { AddCode("Data.PrintText({0})", dumpPrintText.Quote()); }
        void IVisitor.TopRef(Size offset) { AddCode("data.Push(data.Address({0}))", offset.SaveByteCount); }
        void IVisitor.TopFrameRef(Size offset) { AddCode("data.Push(frame.Address({0}))", offset.SaveByteCount); }
        void IVisitor.Assign(Size targetSize) { AddCode("data.Assign({0})", targetSize.SaveByteCount); }
        void IVisitor.BitCast(Size size, Size targetSize, Size significantSize) { AddCode("data.Push(data.Pull({0}).BitCast({1}).BitCast({2}))", targetSize.SaveByteCount, significantSize.ToInt(), size.ToInt()); }
        void IVisitor.PrintText(Size leftSize, Size itemSize) { AddCode("data.Pull({0}).PrintText({1})", leftSize.SaveByteCount, itemSize.SaveByteCount); }
        void IVisitor.RecursiveCall() { AddCode("goto Start"); }
        void IVisitor.ReferenceCode(IReferenceInCode context) { throw new UnexpectedContextReference(context); }

        void IVisitor.Call(Size size, FunctionId functionId, Size argsAndRefsSize)
        {
            AddCode
                ("data.Push({0}(data.Pull({1})))"
                 , Generator.FunctionName(functionId)
                 , argsAndRefsSize.SaveByteCount
                );
        }

        void IVisitor.TopFrameData(Size offset, Size size, Size dataSize)
        {
            AddCode
                ("data.Push(frame.Get({0}, {1}){2})"
                 , dataSize.ByteCount
                 , offset.SaveByteCount
                 , BitCast(size, dataSize)
                );
        }

        void IVisitor.LocalVariableAccess(string holder, Size offset, Size size, Size dataSize)
        {
            AddCode
                ("data.Push({0}.Get({1}, {2}){3})"
                 , holder
                 , dataSize.ByteCount
                 , offset.SaveByteCount
                 , BitCast(size, dataSize)
                );
        }

        void IVisitor.Dereference(Size size, Size dataSize)
        {
            AddCode
                ("data.Push(data.Pull({0}).Dereference({1}){2})"
                 , RefBytes
                 , dataSize.ByteCount
                 , BitCast(size, dataSize)
                );
        }

        void IVisitor.BitArrayPrefixOp(ISequenceOfBitPrefixOperation opToken, Size size, Size argSize)
        {
            var sizeBytes = size.SaveByteCount;
            var argBytes = argSize.SaveByteCount;
            if(sizeBytes == argBytes)
                AddCode("data.{0}Prefix(bytes:{1})", opToken.DataFunctionName, sizeBytes);
            else
                AddCode
                    ("data.{0}Prefix(sizeBytes:{1}, argBytes:{2})"
                     , opToken.DataFunctionName
                     , sizeBytes
                     , argBytes
                    );
        }

        void IVisitor.BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
        {
            var sizeBytes = size.SaveByteCount;
            var leftBytes = leftSize.SaveByteCount;
            var rightBytes = rightSize.SaveByteCount;
            if(sizeBytes == 1)
                AddCode
                    ("data.{0}(leftBytes:{1}, rightBytes:{2})"
                     , opToken.DataFunctionName
                     , leftBytes
                     , rightBytes
                    );
            else
                AddCode
                    ("data.{0}(sizeBytes:{1}, leftBytes:{2}, rightBytes:{3})"
                     , opToken.DataFunctionName
                     , sizeBytes
                     , leftBytes
                     , rightBytes
                    );
        }

        void IVisitor.ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
        {
            AddCode("if({0})\n{{", PullBool(condSize.SaveByteCount));
            Indent();
            thenCode.Visit(this);
            Unindent();
            AddCode("}}\nelse\n{{");
            Indent();
            elseCode.Visit(this);
            Unindent();
            AddCode("}}");
        }

        string PullBool(int saveByteCount)
        {
            if(saveByteCount == 1)
                return "data.Pull(1).GetBytes()[0] != 0";
            return "data.Pull(" + saveByteCount + ").IsNotNull()";
        }

        void Unindent() { _indent--; }
        void Indent() { _indent++; }

        void IVisitor.Fiber(FiberHead fiberHead, FiberItem[] fiberItems)
        {
            fiberHead.Visit(this);
            foreach(var fiberItem in fiberItems)
                fiberItem.Visit(this);
        }

        void IVisitor.List(CodeBase[] data)
        {
            foreach(var codeBase in data)
                codeBase.Visit(this);
        }
        internal static string GenerateCSharpStatements(CodeBase codeBase)
        {
            var generator = new CSharpGenerator(codeBase.TemporarySize.SaveByteCount);
            try
            {
                codeBase.Visit(generator);
            }
            catch(UnexpectedContextReference e)
            {
                Tracer.AssertionFailed("", () => e.Message);
            }
            return generator.Data;
        }
    }
}