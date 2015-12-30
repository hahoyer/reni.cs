using hw.Scanner;


namespace ReniTest.CompilationView
{
    class TraceLogView : ChildView
    {
        public TraceLogView(SourceView master)
            : base(master, "TraceLogView")
        {
            master.RunCode();
            Client = master.CreateTraceLogView();
        }

        protected override SourcePart Source => null;
    }
}