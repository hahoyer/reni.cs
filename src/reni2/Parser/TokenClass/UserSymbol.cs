namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Any non reseved token
    /// </summary>
    internal sealed class UserSymbol: Defineable
    {
        private readonly string _name;

        private UserSymbol(string name)
        {
            _name = name;
        }

        internal override string Name { get { return _name; } }

        public static TokenClassBase Instance(string name)
        {
            return new UserSymbol(name);
        }
    }
}
