using System;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Any non reseved token
    /// </summary>
    [Serializable]
    internal sealed class UserSymbol : Defineable
    {
        private UserSymbol(string name)
        {
            Name = name;
        }

        public static TokenClassBase Instance(string name)
        {
            return new UserSymbol(name);
        }
    }
}
