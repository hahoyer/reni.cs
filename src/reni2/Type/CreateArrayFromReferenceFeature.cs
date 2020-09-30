using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Type
{
    sealed class CreateArrayFromReferenceFeature : DumpableObject
    {
        [EnableDump]
        readonly TypeBase _type;
        public CreateArrayFromReferenceFeature(TypeBase type) { _type = type; }
        Result Result(Category category) => _type.CreateArray(category);
    }
}