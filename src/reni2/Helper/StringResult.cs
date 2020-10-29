using hw.DebugFormatter;

namespace Reni.Helper
{
    sealed class StringResult : DumpableObject, IFormatResult<string>
    {
        internal string Value;
        string IFormatResult<string>.Value {get => Value; set => Value = value;}

        TContainer IFormatResult<string>.Concat<TContainer>(string token, TContainer other)
            => new TContainer {Value = Value + token + other.Value};
    }
}