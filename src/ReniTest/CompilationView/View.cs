using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Forms;
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

            PositionConfig = new PositionConfig
            {
                Target = Frame
            };
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