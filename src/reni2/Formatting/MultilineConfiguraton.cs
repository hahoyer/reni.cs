using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class MultilineConfiguraton : DumpableObject, IConfiguration
    {
        readonly IConfiguration _parent;
        public MultilineConfiguraton(IConfiguration parent)
        {
            _parent = parent;
            Tracer.Assert(!_parent.IsMultiline);
        }

        string IConfiguration.Parenthesis
            (
            IEnumerable<WhiteSpaceToken> leftHead,
            string left,
            IEnumerable<WhiteSpaceToken> leftTail,
            string content,
            IEnumerable<WhiteSpaceToken> rightHead,
            string right,
            IEnumerable<WhiteSpaceToken> rightTail)
            => _parent.Parenthesis(leftHead, left, leftTail, content, rightHead, right, rightTail);

        string IConfiguration.DeclarationItem
            (SourceSyntax item, IEnumerable<WhiteSpaceToken> tail)
            => _parent.DeclarationItem(item, tail);

        string IConfiguration.Default
            (
            string left,
            IEnumerable<WhiteSpaceToken> head,
            string token,
            IEnumerable<WhiteSpaceToken> tail,
            string right)
            => _parent.Default(left, head, token, tail, right);

        string IConfiguration.Exclamation(IToken token, string right)
            => _parent.Exclamation(token, right);

        bool IConfiguration.IsMultiline => true;
        IConfiguration IConfiguration.SingleLine => _parent.SingleLine;
        int? IConfiguration.MaxListItemLength(List list) => _parent.MaxListItemLength(list);
        int? IConfiguration.MaxListLength(List list) => _parent.MaxListLength(list);
    }
}