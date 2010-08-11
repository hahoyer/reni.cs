using Reni.Context;

namespace Reni.Code
{
    internal interface IReferenceInCode
    {
        RefAlignParam RefAlignParam { get; }
        bool IsChildOf(ContextBase contextBase);
        string Dump();
    }
}