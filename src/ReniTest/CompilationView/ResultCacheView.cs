using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using Reni;
using Reni.Context;
using Reni.Formatting;
using ScintillaNET;

namespace ReniTest.CompilationView
{
    sealed class ResultCacheView
    {
        readonly ContextBase Context;
        readonly Result Result;
        readonly Scintilla TextBox;

        public ResultCacheView(KeyValuePair<ContextBase, ResultCache> item)
        {
            Context = item.Key;
            Result = item.Value.Data;

            TextBox = new Scintilla
            {
                Dock = DockStyle.Fill,
                Lexer = ScintillaNET.Lexer.Container,
                VirtualSpaceOptions = VirtualSpace.UserAccessible
            };

            TextBox.Text = Context.Dump() + "\n---------------------------\n" + Result.Dump();
        }

        public Control Control=>TextBox;
    }
}