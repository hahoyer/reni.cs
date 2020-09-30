using Reni.Feature;

namespace Reni
{
    interface IProxyType
    {
        IConversion Converter { get; }
    }
}