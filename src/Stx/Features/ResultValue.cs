using hw.DebugFormatter;
using Stx.CodeItems;
using Stx.DataTypes;

namespace Stx.Features
{
    sealed class ResultValue : DumpableObject
    {
        [EnableDumpExcept(null)]
        public CodeItem[] CodeItems;

        [EnableDumpExcept(null)]
        public DataType DataType;

        [DisableDump]
        public Feature CompleteFeature
        {
            get
            {
                var result = Feature.None;
                if(DataType != null)
                    result += Feature.DataType;
                if(CodeItems != null)
                    result += Feature.CodeItems;

                return result;
            }
        }
    }
}