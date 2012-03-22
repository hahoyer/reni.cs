// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.UI;
using HWClassLibrary.Helper;
using Reni;

namespace WebSite
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e) { }
        
        public string CompileAndRun(string text)
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
            var c = new Compiler(parameters, fileName);
            c.Exec();
            var result = stringStream.Result;
            return result;
        }

        protected void ButtonOkClick(object sender, EventArgs e)
        {
            Result.Text = CompileAndRun(TbName.Text);
        }

        protected void TbName_TextChanged(object sender, EventArgs e)
        {

        }
    }
    sealed class StringStream : ReniObject, IOutStream
    {
        readonly StringBuilder _result = new StringBuilder();
        internal string Result { get { return _result.ToString(); } }
        void IOutStream.Add(string text) { _result.Append(text); }
    }
}

