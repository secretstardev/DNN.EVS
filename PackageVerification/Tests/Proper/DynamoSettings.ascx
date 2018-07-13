<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DynamoSettings.ascx.cs" Inherits="Dynamo.DynamoSettings" %>
<asp:Panel ID="DisabledPanel" runat="server" Visible="false">
Your site administrator has disabled scripting
</asp:Panel>

<asp:Panel ID="PagePanel" runat="server">

<asp:Panel ID="Panel1" runat="server" BorderWidth="1" BorderStyle="Ridge">
Output:<br /><br />
</asp:Panel>

Sample Scripts:<asp:DropDownList ID="ScriptsDDL" runat="server">
</asp:DropDownList>
<asp:Button ID="UseSampleButton" runat="server" Text="Copy" onclick="UseSampleButton_Click" /><br />

Script:<br />
<asp:TextBox ID="TextBox1" runat="server" Height="400px" TextMode="MultiLine" Width="800px"></asp:TextBox>
<br />
<asp:CheckBox ID="ShowDebuggerDetailsCheckbox" Text="Show Debugger Details" runat="server" />
<asp:Button ID="Button1" runat="server" Text="Preview" onclick="Button1_Click" />
<asp:Button ID="SaveButton" runat="server" Text="Save" OnClientClick="FixEditorValue(); return true;"
    onclick="SaveButton_Click" />

<asp:Panel ID="DebuggerPanel" runat="server" BorderWidth="1" BorderStyle="Ridge" Visible="false">
<br /><br />Debugger:<br />
</asp:Panel>



<script language="Javascript" type="text/javascript">

var editID = '<%=TextBox1.ClientID %>';
var editor = document.getElementById(editID);

// initialisation
editAreaLoader.init({
    id: editID	// id of the textarea to transform		
    , start_highlight: true	// if start with highlight
    , allow_resize: "both"
    , allow_toggle: true
    , word_wrap: true
    , language: "en"
    , syntax: "js"
    , line_number: 0
    , toolbar: "new_document, |, search, go_to_line, |, undo, redo, |, select_font, |, change_smooth_selection, highlight, reset_highlight, |"

});
function FixEditorValue() {
    //alert(editAreaLoader.getValue(editID));
    editor.value = (editAreaLoader.getValue(editID));
}


//function AddSnippet(text, value) {
//    //, plugins: "snippets"   ,snippets_select
//    var snippet_select = jQuery("snippet_select");
//    snippet_select.
//          append(jQuery("<option></option>").
//          attr("value", text).
//          text(value)); 


//}


</script>
</asp:Panel>