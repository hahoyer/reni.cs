<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebSite.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head id="Head1" runat="server">
        <title>Reni Compiler Tool</title>
    </head>            

    <body>
        <form id="reniForm" runat="server" >
            Enter text to compile and press tab:
            <br />
            <asp:TextBox 
                id="Code" 
                runat="server" 
                TextMode="MultiLine" 
                Rows="5" 
                Columns="80"
                AutoPostBack="True"
                OnTextChanged="OnTextChanged"
                />
            <br />
            <div>
                <% foreach(var line in ResultText){ %>
                    <div><%= line %></div>
                <% } %>
            </div>
            <br />
            <br />
            <div align="right">
                For more information see: 
                <a href="http://hahoyer-compiler.blogspot.de">http://hahoyer-compiler.blogspot.de</a>
            </div>
        </form>
        </body>

</html>
