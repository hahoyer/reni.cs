using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Parser;
using ScintillaNET;

namespace ReniTest.CompilationView
{
    sealed class SourceView : MainView
    {
        int _lineNumberMarginLength;
        readonly Scintilla TextBox;
        readonly ValueCache<Compiler> CompilerCache;
        readonly FunctionCache<CompileSyntax, ResultCachesView> DetailViews;

        internal SourceView(string text)
            : base("SourceView")
        {
            TextBox = new Scintilla
            {
                Lexer = ScintillaNET.Lexer.Container,
                VirtualSpaceOptions = VirtualSpace.UserAccessible
            };


            foreach(var id in TextStyle.All)
                StyleConfig(id);

            TextBox.StyleNeeded += (s, args) => OnStyleNeeded(args.Position);
            TextBox.TextChanged += (s, args) => OnTextChanged();

            TextBox.ContextMenu = new ContextMenu();
            TextBox.ContextMenu.Popup += (s, args) => OnContextMenuPopup();

            CompilerCache = new ValueCache<Compiler>(GetCompiler);
            DetailViews = new FunctionCache<CompileSyntax, ResultCachesView>(CreateDetailView);

            Client = TextBox;

            TextBox.Text = text;
        }

        ResultCachesView CreateDetailView(CompileSyntax syntax)
            => new ResultCachesView(syntax, this);

        Compiler Compiler => CompilerCache.Value;

        void OnTextChanged()
        {
            LinenumberMarginLength = TextBox.Lines.Count.ToString().Length;
            CompilerCache.IsValid = false;
        }

        void OnContextMenuPopup()
        {
            var menuItems = TextBox.ContextMenu.MenuItems;

            while(menuItems.Count > 0)
                menuItems.RemoveAt(0);

            var p = TextBox.CurrentPosition;
            var c = Compiler.CodeContainer.Issues.ToArray();

            var enumerable = Compiler
                .Locate(p)
                .SourceSyntax
                .ParentChainIncludingThis
                .Select(item => item.Syntax)
                .ToArray();

            var items = enumerable
                .OfType<CompileSyntax>()
                .Where(item => item.ResultCache.Any())
                .Select(CreateMenuItem)
                .ToArray();

            menuItems.AddRange(items);
        }

        MenuItem CreateMenuItem(CompileSyntax syntax)
        {
            var text = syntax.GetType().PrettyName() + " " + syntax.ObjectId;

            if(syntax.ResultCache.Count > 1)
                text += " (" + syntax.ResultCache.Count + ")";

            var menuItem = new MenuItem
                (text, (s, a) => DetailViews[syntax].Run());
            menuItem.Select += (s, a) => OnContextMenuSelect(syntax);
            return menuItem;
        }

        void OnContextMenuSelect(Syntax syntax)
            => TextBox.SetSelection(syntax.SourcePart.Position, syntax.SourcePart.EndPosition);

        Compiler GetCompiler() => new Compiler(text: TextBox.Text);

        void StyleConfig(TextStyle id) => id.Config(TextBox.Styles[id]);

        void OnStyleNeeded(int position)
        {
            var trace = false;
            StartMethodDump(trace, position);
            try
            {
                while(TextBox.GetEndStyled() < position)
                {
                    var current = TextBox.GetEndStyled();
                    var tokens = Compiler.Locate(current);
                    var style = TextStyle.From(tokens, Compiler);
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

        void Bold(SourcePart region) { }
    }
}