namespace Reni.Helper
{
    sealed class IntegerResult : DumpableObject, IFormatResult<int>
    {
        internal int Value;
        int IFormatResult<int>.Value {get => Value; set => Value = value;}

        TContainer IFormatResult<int>.Concat<TContainer>(string token, TContainer other)
            => new TContainer {Value = Value + token.Length + other.Value};
    }
}