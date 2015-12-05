using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Parser;
using Reni.Struct;
using ScintillaNET;

namespace ReniTest.CompilationView
{
    sealed class SourceView : MainView
    {
        int _lineNumberMarginLength;
        readonly Scintilla TextBox;
        readonly ValueCache<CompilerBrowser> CompilerCache;
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

            TextBox.StyleNeeded += (s, args) => SignalStyleNeeded(args.Position);
            TextBox.TextChanged += (s, args) => OnTextChanged();

            TextBox.ContextMenu = new ContextMenu();
            TextBox.ContextMenu.Popup += (s, args) => OnContextMenuPopup();

            CompilerCache = new ValueCache<CompilerBrowser>
                (() => Reni.Compiler.BrowserFromText(TextBox.Text));
            DetailViews = new FunctionCache<CompileSyntax, ResultCachesView>(CreateDetailView);

            Client = TextBox;

            TextBox.Text = text;
        }

        ResultCachesView CreateDetailView(CompileSyntax syntax)
            => new ResultCachesView(syntax, this);

        CompilerBrowser Compiler => CompilerCache.Value;

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

            var compileSyntaxs = Compiler.FindPosition(p);
            var items = compileSyntaxs
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
            menuItem.Select += (s, a) => SignalContextMenuSelect(syntax);
            return menuItem;
        }

        void SignalContextMenuSelect(Syntax syntax)
            => TextBox.SetSelection(syntax.SourcePart.Position, syntax.SourcePart.EndPosition);

        void StyleConfig(TextStyle id) => id.Config(TextBox.Styles[id]);

        void SignalStyleNeeded(int position)
        {
            var trace = false;
            StartMethodDump(trace, position);
            try
            {
                while(TextBox.GetEndStyled() < position)
                {
                    var current = TextBox.GetEndStyled();
                    var tokens = Compiler.LocatePosition(current);
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

        protected override void HandleClicked(FunctionId functionId)
        {
            var function = Compiler.Find(functionId);
            base.HandleClicked(functionId);
        }
    }
}