using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class Holder : ReniObject
    {
        private static readonly ParserInst _parser = new ParserInst(new Scanner(), Main.TokenFactory);
        private readonly string _text;
        private readonly AndSyntax _statement;

        public Holder(string text)
        {
            _text = text;
            var parsedSyntax = _parser.Compile(new Source(_text));
            _statement = (AndSyntax) parsedSyntax;
        }

        internal Set<string> Variables { get { return _statement.Variables; } }
        internal AndSyntax Statement { get { return _statement; } }
    }
}