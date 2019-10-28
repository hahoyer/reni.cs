using Reni.Parser;

namespace Reni.TokenClasses {
    interface IValueProvider
    {
        Result<Value> Get(Syntax syntax);
    }
}