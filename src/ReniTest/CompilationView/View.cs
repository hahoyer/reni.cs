using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Forms;
using JetBrains.Annotations;

namespace ReniTest.CompilationView
{
    class View : DumpableObject
    {
        static bool IsActive;
        readonly Form Frame;
        [UsedImplicitly]
        PositionConfig PositionConfig;

        internal View(string name)
        {
            Frame = new Form
            {
                Name = name,
                Text = name
            };


            PositionConfig = new PositionConfig
            {
                Target = Frame
            };
        }

        internal void Run()
        {
            if(IsActive)
                Frame.Show();
            else
            {
                IsActive = true;
                Application.Run(Frame);
            }
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

        public View Master { set { value.Frame.Closing += (a, s) => Frame.Close(); } }
    }
}