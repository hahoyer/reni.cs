using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;

namespace Reni.Formatting
{
    sealed class Configuration : DumpableObject, SmartFormat.IConfiguration
    {
        string SmartFormat.IConfiguration.StartGap
            (WhiteSpaceToken[] whiteSpaces, ITokenClass right)
            => whiteSpaces.SourcePart().Id;

        string SmartFormat.IConfiguration.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.Get(left, right).Text;

        string SmartFormat.IConfiguration.Gap
            (
            ITokenClass left,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass right
            )
            => rightWhiteSpaces.SourcePart().Id;
    }

    sealed class SmartConfiguration : DumpableObject, SmartFormat.IConfiguration
    {
        string SmartFormat.IConfiguration.StartGap
            (WhiteSpaceToken[] whiteSpaces, ITokenClass right)
            => "";

        string SmartFormat.IConfiguration.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.Get(left, right).Text;

        string SmartFormat.IConfiguration.Gap
            (
            ITokenClass left,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass right
            )
            => SeparatorType.Get(left, right).Text;
    }

    sealed class MinimalConfiguration : DumpableObject, SmartFormat.IConfiguration
    {
        string SmartFormat.IConfiguration.StartGap
            (WhiteSpaceToken[] whiteSpaces, ITokenClass right)
            => "";

        string SmartFormat.IConfiguration.Gap(ITokenClass left, ITokenClass right)
            => SeparatorType.BaseSeparatorType(left, right).Text;

        string SmartFormat.IConfiguration.Gap
            (
            ITokenClass left,
            WhiteSpaceToken[] rightWhiteSpaces,
            ITokenClass right
            )
            => SeparatorType.BaseSeparatorType(left, right).Text;
    }
}