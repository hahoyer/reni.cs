using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using JetBrains.Annotations;
using Reni;
using ScintillaNET;

namespace ReniTest.CompilationView
{
    sealed class View : DumpableObject
    {
        int _lineNumberMarginLength;
        readonly Form Frame;
        readonly Scintilla TextBox;
        [UsedImplicitly]
        readonly PositionConfig PositionConfig;
        readonly ValueCache<Compiler> CompilerCache;

        internal View(string text)
        {
            TextBox = new Scintilla
            {
                Dock = DockStyle.Fill,
                Lexer = Lexer.Container,
                VirtualSpaceOptions = VirtualSpace.UserAccessible
            };

            foreach(var id in TextStyle.All)
                StyleConfig(id);

            TextBox.StyleNeeded += (s, args) => OnStyleNeeded(args.Position);
            TextBox.TextChanged += (s, args) => OnTextChanged();

            TextBox.ContextMenu = new ContextMenu();
            TextBox.ContextMenu.Popup += (s, args) => OnContextMenuPopup();

            Frame = new Form
            {
                Name = "MainView"
            };

            CompilerCache = new ValueCache<Compiler>(GetCompiler);

            Frame.Controls.Add(TextBox);

            PositionConfig = new PositionConfig
            {
                Target = Frame
            };

            TextBox.Text = text;
        }

        Compiler Compiler => CompilerCache.Value;

        void OnTextChanged()
        {
            LinenumberMarginLength = TextBox.Lines.Count.ToString().Length;
            CompilerCache.IsValid = false;
        }

        void OnContextMenuPopup()
        {
            var p = TextBox.CurrentPosition;
            var t = Compiler.Locate(p);
            var c = Compiler.CodeContainer;
            NotImplementedMethod(nameof(t),t);
        }

        internal void Run() => Application.Run(Frame);

        Compiler GetCompiler() => new Compiler(text: TextBox.Text);

        void StyleConfig(TextStyle id) => id.Config(TextBox.Styles[id]);

        void OnStyleNeeded(int position)
        {
            var trace = true;
            StartMethodDump(trace, position);
            try
            {
                var compiler = Compiler;

                while(TextBox.GetEndStyled() < position)
                {
                    var current = TextBox.GetEndStyled();
                    var tokens = compiler.Locate(current);
                    var style = TextStyle.From(tokens, compiler);
                    TextBox.StartStyling(tokens.StartPosition);
                    TextBox.SetStyling(tokens.SourcePart.Length, style);
                }

                ReturnVoidMethodDump(false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        int LinenumberMarginLength
        {
            get { return _lineNumberMarginLength; }
            set
            {
                if(_lineNumberMarginLength == value)
                    return;

                _lineNumberMarginLength = value;
                const int Padding = 2;
                TextBox.Margins[0].Width
                    = TextBox.TextWidth(Style.LineNumber, new string('9', value + 1))
                        + Padding;
            }
        }
    }
}