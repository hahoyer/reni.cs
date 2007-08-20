using System.Diagnostics;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Any non reseved token
    /// </summary>
    sealed public class UserSymbol: Defineable
    {
        string _potentialTypeName;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="typeName">Suggested type name to turn it into a reseved token</param>
        public UserSymbol(string typeName)
        {
            _potentialTypeName = typeName;
        }

        /// <summary>
        /// Name of the type the parser would map this token to.
        /// The namespace should be Reni.Parser.TokenClass.Symbol in case of symbols 
        /// and Reni.Parser.TokenClass.Name in case of alphanumeric names
        /// </summary>
        public string PotentialTypeName{get{return _potentialTypeName;}}

    }
}
