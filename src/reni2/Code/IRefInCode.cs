using Reni.Context;

namespace Reni.Code
{
    internal interface IRefInCode
    {
        RefAlignParam RefAlignParam { get; }
        bool IsChildOf(ContextBase contextBase);
        string Dump();
    }
}