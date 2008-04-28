namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Any non reseved token
    /// </summary>
    internal sealed class UserSymbol: Defineable
    {
        private readonly bool _isSymbol;
        private readonly string _name;

        private UserSymbol(bool isSymbol, string name)
        {
            _isSymbol = isSymbol;
            _name = name;
        }

        internal override bool IsSymbol { get { return _isSymbol; } }
        internal override string Name { get { return _name; } }

        public static Base Instance(bool isSymbol, string name)
        {
            return new UserSymbol(isSymbol, name);
        }
    }
}
