using System;
using System.Diagnostics;
using System.Threading;
using hw.Helper;

namespace hw.DebugFormatter
{
    sealed class Writer
    {
        sealed class WriteInitiator
        {
            string LastName = "";
            string Name = "";

            public bool ThreadChanged => Name != LastName;

            public string ThreadFlagString => "[" + LastName + "->" + Name + "]\n";

            public void NewThread()
            {
                LastName = Name;
                Name = Thread.CurrentThread.ManagedThreadId.ToString();
            }
        }

        int IndentCount;
        bool IsLineStart = true;
        readonly WriteInitiator Instance = new WriteInitiator();

        public Writer() => DebugTextWriter.Register();

        internal void IndentStart() => IndentCount++;
        internal void IndentEnd() => IndentCount--;

        internal void ThreadSafeWrite(string text, bool isLine)
        {
            lock(Instance)
            {
                Instance.NewThread();

                text = text.Indent(isLineStart: IsLineStart, count: IndentCount);

                if(Instance.ThreadChanged && Debugger.IsAttached)
                {
                    var threadFlagString = Instance.ThreadFlagString;
                    if(!IsLineStart)
                        if(text.Length > 0 && text[0] == '\n')
                            threadFlagString = "\n" + threadFlagString;
                        else
                            throw new NotImplementedException();
                    Debug.Write(threadFlagString);
                }

                Write(text, isLine);

                IsLineStart = isLine;
            }
        }

        static void Write(string text, bool isLine)
        {
            if(isLine)
                Console.WriteLine(text);
            else
                Console.Write(text);
        }
    }
}