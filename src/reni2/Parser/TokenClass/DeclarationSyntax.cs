using Reni.Struct;

namespace Reni.Parser.TokenClass
{
    internal sealed class DeclarationSyntax : ParsedSyntax
    {
        internal readonly DefineableToken Name;
        internal readonly IParsedSyntax Definition;

        internal DeclarationSyntax(DefineableToken name, Token token, IParsedSyntax definition) : base(token)
        {
            Name = name;
            Definition = definition;
            StopByObjectId(-876);
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal protected override string DumpShort()
        {
            return Name.Name + ": " + Definition.DumpShort();
        }

        /// <summary>
        /// What to when syntax element is surrounded by parenthesis?
        /// </summary>
        /// <returns></returns>
        /// created 19.07.2007 23:20 on HAHOYER-DELL by hh
        internal protected override IParsedSyntax SurroundedByParenthesis(Token token)
        {
            return Container.Create(token, this);
        }
    }
}