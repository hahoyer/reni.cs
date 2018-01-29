using hw.DebugFormatter;
using hw.Helper;
using Stx.CodeItems;

namespace Stx.DataTypes
{
    abstract class DataType
    {
        public static readonly DataType Integer = new Integer();
        readonly FunctionCache<int, DataType> ArrayCache;
        DataType DereferenceCache;

        DataType ReferenceCache;

        public DataType() => ArrayCache = new FunctionCache<int, DataType>(count => new Array(this, count));

        public abstract int ByteSize {get;}

        [DisableDump]
        public DataType Reference => ReferenceCache ?? (ReferenceCache = new Reference(this));

        public DataType Dereference => DereferenceCache ?? (DereferenceCache = GetDereference());
        protected virtual DataType GetDereference() => this;

        public DataType Array(int elementCount) => ArrayCache[elementCount];
    }

    sealed class Reference : DataType
    {
        readonly DataType DataType;
        public Reference(DataType dataType) => DataType = dataType;

        public override int ByteSize => CodeItem.PointerByteSize;
        protected override DataType GetDereference() => DataType;
    }

    sealed class Array : DataType
    {
        readonly int ElementCount;
        readonly DataType ElementType;

        public Array(DataType elementType, int elementCount)
        {
            ElementType = elementType;
            ElementCount = elementCount;
        }

        public override int ByteSize => ElementCount * ElementType.ByteSize;
    }

    sealed class Integer : DataType
    {
        public override int ByteSize => 4;
    }
}