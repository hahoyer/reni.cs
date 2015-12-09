using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Type;
using ScintillaNET;

namespace ReniTest.CompilationView
{
    sealed class SourceView : MainView
    {
        int _lineNumberMarginLength;
        readonly Scintilla TextBox;
        readonly ValueCache<CompilerBrowser> CompilerCache;
        readonly FunctionCache<CompileSyntax, ResultCachesView> ResultCachesViews;
        readonly FunctionCache<object, ChildView> ChildViews;

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

            ResultCachesViews = new FunctionCache<CompileSyntax, ResultCachesView>
                (item => new ResultCachesView(item, this));
            ChildViews = new FunctionCache<object, ChildView>(CreateView);

            Client = TextBox;

            TextBox.Text = text;
        }

        FunctionView CreateView(FunctionType target)
            => target == null ? null : new FunctionView(target, this);

        ContextView CreateView(ContextBase target)
            => target == null ? null : new ContextView(target, this);

        TypeView CreateView(TypeBase target)
            => target == null ? null : new TypeView(target, this);

        ChildView CreateView(object target) => CreateView(target as FunctionType)
            ?? CreateView(target as ContextBase)
                ?? CreateView(target as TypeBase)
                    ?? UnexpectedTarget(target);

        ChildView UnexpectedTarget(object item)
        {
            NotImplementedMethod(item);
            return null;
        }

        internal CompilerBrowser Compiler => CompilerCache.Value;

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

            Compiler.Ensure();

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
                (text, (s, a) => ResultCachesViews[syntax].Run());
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

        void SignalClickedObject(object target) => ChildViews[target].Run();

        internal void SignalClicked(TypeBase target) => SignalClickedObject(target);
        internal void SignalClicked(ContextBase target) => SignalClickedObject(target);
        internal void SignalClicked(IContextReference target) => SignalClickedObject(target);
        internal void SignalClickedFunction(int index)
            => SignalClickedObject(Compiler.FindFunction(index));

        internal void SelectSource(SourcePart source)
        {
            if(source == null)
                return;
            TextBox.SetSelection(source.Position, source.EndPosition);
        }
    }
}