using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class Holder : ReniObject
    {
        private static readonly ParserInst _parser = new ParserInst(new Scanner(), Main.TokenFactory);
        private readonly string _text;
        private readonly ClauseSyntax _statement;

        public Holder(string text)
        {
            var file = File.m("main.proof");
            file.String = text;
            _text = text;
            var parsedSyntax = _parser.Compile(new Source(file));
            _statement = (ClauseSyntax) parsedSyntax;
        }

        internal Set<string> Variables { get { return _statement.Variables; } }
        internal ClauseSyntax Statement { get { return _statement; } }
    }
}