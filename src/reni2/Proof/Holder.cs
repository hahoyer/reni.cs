using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class Holder
    {
        private static readonly ParserInst _parser = new ParserInst(new Scanner(), TokenFactory.Instance);
        private readonly string _text;
        private IParsedSyntax _syntax;

        public Holder(string text)
        {
            _text = text;
            _syntax = _parser.Compile(new Source(_text));
        }

        public void Replace(string target, string value)
        {
            throw new NotImplementedException();
        }
    }
}