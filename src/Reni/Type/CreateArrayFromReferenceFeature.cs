using hw.DebugFormatter;

namespace Reni.Type
{
    sealed class CreateArrayFromReferenceFeature : DumpableObject
    {
        [EnableDump]
        readonly TypeBase Type;

        public CreateArrayFromReferenceFeature(TypeBase type) => Type = type;
    }
}