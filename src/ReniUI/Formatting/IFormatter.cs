using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    public interface IFormatter
    {
        string Reformat(SourceSyntax target, SourcePart part);
    }

    public static class FormatterExtension
    {
        public static IFormatter Create(this Configuration configuration )
            => new HierachicalFormatter(configuration ?? new Configuration());

        public static string Reformat(this SourceSyntax syntax, SourcePart sourcePart, IFormatter formatter = null)
            =>
                (formatter ?? new Formatting.Configuration().Create()).Reformat
                    (syntax, sourcePart);
    }
}