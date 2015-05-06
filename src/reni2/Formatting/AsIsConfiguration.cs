using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;

namespace Reni.Formatting
{
    sealed class AsIsConfiguration : DumpableObject, IGapHandler, IGapHandlerWithWhiteSpaces
    {
        string IGapHandlerWithWhiteSpaces.StartGap
            (IEnumerable<WhiteSpaceToken> whiteSpaces, ITokenClass right)
            => whiteSpaces.SourcePart().Id;

        string IGapHandler.StartGap(ITokenClass right) => "";

        string IGapHandler.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.Get(left, right).Text;

        string IGapHandlerWithWhiteSpaces.Gap
            (
            ITokenClass left,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right
            )
            => rightWhiteSpaces.SourcePart().Id;
    }

    sealed class SmartConfiguration : DumpableObject, IGapHandler
    {
        internal static readonly IGapHandler Instance = new SmartConfiguration();

        SmartConfiguration() { }

        string IGapHandler.StartGap(ITokenClass right) => "";

        string IGapHandler.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.Get(left, right).Text;
    }

    sealed class IgnoreWhiteSpaceConfiguration
        : DumpableObject, IGapHandlerWithWhiteSpaces
    {
        readonly IGapHandler _parent;

        internal IgnoreWhiteSpaceConfiguration(IGapHandler parent) { _parent = parent; }

        string IGapHandlerWithWhiteSpaces.StartGap
            (IEnumerable<WhiteSpaceToken> whiteSpaces, ITokenClass right) => _parent.StartGap(right);

        string IGapHandlerWithWhiteSpaces.Gap
            (
            ITokenClass left,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right
            )
            => _parent.Gap(left, right);
    }

    sealed class MinimalConfiguration : DumpableObject, IGapHandler
    {
        string IGapHandler.StartGap(ITokenClass right) => "";

        string IGapHandler.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.BaseSeparatorType(left, right).Text;
    }
}