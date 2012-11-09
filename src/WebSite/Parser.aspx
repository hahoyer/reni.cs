<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Parser.aspx.cs" Inherits="WebSite.Parser" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Parser</title>
</head>
<body>
    <form id="form1" method="post" runat="server">
        <asp:Label ID="DateTimeNow" runat="server" Text="Label"></asp:Label>
    <table style="height: 283px; width: 776px; margin-bottom: 156px">
        <tr>
            <td>
                <asp:TextBox id="PrioList" runat="server" TextMode="MultiLine" Height="430px" OnTextChanged="OnTextChanged" AutoPostBack="True"/>
            </td>
            <td>
                <asp:TextBox id="PrioTable" runat="server" TextMode="MultiLine" Height="430px" Width="594px" />
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
