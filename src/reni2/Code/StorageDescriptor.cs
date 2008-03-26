using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Parser.TokenClass;

namespace Reni.Code
{
    /// <summary>
    /// Storage parameters for code generation
    /// </summary>
    public class StorageDescriptor : ReniObject
    {
        private Size _start;
        private readonly Size _dataEndAddr;
        private readonly Size _frameSize;

        /// <summary>
        /// Current start address.
        /// </summary>
        /// <value>The start.</value>
        /// created 26.11.2006 13:17
        public Size Start { get { return _start; } }

        /// <summary>
        /// Gets the data end addr.
        /// </summary>
        /// <value>The data end addr.</value>
        /// created 26.11.2006 15:08
        public Size DataEndAddr { get { return _dataEndAddr; } }

        /// <summary>
        /// Initializes a new instance of the StorageDescriptor class.
        /// </summary>
        /// <param name="dataEndAddr">The data end addr.</param>
        /// <param name="frameSize">Size of the args.</param>
        /// created 26.11.2006 13:17
        public StorageDescriptor(Size dataEndAddr, Size frameSize)
        {
            if (dataEndAddr == null)
                throw new ArgumentNullException("dataEndAddr");
            if (frameSize == null)
                throw new ArgumentNullException("frameSize");
            
            _dataEndAddr = dataEndAddr;
            _frameSize = frameSize;
            _start = dataEndAddr;
        }

        /// <summary>
        /// Shifts the start address.
        /// </summary>
        /// <param name="deltaSize">Size of the delta.</param>
        /// created 26.11.2006 13:18
        public void ShiftStartAddress(Size deltaSize)
        {
            _start += deltaSize;
        }
        /// <summary>
        /// Generates the function return.
        /// </summary>
        /// <value>The function return.</value>
        /// created 26.11.2006 23:22
        [DumpData(false)]
        private string FunctionReturn
        {
            get
            {
                Size resultSize = DataEndAddr - Start;
                if (_frameSize.IsZero && resultSize.IsZero)
                    return "";
                return CreateMoveBytesToFrame(resultSize, resultSize, Start)
                    + "; // StorageDescriptor.FunctionReturn";
            }
        }

        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="isFunction">if set to <c>true</c> [is function].</param>
        /// <returns></returns>
        public string GetBody(List<LeafElement> data, bool isFunction)
        {
            string result = GetStatements(data);
            if (isFunction)
                result =
                    "StartFunction:\n"
                    + result
                    + FunctionReturn;
            return result;
        }

        /// <summary>
        /// Assigns the specified ref align param.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public string Assign(RefAlignParam refAlignParam, Size size)
        {
            if (refAlignParam.Is_3_32)
            {
                string dest = CreateDataRef(Start+size, refAlignParam.RefSize);
                if (IsBuildInIntType(size))
                    return "*(" 
                        + CreateIntPtrCast(size) 
                        + dest 
                        + ") = " 
                        + CreateDataRef(Start, size);

                return "Data.MoveBytes("
                   + size.ByteCount
                   + ", "
                   + "(sbyte*)" + dest
                   + ", "
                   + CreateDataPtr(Start)
                   + ")";
            }

            NotImplementedFunction(this, refAlignParam, size);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a prefix operation.
        /// </summary>
        /// <param name="opToken">The op token.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        internal string BitArrayPrefixOp(Defineable opToken, Size size)
        {
            if (IsBuildInIntType(size))
                return CreateDataRef(Start, size)
                       + " = "
                       + CreateIntCast(size)
                       + "("
                       + opToken.CSharpNameOfDefaultOperation
                       + " "
                       + CreateDataRef(Start, size)
                       + ")";

            return "Data."
                + opToken.DataFunctionName
                + "("
                + size.ByteCount
                + ", "
                + CreateDataPtr(Start)
                + ")";
        }

        /// <summary>
        /// Bits the array op.
        /// </summary>
        /// <param name="opToken">The op token.</param>
        /// <param name="resultSize">The resultSize.</param>
        /// <param name="leftSize">Size of the left.</param>
        /// <param name="rightSize">Size of the right.</param>
        /// <returns></returns>
        /// created 10.10.2006 00:29
        internal string BitArrayOp(Defineable opToken, Size resultSize, Size leftSize, Size rightSize)
        {
            if (IsBuildInIntType(leftSize) && IsBuildInIntType(rightSize))
            {
                string value = GetValueForBuiltInIntType(leftSize, opToken, rightSize);
                if(opToken.IsCompareOperator)
                    value = "(" + value + "? -1:0" + ")";
                return GetFrameForBuiltInIntType(resultSize, leftSize + rightSize) + value;
            }

            return "Data."
                + opToken.DataFunctionName
                + "("
                + resultSize.ByteCount
                + ", "
                + CreateDataPtr(Start+leftSize+rightSize-resultSize)
                + ", "
                + leftSize.ByteCount
                + ", "
                + CreateDataPtr(Start+rightSize)
                + ", "
                + rightSize.ByteCount
                + ", "
                + CreateDataPtr(Start)
                + ")";

        }

        /// <summary>
        /// Creates a conditional operation that is used as header of then-else construction
        /// </summary>
        /// <param name="opToken">The op token.</param>
        /// <param name="leftSize">Size of the left.</param>
        /// <param name="rightSize">Size of the right.</param>
        /// <param name="thenElseObjectId">The then else object id.</param>
        /// <param name="condSize">Size of the cond.</param>
        /// <returns></returns>
        internal string BitArrayOpThen(Defineable opToken, Size leftSize, Size rightSize, int thenElseObjectId, Size condSize)
        {
            if (IsBuildInIntType(leftSize) && IsBuildInIntType(rightSize))
                return "if(!("
                   + CreateDataRef(Start + rightSize, leftSize)
                   + " "
                   + opToken.CSharpNameOfDefaultOperation
                   + " "
                   + CreateDataRef(Start, rightSize)
                   + ")) goto Else" + thenElseObjectId;
            return "if(!Data."
                + opToken.DataFunctionName
                + "("
                + leftSize.ByteCount
                + ", "
                + CreateDataPtr(Start + rightSize)
                + ", "
                + rightSize.ByteCount
                + ", "
                + CreateDataPtr(Start)
                + ")) goto Else" + thenElseObjectId;
        }

        /// <summary>
        /// Bits the cast.
        /// </summary>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="size">The size.</param>
        /// <param name="significantSize">Size of the significant.</param>
        /// <returns></returns>
        /// created 10.10.2006 00:24
        public string BitCast(Size targetSize, Size size, Size significantSize)
        {
            int targetBytes = targetSize.ByteCount;
            int bytes = size.ByteCount;
            if (targetBytes == bytes)
            {
                if (significantSize == Size.Byte * bytes)
                    return "";
                int bits = (size - significantSize).ToInt();
                if (IsBuildInIntType(size))
                {
                    string result =
                        CreateDataRef(Start, size)
                        + " = "
                        + CreateIntCast(size)
                        + "("
                        + CreateIntCast(size)
                        + "("
                        + CreateDataRef(Start, size) + " << " + bits
                        + ") >> "
                        + bits
                        + ")";
                    return result;
                }
                return "Data.BitCast("
                   + size.ByteCount
                   + ", "
                   + CreateDataPtr(Start)
                   + ", "
                   + bits
                   + ")";
            }

            NotImplementedFunction(this, targetSize, size);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the code for a bitsarray.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// created 10.10.2006 00:27
        public string BitsArray(Size size, BitsConst data)
        {
            if (IsBuildInIntType(size))
                return
                    CreateDataRef(Start - size, size)
                    + " = "
                    + CreateIntCast(size)
                    + "("
                    + data.CodeDump()
                    + ")";

            string result = "";
            for (int i = 0; i < size.ByteCount; i++)
            {
                result += ", ";
                result += data.Byte(i);
            }

            return "Data.BitsArray("
                       + size.ByteCount
                       + ", "
                       + CreateDataPtr(Start - size)
                       + result
                       + ")";
        }

        /// <summary>
        /// Creates the code for a call.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="frameSize">Size of the args and refs.</param>
        /// <returns></returns>
        /// created 15.11.2006 21:41
        public string Call(int index, Size frameSize)
        {
            return Generator.FunctionMethodName(index) + "(" + CreateDataPtr(Start + frameSize) + ")";
        }


        /// <summary>
        /// Creates the code for dumping numbers.
        /// </summary>
        /// <param name="leftSize">Size of the left.</param>
        /// <param name="rightSize">Size of the right.</param>
        /// <returns></returns>
        /// created 08.01.2007 16:39
        public string DumpPrint(Size leftSize, Size rightSize)
        {
            if (rightSize.IsZero)
                return BitArrayDumpPrint(leftSize);
            NotImplementedMethod(leftSize, rightSize);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the code for dumping text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        /// created 08.01.2007 18:38
        public string DumpPrintText(string text)
        {
            return "Data.DumpPrint(" + HWString.ToStringLiteral(text) + ")";
        }
        
        /// <summary>
        /// Creates the code for else construct.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:12
        public string Else(int objectId)
        {
            return "goto EndCondition" + objectId + "; Else" + objectId + ":";
        }

        /// <summary>
        /// Creates the endig code of a then-else construct.
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:09
        public string EndCondional(int objectId)
        {
            return "EndCondition" + objectId + ":";
        }

        /// <summary>
        /// Creates a recursive call, i. e. a jump to start of function..
        /// </summary>
        /// <returns></returns>
        public string RecursiveCall()
        {
            return "goto StartFunction";
        }
        /// <summary>
        /// Creates the code for incrementing a reference.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 21.10.2006 02:10
        public string RefPlus(Size size, int right)
        {
            if (right == 0)
                return "";

            if (size.ToInt() == 32)
                return CreateDataRef(Start, size) + " += " + right;

            NotImplementedFunction(this, size, right);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the code for copying things from frame to top of data area.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="destinationSize">Size of the destination.</param>
        /// <returns></returns>
        /// created 04.01.2007 16:38
        public string TopFrame(RefAlignParam refAlignParam, Size offset, Size targetSize, Size destinationSize)
        {
            if (refAlignParam.Is_3_32)
                return CreateMoveBytesFromFrame(targetSize, Start - destinationSize, offset*-1);

            NotImplementedFunction(this, refAlignParam, offset, targetSize, destinationSize);
            throw new NotImplementedException();
        }
        /// <summary>
        /// Creates the code for end of statement cleanup.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="bodySize">The body.</param>
        /// <returns></returns>
        /// created 10.10.2006 00:25
        public string StatementEnd(Size size, Size bodySize)
        {
            return CreateMoveBytes(size, Start + bodySize, Start);

        }

        /// <summary>
        /// Creates a conditional operation that is used as header of then-else construction
        /// </summary>
        /// <param name="objectId">The object id.</param>
        /// <param name="condSize">Size of the cond.</param>
        /// <returns></returns>
        /// created 09.01.2007 04:15
        public string Then(int objectId, Size condSize)
        {
            return "if(" + CreateDataRef(Start, condSize) + "==0) goto Else" + objectId;
        }

        /// <summary>
        /// Creates the code for copying things from data area to top of data area.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="destinationSize">Size of the destination.</param>
        /// <returns></returns>
        /// created 19.10.2006 22:27
        public string TopData(RefAlignParam refAlignParam, Size offset, Size targetSize, Size destinationSize)
        {
            if (refAlignParam.Is_3_32)
                return CreateMoveBytes(targetSize, Start - destinationSize, Start + offset);

            NotImplementedFunction(this, refAlignParam, offset, targetSize, destinationSize);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the code for a referenece to top of data area.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// created 21.10.2006 02:10
        public string TopRef(RefAlignParam refAlignParam, Size offset)
        {
            if (refAlignParam.Is_3_32)
            {
                return CreateDataRef(Start - refAlignParam.RefSize, refAlignParam.RefSize)
                       + " = "
                       + CreateIntCast(refAlignParam.RefSize)
                       + CreateDataPtr(Start + offset);
            }

            NotImplementedMethod(refAlignParam);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a reference to frame.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// created 03.01.2007 22:45
        public string FrameRef(RefAlignParam refAlignParam, Size offset)
        {
            if (refAlignParam.Is_3_32)
            {
                return CreateDataRef(Start - refAlignParam.RefSize, refAlignParam.RefSize)
                       + " = "
                       + CreateIntCast(refAlignParam.RefSize)
                       + CreateFrameBackPtr(offset*-1);
            }

            NotImplementedMethod(refAlignParam, offset);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the code copy the content of a reference to to of data area
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="destinationSize">Size of the destination.</param>
        /// <returns></returns>
        /// created 10.10.2006 00:24
        public string Unref(RefAlignParam refAlignParam, Size targetSize, Size destinationSize)
        {
            if (refAlignParam.Is_3_32)
            {
                if (IsBuildInIntType(targetSize))
                    return CreateDataRef(Start + refAlignParam.RefSize - destinationSize, targetSize)
                       + " = "
                       + CreateCastToIntRef(targetSize, CreateDataRef(Start, refAlignParam.RefSize));
                return "Data.MoveBytes("
                   + targetSize.ByteCount
                   + ", "
                   + CreateDataPtr(Start + refAlignParam.RefSize - destinationSize)
                   + ", "
                   + CreateCastToIntPtr(Size.Byte, CreateDataRef(Start, refAlignParam.RefSize))
                   + ")";
            }
            NotImplementedMethod(refAlignParam, targetSize, destinationSize);
            throw new NotImplementedException();
        }

        private string BitArrayDumpPrint(Size size)
        {
            if (IsBuildInIntType(size))
                return "Data.DumpPrint(" + CreateDataRef(Start, size) + ")";
            return "Data.DumpPrint(" + size.ByteCount + ", " + CreateDataPtr(Start) + ")";
        }

        private string GetFrameForBuiltInIntType(Size resultSize, Size argsSize)
        {
            return CreateDataRef(Start + argsSize - resultSize, resultSize)
                   + " = "
                   + CreateIntCast(resultSize);
        }

        private string GetValueForBuiltInIntType(Size leftSize, Defineable opToken, Size rightSize)
        {
            return "("
                   + CreateDataRef(Start + rightSize, leftSize)
                   + " "
                   + opToken.CSharpNameOfDefaultOperation
                   + " "
                   + CreateDataRef(Start, rightSize)
                   + ")";
        }

        private static string CreateCastToIntPtr(Size size, string result)
        {
            return "(" + CreateIntPtrCast(size) + " " + result + ")";
        }

        private static string CreateCastToIntRef(Size size, string result)
        {
            return "(*" + CreateIntPtrCast(size) + " " + result + ")";
        }

        private static string CreateDataPtr(Size start)
        {
            return "(data+" + start.SaveByteCount + ")";
        }

        private static string CreateDataRef(Size start, Size size)
        {
            return CreateCastToIntRef(size, CreateDataPtr(start));
        }

        private static string CreateFrameBackPtr(Size start)
        {
            return "(frame-" + start.SaveByteCount + ")";
        }

        private static string CreateFrameBackRef(Size start, Size size)
        {
            return CreateCastToIntRef(size, CreateFrameBackPtr(start));
        }

        private static string CreateIntCast(Size size)
        {
            int bits = size.ByteCount * 8;
            switch (bits)
            {
                case 8:
                case 16:
                case 32:
                case 64:
                    return "(" + CreateIntType(size) + ")";
            }
            NotImplementedFunction(size, "bits", bits);
            throw new NotImplementedException();
        }

        private static string CreateIntPtrCast(Size size)
        {
            return "(" + CreateIntType(size) + "*)";
        }

        private static string CreateIntType(Size size)
        {
            int bits = size.ByteCount * 8;
            switch (bits)
            {
                case 8:
                    return "sbyte";
                case 16:
                case 32:
                case 64:
                    return "Int" + bits;
            }
            NotImplementedFunction(size, "bits", bits);
            throw new NotImplementedException();
        }

        private static string CreateMoveBytes(Size size, Size destStart, Size sourceStart)
        {
            if (size.IsZero)
                return "";
            if (destStart == sourceStart)
                return "";

            if (IsBuildInIntType(size))
                return CreateDataRef(destStart, size)
                   + " = "
                   + CreateDataRef(sourceStart, size)
                ;
            return "Data.MoveBytes("
               + size.ByteCount
               + ", "
               + CreateDataPtr(destStart)
               + ", "
               + CreateDataPtr(sourceStart)
               + ")";
        }

        private static string CreateMoveBytesFromFrame(Size size, Size destStart, Size sourceStart)
        {
            if (size.IsZero)
                return "";

            if (IsBuildInIntType(size))
                return CreateDataRef(destStart, size)
                   + " = "
                   + CreateFrameBackRef(sourceStart, size)
                ;
            return "Data.MoveBytes("
               + size.ByteCount
               + ", "
               + CreateDataPtr(destStart)
               + ", "
               + CreateFrameBackPtr(sourceStart)
               + ")";
        }

        private static string CreateMoveBytesToFrame(Size size, Size destStart, Size sourceStart)
        {
            if (size.IsZero)
                return "";

            if (IsBuildInIntType(size))
                return CreateFrameBackRef(destStart, size)
                   + " = "
                   + CreateDataRef(sourceStart, size)
                ;
            return "Data.MoveBytes("
               + size.ByteCount
               + ", "
               + CreateFrameBackPtr(destStart)
               + ", "
               + CreateDataPtr(sourceStart)
               + ")";
        }
        
        private static bool IsBuildInIntType(Size size)
        {
            int bits = size.ByteCount * 8;
            switch (bits)
            {
                case 8:
                case 16:
                case 32:
                case 64:
                    return true;
            }
            return false;
        }

        private string GetStatements(List<LeafElement> data)
        {
            string statements = "";
            for (int i = 0; i < data.Count; i++)
            {
                statements += data[i].Statements(this);
                ShiftStartAddress(data[i].DeltaSize);
            }
            return statements;
        }
    }
}