#region Copyright (C) 2012

//     Project WebSite
//     Copyright (C) 2012 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.UI;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni;

namespace WebSite
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        static string CompileAndRun(string text)
        {
            var fileName =
                Environment.GetEnvironmentVariable("temp")
                + "\\reni.server\\"
                + Thread.CurrentThread.ManagedThreadId
                + ".reni";
            var fileHandle = fileName.FileHandle();
            fileHandle.AssumeDirectoryOfFileExists();
            fileHandle.String = text;
            var stringStream = new StringStream();
            var parameters = new CompilerParameters {OutStream = stringStream};
            var c = new Compiler(fileName, parameters);

            var exceptionText = "";
            try
            {
                c.Exec();
            }
            catch(Exception exception)
            {
                exceptionText = exception.Message;
            }

            var result = "";

            var log = stringStream.Log;
            if(log != "")
                result += "Log: \n" + log + "\n";

            var data = stringStream.Data;
            if (data != "")
                result += "Data: \n" + data + "\n";

            if (exceptionText != "")
                result += "Exception: \n" + exceptionText;

            return result;
        }

        protected void ButtonOkClick(object sender, EventArgs e) { Result.Text = CompileAndRun(Code.Text); }
    }

    sealed class StringStream : ReniObject, IOutStream
    {
        readonly StringBuilder _data = new StringBuilder();
        readonly StringBuilder _log = new StringBuilder();
        internal string Data { get { return _data.ToString(); } }
        internal string Log { get { return _log.ToString(); } }
        void IOutStream.AddData(string text) { _data.Append(text); }
        void IOutStream.AddLog(string text) { _log.Append(text); }
    }
}