<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Parser.aspx.cs" Inherits="WebSite.Parser" %>
<%@ Import Namespace="HWClassLibrary.Debug" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="HWClassLibrary.Helper" %>

<!DOCTYPE html>

<html>
<head id="Head1" runat="server">
    <title>Parser</title>
    <link rel="stylesheet" href="Parser.css" type="text/css" runat="server" />
</head>
<body onload="draw();">
    <form id="form1" method="post" runat="server">
        <%= DateTime.Now.Format() %>
        <br/>
        <div>
            <asp:TextBox 
                id="PrioList" 
                runat="server" 
                TextMode="MultiLine" 
                Rows="20"
                Columns="12"
                OnTextChanged="OnPrioListChanged" 
                AutoPostBack="True"
                />
                    
            <table border="1" id="PrioTable">
                <tr>
                    <th/>
                    <% foreach(var recentTokenName in PrioTable.RecentToken){ %>
                        <th><%= recentTokenName %></th>
                    <%}%>
                </tr>
                <% foreach(var newTokenName in PrioTable.NewToken){ %>
                    <tr>
                        <th><%= newTokenName %></th>
                        <% foreach(var recentTokenName in PrioTable.RecentToken){ %>
                            <td align="center">
                                <% switch(PrioTable.Relation(newTokenName,recentTokenName)){ %>
                                    <% case '+': %>
                                    <asp:Image ID="PushImage" runat="server" ImageUrl="~/Resources/Stack.Push.bmp"/>
                                    <% break;%>
                                    <% case '-': %>
                                    <asp:Image ID="PullImage" runat="server" ImageUrl="~/Resources/Stack.Pull.bmp"/>
                                    <% break; %>
                                    <% default: %>
                                    <asp:Image ID="MatchImage" runat="server" ImageUrl="~/Resources/Stack.Match.bmp"/>
                                    <% break; %>
                                <% }%>
                            </td>
                        <%}%>
                    </tr>
                <%}%>
            </table>
            <asp:TextBox 
                id="Program" 
                runat="server" 
                OnTextChanged="OnProgramChanged" 
                AutoPostBack="True"
                TextMode="MultiLine" 
                />
                <asp:Image id="SyntaxTree" runat="server"/>
                
        </div>
    </form>
</body>
</html>

