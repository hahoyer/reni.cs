using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;

namespace Reni.Formatting
{
    sealed class Configuration : DumpableObject, SmartFormat.IGapHandler, SmartFormat.IGapHandlerWithWhiteSpaces
    {
        string SmartFormat.IGapHandlerWithWhiteSpaces.StartGap
            (IEnumerable<WhiteSpaceToken> whiteSpaces, ITokenClass right)
            => whiteSpaces.SourcePart().Id;

        string SmartFormat.IGapHandler.StartGap(ITokenClass right) => "";

        string SmartFormat.IGapHandler.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.Get(left, right).Text;

        string SmartFormat.IGapHandlerWithWhiteSpaces.Gap
            (
            ITokenClass left,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right
            )
            => rightWhiteSpaces.SourcePart().Id;
    }

    sealed class SmartConfiguration : DumpableObject, SmartFormat.IGapHandler
    {
        internal static readonly SmartFormat.IGapHandler Instance = new SmartConfiguration();

        SmartConfiguration() { }

        string SmartFormat.IGapHandler.StartGap(ITokenClass right) => "";

        string SmartFormat.IGapHandler.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.Get(left, right).Text;
    }

    sealed class IgnoreWhiteSpaceConfiguration
        : DumpableObject, SmartFormat.IGapHandlerWithWhiteSpaces
    {
        readonly SmartFormat.IGapHandler _parent;

        internal IgnoreWhiteSpaceConfiguration(SmartFormat.IGapHandler parent) { _parent = parent; }

        string SmartFormat.IGapHandlerWithWhiteSpaces.StartGap
            (IEnumerable<WhiteSpaceToken> whiteSpaces, ITokenClass right) => _parent.StartGap(right);

        string SmartFormat.IGapHandlerWithWhiteSpaces.Gap
            (
            ITokenClass left,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right
            )
            => _parent.Gap(left, right);
    }
                       
    sealed class MinimalConfiguration : DumpableObject, SmartFormat.IGapHandler
    {
        string SmartFormat.IGapHandler.StartGap(ITokenClass right) => "";

        string SmartFormat.IGapHandler.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.BaseSeparatorType(left, right).Text;
    }
}