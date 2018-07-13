<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DynamoViewer.ascx.cs" Inherits="Dynamo.DynamoViewer" %>
<asp:Panel ID="DisabledPanel" runat="server" Visible="false">
Your site administrator has disabled scripting
</asp:Panel>

<asp:UpdatePanel ID="UpdatePanel2" runat="server" visible="true">
<ContentTemplate>
<asp:Panel ID="Panel1" runat="server">
</asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>
<br /><br /><br />
<asp:HyperLink ID="EditScriptHyperLink" runat="server">
    <asp:Image ID="Image1" runat="server" ImageUrl="~/images/edit.gif" />
    Edit Script
</asp:HyperLink>

