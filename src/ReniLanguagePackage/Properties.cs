using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.Shell;

namespace HoyerWare.ReniLanguagePackage
{
    public sealed class Properties : DialogPage
    {
        public Properties() { MinImprovementOfLineBreak = 3; }

        [Category("Formatting")]
        [Description(
            "If set, tries to constraint line length until this value by inserting"
                + " line breaks at suitable positions.")]
        public int? MaxLineLength { get; set; }

        [Category("Formatting")]
        [Description(
            "If set, delimits the number of consecutive line breaks to this value.")]
        public int? EmptyLineLimit { get; set; }

        [Category("Formatting")]
        [Description(
            "When trying to insert a line break, the improvment should be at least this value.")]
        public int MinImprovementOfLineBreak { get; set; }
    }
}