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
using System.Drawing;
using System.Linq;
using System.Web.UI;
using HWClassLibrary.Debug;
using Reni.Parser;

namespace WebSite
{
    public partial class Parser : Page
    {
        protected PrioTable PrioTable = PrioTable.FromText("");
        protected void OnPrioListChanged(object sender, EventArgs e)
        {
            if(PrioList.Text == "?")
                PrioList.Text = PreparedPrioTableText;
            PrioTable = Services.FormatPrioTable(PrioList.Text);
        }
        const string PreparedPrioTableText = @"Left not
Left and
Left or
Left * /
Left + -
Left = <>
Right :=
TELevel then else
Left function
Right :
Right , ;
ParLevel ( { ) }
";

        protected void OnProgramChanged(object sender, EventArgs e)
        {
            SyntaxTree.ImageUrl
                = "data:image/png;base64,"
                  + Services
                        .SyntaxGraph(PrioTable, Program.Text,new Size(800,600))
                        .ToBase64();
        }
    }
}