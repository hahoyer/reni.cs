using hw.DebugFormatter;
using hw.Helper;

namespace Bnf.StructuredText
{
    class Context : DumpableObject
    {
        public static readonly Context Root = new RootContext();

        public Context WithVariable(string name, DataType dataType) => new ContextWithVariable(this, name, dataType);
    }

    sealed class ContextWithVariable : Context
    {
        readonly DataType DataType;
        readonly string Name;
        readonly Context Parent;

        public ContextWithVariable(Context parent, string name, DataType dataType)
        {
            Parent = parent;
            Name = name;
            DataType = dataType;
        }
    }

    sealed class RootContext : Context {}

    class DataType
    {
        public static readonly DataType Integer = new DataType();
        readonly FunctionCache<int, DataType> ArrayCache;
        public DataType() {ArrayCache = new FunctionCache<int, DataType>(i => new ArrayType(i));}
        public DataType Array(int count) => ArrayCache[count];
    }

    sealed class ArrayType : DataType
    {
        readonly int Count;

        public ArrayType(int count) => Count = count;
    }
}