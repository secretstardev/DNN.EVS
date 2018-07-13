<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DynamoAdminSettings.ascx.cs" Inherits="Dynamo.DesktopModules.Admin.DynamoAdminSettings" %>
<b>Portal Script Settings</b>
<fieldset title="Portal Script Settings" >
<i>These settings are for ALL Dynamo modules on the entire Portal, and not just this single instance.</i><br /><br />
<asp:CheckBox ID="EnableScripting" AutoPostBack="true" runat="server" Text="Scripting Support Enabled" oncheckedchanged="EnableScripting_CheckedChanged" />
<asp:Panel ID="ScriptingEnabledPanel" Enabled="false" runat="server">
<asp:CheckBox ID="AllowCLRCheckbox" runat="server" Text="Allow CLR" />
<br /><br />
Portal Scripting Security Options:
<br />
<asp:CheckBox ID="IgnoreSecurityCheckbox" AutoPostBack="true" runat="server"  Text="Ignore All Security Checks"  oncheckedchanged="IgnoreSecurityCheckbox_CheckedChanged"  />
<asp:Panel ID="SecurityOptionsPanel" Enabled="false" runat="server">
<asp:CheckBoxList ID="SecurityOptionsCheckboxList" DataTextField="Name" DataValueField="FullName" runat="server"></asp:CheckBoxList>
</asp:Panel>
</asp:Panel>
<asp:Button ID="SaveButton" runat="server" Text="Save" onclick="SaveButton_Click" />
