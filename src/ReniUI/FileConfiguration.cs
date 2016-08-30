using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using ScintillaNET;

namespace ReniUI
{
    sealed class FileConfiguration : DumpableObject
    {
        readonly string FileName;
        public FileConfiguration(string fileName) { FileName = fileName; }

        internal Persister FilePersister
            =>
            new Persister(SystemConfiguration.GetEditorConfigurationPath(FileName).FileHandle())
            ;

        internal string Status
        {
            get { return StatusFile.String; }
            set { StatusFile.String = value; }
        }

        File StatusFile
        {
            get
            {
                var statusFileName = SystemConfiguration.GetConfigurationPath(FileName)
                    .PathCombine("Status");
                var fileHandle = statusFileName.FileHandle();
                return fileHandle;
            }
        }

        internal void OnClosing() { Status = "Closed"; }

        internal void OnUpdate(UpdateChange change)
        {
            if(change.HasFlag(UpdateChange.Selection))
                FilePersister.Store("Selection");
        }

        internal void Connect(Scintilla editor)
        {
            FilePersister.Register
                (
                    "Selection",
                    item => editor.SetSelection(item.Item1, item.Item2),
                    () =>
                        new Tuple<int, int>
                            (editor.SelectionStart, editor.SelectionEnd))
                ;
            FilePersister.Load();
            Status = "Open";
            editor.UpdateUI += (s, args) => OnUpdate(args.Change);
        }
    }
}