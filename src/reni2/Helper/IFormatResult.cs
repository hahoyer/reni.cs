namespace Reni.Helper
{
    interface IFormatResult<TValue>
    {
        TValue Value {get; set;}

        TContainer Concat<TContainer>(string token, TContainer other)
            where TContainer : class, IFormatResult<TValue>, new();
    }
}