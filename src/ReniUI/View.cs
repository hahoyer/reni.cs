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

namespace ReniUI
{
    public abstract class View : DumpableObject
    {
        protected readonly Form Frame;
        [UsedImplicitly]
        PositionConfig PositionConfig;

        protected View(string name, string configFileName = null)
        {
            Frame = new Form
            {
                Name = name,
                Text = name
            };

            Frame.Closing += OnClosing;

            if(configFileName == null)
                configFileName = GetFileName();
            configFileName.FileHandle().AssumeDirectoryOfFileExists();

            PositionConfig = new PositionConfig(()=> configFileName)
            {
                Target = Frame
            };
        }

        string GetFileName() => Frame.Text.Select(ToValidFileChar).Aggregate("", (c, n) => c + n);

        static string ToValidFileChar(char c)
        {
            if(Path.GetInvalidFileNameChars().Contains(c))
                return "%" + (int) c;
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