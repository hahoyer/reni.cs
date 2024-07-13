using Reni.Feature;
using Reni.Type;

namespace Reni.SyntaxFactory
{
    interface IDeclarationTag;
    interface IValueAnnotation
    {
        IImplementation GetFeature(TypeBase type);
    }
}