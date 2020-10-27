using System.Linq;
using System.Windows.Forms;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.Type;
using ScintillaNET;

namespace ReniUI.CompilationView
{
    public sealed class SourceView : MainView, Extension.IClickHandler
    {
        int _lineNumberMarginLength;
        readonly FunctionCache<object, ChildView> ChildViews;
        readonly ValueCache<CompilerBrowser> CompilerCache;
        readonly TraceLogView LogView;
        readonly FunctionCache<ValueSyntax, ResultCachesView> ResultCachesViews;
        readonly Scintilla TextBox;
        readonly BrowseTraceCollector TraceLog;

        public SourceView(string text)
            : base("SourceView")
        {
            TextBox = new Scintilla
            {
                Lexer = Lexer.Container, VirtualSpaceOptions = VirtualSpace.UserAccessible
            };

            foreach(var id in TextStyle.All)
                StyleConfig(id);

            TextBox.StyleNeeded += (s, args) => SignalStyleNeeded(args.Position);
            TextBox.TextChanged += (s, args) => OnTextChanged();

            TextBox.ContextMenu = new ContextMenu();
            TextBox.ContextMenu.Popup += (s, args) => OnContextMenuPopup();

            CompilerCache = new ValueCache<CompilerBrowser>(CreateCompilerBrowser);

            ResultCachesViews = new FunctionCache<ValueSyntax, ResultCachesView>
                (item => new ResultCachesView(item, this));
            ChildViews = new FunctionCache<object, ChildView>(CreateView);

            Client = TextBox;

            TextBox.Text = text;

            TraceLog = new BrowseTraceCollector(this);
            LogView = new TraceLogView(this);
        }

        internal CompilerBrowser Compiler => CompilerCache.Value;

        int LinenumberMarginLength
        {
            get => _lineNumberMarginLength;
            set
            {
                if(_lineNumberMarginLength == value)
                    return;

                _lineNumberMarginLength = value;
                const int Padding = 2;
                TextBox.Margins[0].Width
                    = TextBox.TextWidth(Style.LineNumber, new string('9', value + 1)) + Padding;
            }
        }

        void Extension.IClickHandler.Signal(object target) => SignalClickedObject(target);

        CompilerBrowser CreateCompilerBrowser()
            => CompilerBrowser.FromText
            (
                TextBox.Text,
                new CompilerParameters
                {
                    OutStream = new StringStream()
                }
            );

        public new void Run()
        {
            LogView.Run();
            base.Run();
        }

        FunctionView CreateView(FunctionType target)
            => target == null? null : new FunctionView(target, this);

        ContextView CreateView(ContextBase target)
            => target == null? null : new ContextView(target, this);

        TypeView CreateView(TypeBase target)
            => target == null? null : new TypeView(target, this);

        CodeView CreateView(CodeBase target)
            => target == null? null : new CodeView(target, this);

        CompoundView CreateView(Compound target)
            => target == null? null : new CompoundView(target, this);

        StepView CreateView(Step target)
            => target == null? null : new StepView(target, this);

        ChildView CreateView(object target)
            => CreateView(target as FunctionType) ??
               CreateView(target as ContextBase) ??
               CreateView(target as TypeBase) ??
               CreateView(target as CodeBase) ??
               CreateView(target as Step) ?? CreateView(target as Compound) ?? UnexpectedTarget(target);

        ChildView UnexpectedTarget(object item)
        {
            NotImplementedMethod(item);
            return null;
        }

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

        MenuItem CreateMenuItem(ValueSyntax syntax)
        {
            var text = syntax.GetType().PrettyName() + " " + syntax.ObjectId;

            if(syntax.ResultCache.Count > 1)
                text += " (" + syntax.ResultCache.Count + ")";

            var menuItem = new MenuItem
                (text, (s, a) => ResultCachesViews[syntax].Run());
            menuItem.Select += (s, a) => SignalContextMenuSelect(syntax);
            return menuItem;
        }

        void SignalContextMenuSelect(ValueSyntax syntax)
            => TextBox.SetSelection(syntax.Anchor.SourcePart.Position, syntax.Anchor.SourcePart.EndPosition);

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

        void SignalClickedObject(object target) => ChildViews[target].Run();

        internal void SignalClickedFunction(int index)
            => SignalClickedObject(Compiler.Function(index));

        internal void SelectSource(SourcePart source)
        {
            if(source == null)
                return;
            TextBox.SetSelection(source.Position, source.EndPosition);
        }

        internal ITraceLogItem TraceLogItem(IFormalCodeItem target)
            => TraceLog.GetItems(target);

        internal void RunCode()
        {
            Compiler.Ensure();
            var parent = Compiler.ExecutionContext;
            var dataStack = new DataStack(parent)
            {
                TraceCollector = TraceLog
            };

            Compiler.Execute(dataStack);
        }

        internal DataGridView CreateTraceLogView() => TraceLog.CreateView();

        internal void SignalClickedSteps(Step[] target)
            => LogView.SignalClickedObject(target);

        internal void SignalClickedStep(Step target)
            => SignalClickedObject(target);
    }

    interface ITraceLogItem
    {
        Control CreateLink();
    }
}