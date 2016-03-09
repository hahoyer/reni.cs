using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using Reni;
using Reni.Parser;
using ReniUI.Commands;
using ReniUI.CompilationView;
using ReniUI.Formatting;
using ScintillaNET;

namespace ReniUI
{
    public sealed class EditorView : ChildView
    {
        const string ConfigRoot = "StudioConfig";
        static readonly TimeSpan DelayForSave = TimeSpan.FromSeconds(1);
        static readonly TimeSpan NoPeriodicalActivation = TimeSpan.FromMilliseconds(-1);

        int _lineNumberMarginLength;
        readonly Scintilla TextBox;
        readonly ValueCache<CompilerBrowser> CompilerCache;
        readonly string FileName;
        internal readonly IStudioApplication Master;
        SaveManager _saveManager;

        public EditorView(string fileName, IStudioApplication master)
            : base(
                master,
                Path.Combine(ConfigRoot, "EditorFiles", fileName, "Editor", "Position")
                )
        {
            FileName = fileName;
            Master = master;
            TextBox = new Scintilla
            {
                Lexer = ScintillaNET.Lexer.Container,
                VirtualSpaceOptions = VirtualSpace.UserAccessible,
            };

            TextBox.ClearCmdKey(Keys.Insert);

            foreach(var id in TextStyle.All)
                StyleConfig(id);

            TextBox.StyleNeeded += (s, args) => SignalStyleNeeded(args.Position);
            TextBox.TextChanged += (s, args) => OnTextChanged();

            TextBox.ContextMenu = new ContextMenu();
            TextBox.ContextMenu.Popup += (s, args) => OnContextMenuPopup();

            CompilerCache = new ValueCache<CompilerBrowser>(CreateCompilerBrowser);

            Client = TextBox;

            TextBox.Text = FileName.FileHandle().String;
            TextBox.SetSavePoint();
            AlignTitle();
            TextBox.TextChanged += (s, args) => RunSaveManager();

            Frame.Menu = this.CreateMainMenu();
        }

        void RunSaveManager()
        {
            AlignTitle();
            if(_saveManager == null)
                _saveManager = new SaveManager(this);
            _saveManager.Start();
        }

        sealed class SaveManager
        {
            readonly EditorView Parent;
            readonly System.Threading.Timer Timer;

            public SaveManager(EditorView parent)
            {
                Parent = parent;
                Timer = new System.Threading.Timer(Execute);
            }

            void Execute(object state) => InvokeAsynchron(Parent.SaveFile) ;

            void InvokeAsynchron(Action action) => Parent.Frame.InvokeAsynchron(action);

            public void Start() => Timer.Change(DelayForSave, NoPeriodicalActivation);
        }

        void SaveFile()
        {
            _saveManager = null;
            FileName.FileHandle().String = TextBox.Text;
            TextBox.SetSavePoint();
            AlignTitle();
        }

        CompilerBrowser CreateCompilerBrowser()
            => CompilerBrowser.FromText
                (
                    TextBox.Text,
                    new CompilerParameters
                    {
                        OutStream = new StringStream()
                    }
                );

        public new void Run() => base.Run();

        internal CompilerBrowser Compiler => CompilerCache.Value;

        void OnTextChanged()
        {
            LinenumberMarginLength = TextBox.Lines.Count.ToString().Length;
            CompilerCache.IsValid = false;
        }

        void AlignTitle() { Title = FileName + (TextBox.Modified ? "*" : ""); }

        void OnContextMenuPopup()
        {
            var menuItems = TextBox.ContextMenu.MenuItems;

            while(menuItems.Count > 0)
                menuItems.RemoveAt(0);

            Compiler.Ensure();

            var p = TextBox.CurrentPosition;

            var compileSyntaxs = Compiler.FindPosition(p);
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

        public void Open() { NotImplementedMethod(); }

        public void FormatAll()
        {
            var sourcePart = Compiler.Source.All;
            var reformat = Compiler.Locate(sourcePart).GetEditPieces(sourcePart).ToArray();
            NotImplementedMethod();
        }

        public void FormatSelection()
        {
            NotImplementedMethod();

        }

        public bool HasSelection() => TextBox.SelectedText != "";
    }
}