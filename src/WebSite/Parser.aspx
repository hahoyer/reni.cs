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
  <script type="text/javascript">
      function draw() {
          var canvas = document.getElementById("SyntaxTree");
          if (canvas.getContext) {
              var ctx = canvas.getContext("2d");                // Get the context.
              ctx.clearRect(0, 0, canvas.width, canvas.height);    // Clear the last image, if it exists.
              var image = document.getElementById("pix");       // Get the address of the picture.
              // Straight draw. 
              ctx.drawImage(image, 1, 1);
              // Stretch the image a bit.
              ctx.drawImage(image, 125, 125, 200, 200);
              // Draw it in pieces.
              ctx.drawImage(image, 1, 1, image.width / 2, image.height / 2, 50, 125, 50, 50);
              ctx.drawImage(image, 1, image.height / 2, image.width / 2, image.height / 2, 50, 275, 50, 50);
              ctx.drawImage(image, image.width / 2, 1, image.width / 2, image.height / 2, 350, 125, 50, 50);
              ctx.drawImage(image, image.width / 2, image.height / 2, image.width / 2, image.height / 2, 350, 275, 50, 50);
          }
          document.getElementById("pix").style.display = "none";  // hide image when we're done
      }
  </script>
</head>
<body onload="draw();">
 <img id="pix" src="http://go.microsoft.com/fwlink/?LinkID=199028" />   
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

            <canvas id="SyntaxTree" width="600" height="500" runat="server"/>
                
        </div>
    </form>
</body>
</html>

