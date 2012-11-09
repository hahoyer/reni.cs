<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Parser.aspx.cs" Inherits="WebSite.Parser" %>
<%@ Import Namespace="HWClassLibrary.Helper" %>
<%@ Import Namespace="Reni.Parser" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Parser</title>
</head>
<body>
    <form id="form1" method="post" runat="server">
        <%= DateTime.Now.Format() %>
        <br/>
        <div align="left">
            <asp:TextBox 
                id="PrioList" 
                runat="server" 
                TextMode="MultiLine" 
                Rows="20"
                Columns="12"
                OnTextChanged="OnTextChanged" 
                AutoPostBack="True"
                />
        <div>
            <%= PrioTableText %>

        </div>
        </div>
                
    </form>
</body>
</html>
