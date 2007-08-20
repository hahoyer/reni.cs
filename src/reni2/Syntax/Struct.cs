using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Syntax
{
    internal sealed class Struct : Base
    {
        [DumpData(true)]
        private Reni.Struct _data;

        internal Struct(Reni.Struct data)
        {
            _data = data;
        }

        /// <summary>
        /// Dumps the short.
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 22:09 on HAHOYER-DELL by hh
        internal override string DumpShort()
        {
            return _data.Dump();
        }

        /// <summary>
        /// Visitor function, that ensures correct alignment
        /// This function shoud be called by cache elments only
        /// </summary>
        /// <param name="context">Environment used for deeper visit and alignment</param>
        /// <param name="category">Categories</param>
        /// <returns></returns>
        //[DebuggerHidden]
        public override Result VirtVisit(Context.Base context, Category category)
        {
            return _data.Visit(context, category);
        }

        internal override Base CreateListSyntaxReverse(Base left, Token token)
        {
            return new Struct(Reni.Struct.Create(left, _data));
        }

        internal override Base CreateListSyntaxReverse(DeclarationSyntax left, Token token)
        {
            return new Struct(Reni.Struct.Create(left, _data));
        }

        internal static Base Create(Base left)
        {
            return new Struct(Reni.Struct.Create(left));
        }

        internal static Base Create(Base left, Base right)
        {
            return new Struct(Reni.Struct.Create(left, right));
        }

        internal static Base Create(DeclarationSyntax left)
        {
            return new Struct(Reni.Struct.Create(left));
        }

        internal static Base Create(ConverterSyntax left)
        {
            return new Struct(Reni.Struct.Create(left));
        }

        internal static Base Create(DeclarationSyntax left, Base right)
        {
            return new Struct(Reni.Struct.Create(left, right));
        }
    }
}