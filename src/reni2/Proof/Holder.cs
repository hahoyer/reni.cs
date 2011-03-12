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
        private static readonly ParserInst _parser = new ParserInst(new Scanner(), TokenFactory.Instance);
        private readonly string _text;
        private readonly ParsedSyntax _syntax;

        public Holder(string text)
        {
            _text = text;
            _syntax = (ParsedSyntax) _parser.Compile(new Source(_text));
        }

        internal Set<string> Variables
        {
            get { return _syntax.Variables; }
        }

        public void Replace(string target, string value)
        {
            var newClause = _parser.Compile(new Source("(" +target + ")=(" + value + ")" ));
            DumpDataWithBreak("","newClause",newClause);
        }
    }
}