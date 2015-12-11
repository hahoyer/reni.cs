using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using JetBrains.Annotations;

namespace ReniTest.CompilationView
{
    abstract class View : DumpableObject
    {
        protected readonly Form Frame;
        [UsedImplicitly]
        PositionConfig PositionConfig;

        protected View(string name)
        {
            Frame = new Form
            {
                Name = name,
                Text = name
            };

            Frame.Closing += OnClosing;

            PositionConfig = new PositionConfig(GetFileName)
            {
                Target = Frame
            };
        }

        string GetFileName()
        {
            return Frame.Text.Select(ToValidFileChar).Aggregate("", (c, n) => c + n);
        }

        static string ToValidFileChar(char c)
        {
            if(Path.GetInvalidFileNameChars().Contains(c))
                return "%" + (int)c;
            return "" + c;
        }

        void OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Frame.Visible = false;
        }

        internal Control Client
        {
            get { return Frame.Controls.Cast<Control>().FirstOrDefault(); }
            set
            {
                Tracer.Assert(Frame.Controls.Count == 0);
                value.Dock = DockStyle.Fill;
                Frame.Controls.Add(value);
            }
        }
    }
}