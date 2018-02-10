using hw.DebugFormatter;
using hw.Helper;

namespace hw.Scanner
{
    sealed class ImbeddedTokenType : DumpableObject, IFactoryTokenType
    {
        readonly ITokenType Parent;
        public ImbeddedTokenType(ITokenType parent) => Parent = parent;

        string IUniqueIdProvider.Value => Parent.Value;
        ITokenType ITokenTypeFactory.Get(string id) => Parent;
    }

    sealed class ImbeddedTokenFactoryType : DumpableObject, IFactoryTokenType
    {
        readonly string Id;
        readonly ITokenTypeFactory Parent;

        public ImbeddedTokenFactoryType(ITokenTypeFactory parent, string id)
        {
            Parent = parent;
            Id = id;
        }

        string IUniqueIdProvider.Value => Id;
        ITokenType ITokenTypeFactory.Get(string id) => Parent.Get(id);
    }
}