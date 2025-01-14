using Reni.Code;
using Reni.Feature;

namespace Reni.Type;

interface IReference : IContextReference
{
    IConversion Converter { get; }
    bool IsWeak { get; }
}