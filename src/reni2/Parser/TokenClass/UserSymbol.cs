namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Any non reseved token
    /// </summary>
    internal sealed class UserSymbol: Defineable
    {
        private static readonly Base _symbolInstance = new UserSymbol(true);
        private static readonly Base _nonSymbolInstance = new UserSymbol(false);
        private readonly bool _isSymbol;

        private UserSymbol(bool isSymbol) {
            _isSymbol = isSymbol;
        }

        internal static Base Instance(bool isSymbol)
        {
            return isSymbol ? _symbolInstance : _nonSymbolInstance;
        }

        internal override bool IsSymbol { get { return _isSymbol; } }
    }
}
