﻿using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Shell;

namespace ReniVSIX
{
    public sealed class Properties : DialogPage
    {
        [Category("Formatting")]
        [Description(
            "If set, tries to constraint line length until this value by inserting"
            + " line breaks at suitable positions.")]
        [UsedImplicitly]
        public int? MaxLineLength { get; set; }

        [Category("Formatting")]
        [Description(
            "If set, delimits the number of consecutive line breaks to this value.")]
        [UsedImplicitly]
        public int? EmptyLineLimit { get; set; }
    }
}