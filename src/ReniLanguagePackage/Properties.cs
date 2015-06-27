using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Shell;

namespace HoyerWare.ReniLanguagePackage
{
    public sealed class Properties : DialogPage
    {
        [Category("Formatting")]
        [Description(
            "If set, tries to constraint line length until this value by inserting"
                + " line breaks at suitable positions.")]
        [UsedImplicitly]
        public int? MaxLineLength;

        [Category("Formatting")]
        [Description(
            "If set, delimits the number of consecutive line breaks to this value.")]
        [UsedImplicitly]
        public int? EmptyLineLimit;
    }
}