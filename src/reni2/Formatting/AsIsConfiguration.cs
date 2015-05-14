using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.Parser;

namespace Reni.Formatting
{
    sealed class AsIsConfiguration : DumpableObject, IWhitespacelessGapHandler, IGapHandler
    {
        string IGapHandler.StartGap
            (
            bool level,
            int indentLevel,
            IEnumerable<WhiteSpaceToken> whiteSpaces,
            ITokenClass right)
            => whiteSpaces.SourcePart().Id;

        string IWhitespacelessGapHandler.StartGap(int indentLevel, ITokenClass right) => "";

        string IWhitespacelessGapHandler.Gap(int indentLevel, ITokenClass left, ITokenClass right)
            => SeparatorType.Get(left, right).Text;

        string IGapHandler.Gap
            (
            int indentLevel1,
            ITokenClass left,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right)
            => rightWhiteSpaces.SourcePart().Id;

        string IGapHandler.Indent(string tag, WhiteSpaceToken[] precededWith)
        {
            if(precededWith.HasComment())
                NotImplementedMethod(tag, precededWith.ToArray());
            return tag;
        }
    }

    sealed class SmartConfiguration : DumpableObject, IWhitespacelessGapHandler
    {
        internal static readonly IWhitespacelessGapHandler Instance = new SmartConfiguration();

        SmartConfiguration() { }

        string IWhitespacelessGapHandler.StartGap(int indentLevel, ITokenClass right) => "";

        string IWhitespacelessGapHandler.Gap(int indentLevel, ITokenClass left, ITokenClass right)
            => SeparatorType.Get(left, right).Text;
    }

    sealed class IgnoreWhiteSpaceConfiguration : DumpableObject, IGapHandler
    {
        readonly IWhitespacelessGapHandler _parent;

        internal IgnoreWhiteSpaceConfiguration(IWhitespacelessGapHandler parent)
        {
            _parent = parent;
        }

        string IGapHandler.StartGap
            (
            bool level,
            int indentLevel,
            IEnumerable<WhiteSpaceToken> whiteSpaces,
            ITokenClass right)
            => _parent.StartGap(indentLevel, right);

        string IGapHandler.Gap
            (
            int indentLevel,
            ITokenClass left,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right)
            => _parent.Gap(indentLevel, left, right);

        string IGapHandler.Indent(string tag, WhiteSpaceToken[] precededWith)
        {
            if(precededWith.HasComment())
                NotImplementedMethod(tag, precededWith.ToArray());
            return tag;
        }
    }

    sealed class KeepCommentConfiguration : DumpableObject, IGapHandler
    {
        readonly IWhitespacelessGapHandler _parent;

        internal KeepCommentConfiguration(IWhitespacelessGapHandler parent) { _parent = parent; }

        string IGapHandler.StartGap
            (
            bool topOfChain,
            int indentLevel,
            IEnumerable<WhiteSpaceToken> whiteSpaces,
            ITokenClass right)
        {
            NotImplementedMethod(topOfChain, indentLevel, whiteSpaces.ToArray(), right);

            var c = whiteSpaces.Trim().Id();
            return c == "" ? _parent.StartGap(indentLevel, right) : c;
        }

        string IGapHandler.Gap
            (
            int indentLevel,
            ITokenClass left,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right)
        {
            NotImplementedMethod(indentLevel, left, rightWhiteSpaces.ToArray(), right);
            var c = rightWhiteSpaces.Trim().Id();
            return c == "" ? _parent.Gap(indentLevel, left, right) : c;
        }

        string IGapHandler.Indent(string tag, WhiteSpaceToken[] precededWith)
        {
            NotImplementedMethod(tag, precededWith.ToArray());
            if (precededWith.HasComment())
                NotImplementedMethod(tag, precededWith.ToArray());
            return tag;
        }
    }

    sealed class MinimalConfiguration : DumpableObject, IWhitespacelessGapHandler
    {
        string IWhitespacelessGapHandler.StartGap(int indentLevel, ITokenClass right) => "";

        string IWhitespacelessGapHandler.Gap(int indentLevel, ITokenClass left, ITokenClass right)
            => SeparatorType.BaseSeparatorType(left, right).Text;
    }
}