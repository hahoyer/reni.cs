using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AutocompleteMenuNS;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.TokenClasses;
using Reni.Validation;
using ReniUI.Classification;
using ReniUI.Commands;
using ReniUI.CompilationView;
using ReniUI.Formatting;
using ScintillaNET;

namespace ReniUI
{
    public sealed class EditorView : ChildView, IssuesView.IDataProvider
    {
        const string ConfigRoot = "StudioConfig";
        static readonly TimeSpan DelayForSave = TimeSpan.FromSeconds(1);
        static readonly TimeSpan NoPeriodicalActivation = TimeSpan.FromMilliseconds(-1);

        sealed class SaveManager
        {
            readonly EditorView Parent;
            readonly System.Threading.Timer Timer;

            public SaveManager(EditorView parent)
            {
                Parent = parent;
                Timer = new System.Threading.Timer(Execute);
            }

            void Execute(object state) => InvokeAsynchron(Parent.SaveFile);

            void InvokeAsynchron(Action action) => Parent.Frame.InvokeAsynchron(action);

            public void Start() => Timer.Change(DelayForSave, NoPeriodicalActivation);
        }

        internal sealed class ConfigurationClass : DumpableObject
        {
            readonly Persister FilePersister;

            internal ConfigurationClass(EditorView parent)
            {
                FilePersister = new Persister(parent.ConfigFile);

                FilePersister.Register
                    (
                        "Selection",
                        item => parent.TextBox.SetSelection(item.Item1, item.Item2),
                        () =>
                            new Tuple<int, int>
                                (parent.TextBox.SelectionStart, parent.TextBox.SelectionEnd))
                    ;
                FilePersister.Load();
            }

            internal void OnUpdate(UpdateChange change)
            {
                switch(change)
                {
                case UpdateChange.Selection:
                    FilePersister.Store("Selection");
                    return;
                case UpdateChange.Content:
                case UpdateChange.VScroll:
                case UpdateChange.HScroll:
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
                }
            }
        }

        int _lineNumberMarginLength;
        readonly Scintilla TextBox;
        readonly ValueCache<CompilerBrowser> CompilerCache;
        readonly string FileName;
        internal readonly IStudioApplication Master;
        SaveManager _saveManager;
        readonly IssuesView IssuesView;
        readonly AutocompleteMenu AutocompleteMenu;
        readonly ConfigurationClass Configuration;

        public EditorView(string fileName, IStudioApplication master)
            : base(master, Path.Combine(ConfigRoot, "EditorFiles", fileName, "Position"))
        {
            FileName = fileName;
            Master = master;
            TextBox = new Scintilla
            {
                Lexer = Lexer.Container,
                VirtualSpaceOptions = VirtualSpace.UserAccessible
            };

            TextBox.ClearCmdKey(Keys.Insert);

            foreach(var id in TextStyle.All)
                StyleConfig(id);

            TextBox.StyleNeeded += (s, args) => SignalStyleNeeded(args.Position);
            TextBox.TextChanged += (s, args) => OnTextChanged();
            TextBox.KeyDown += (s, args) => OnKeyDown(args);

            CompilerCache = new ValueCache<CompilerBrowser>(CreateCompilerBrowser);

            Client = TextBox;

            TextBox.Text = FileName.FileHandle().String;
            Configuration = new ConfigurationClass(this);
            TextBox.UpdateUI += (s, args) => Configuration.OnUpdate(args.Change);

            TextBox.SetSavePoint();
            AlignTitle();
            TextBox.TextChanged += (s, args) => RunSaveManager();

            var result = new MainMenu();
            result.MenuItems.AddRange(this.Menus().Select(item => item.CreateMenuItem()).ToArray());
            Frame.Menu = result;

            IssuesView = new IssuesView(this);

            AutocompleteMenu = new AutocompleteMenu
            {
                AutoPopup = true,
                AppearInterval = 0,
                MinFragmentLength = 1
            };


            AutocompleteMenu.WrapperNeeded +=
                (s, a) => a.Wrapper = new ScintillaWrapper((Scintilla) a.TargetControl);
            AutocompleteMenu.SetAutocompleteItems(Extension.Query(GetOptions));
        }

        hw.Helper.File ConfigFile
            => Path.Combine(ConfigRoot, "EditorFiles", FileName, "EditorConfiguration").FileHandle()
            ;

        IEnumerable<AutocompleteItem> GetOptions() 
            => Compiler
            .DeclarationOptions(TextBox.SelectionStart - 1)
            .Select(item => new AutocompleteItem(item));

        void OnKeyDown(KeyEventArgs e)
        {
            if(e.Control && e.KeyCode == Keys.Space)
                AutocompleteMenu.Show(TextBox, true);
        }

        void RunSaveManager()
        {
            AlignTitle();
            if(_saveManager == null)
                _saveManager = new SaveManager(this);
            _saveManager.Start();
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
                        OutStream = new StringStream(),
                        ProcessErrors = true
                    },
                    sourceIdentifier: FileName);

        public new void Run() => base.Run();

        CompilerBrowser Compiler => CompilerCache.Value;

        internal void Open() => NotImplementedMethod();

        internal void FormatAll() => Format(Compiler.Source.All);

        internal void FormatSelection() => Format(SourcePart);

        SourcePart SourcePart
            =>
                (Compiler.Source + TextBox.SelectionStart).Span
                    (TextBox.SelectionEnd - TextBox.SelectionEnd);

        internal bool HasSelection() => TextBox.SelectedText != "";

        internal void Issues() => IssuesView.Run();

        void OnTextChanged()
        {
            LinenumberMarginLength = TextBox.Lines.Count.ToString().Length;
            CompilerCache.IsValid = false;
        }

        void AlignTitle() { Title = FileName + (TextBox.Modified ? "*" : ""); }

        void StyleConfig(TextStyle id) => id.Config(TextBox.Styles[id]);

        void SignalStyleNeeded(int position)
        {
            var sourceSyntax = Compiler.Syntax;
            while(TextBox.GetEndStyled() < position)
            {
                var current = TextBox.GetEndStyled();
                var token = Token.LocatePosition(sourceSyntax, current);
                var style = TextStyle.From(token, Compiler);
                TextBox.StartStyling(token.StartPosition);
                TextBox.SetStyling(token.SourcePart.Length, style);
                sourceSyntax = token.Syntax;
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
                TextBox.Margins[0].Width = TextBox.TextWidth
                    (Style.LineNumber, new string('9', value + 1)) + Padding;
            }
        }

        void Format(SourcePart sourcePart)
        {
            var reformat = Compiler.Locate(sourcePart)
                .GetEditPieces
                (
                    sourcePart,
                    new Formatting.Configuration
                    {
                        EmptyLineLimit = 1,
                        MaxLineLength = 120
                    }.Create()).Reverse();

            foreach(var piece in reformat)
            {
                TextBox.TargetStart = piece.Position;
                TextBox.TargetEnd = piece.Position + piece.RemoveCount;
                TextBox.ReplaceTarget(piece.NewText);
            }
        }

        IEnumerable<Issue> IssuesView.IDataProvider.Data => Compiler.Issues;

        IApplication IssuesView.IDataProvider.Master => Master;

        void IssuesView.IDataProvider.SignalClicked(SourcePart part)
        {
            TextBox.SetSelection(part.Position, part.EndPosition);
            TextBox.FirstVisibleLine = part.Source.LineIndex(part.Position);
            TextBox.FirstVisibleLine = part.Source.LineIndex(part.EndPosition);
        }

        internal void ListMembers() => TextBox.AutoCComplete();
    }
}