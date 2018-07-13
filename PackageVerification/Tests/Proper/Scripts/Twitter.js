@{

style(".TwitterStatus{color:blue}");

echo("<script type='text/javascript'>");
echo("function UpdatePanel() {");
echo("__doPostBack('"+UpdatePanel.ClientID+"', '');");
echo("setTimeout(eval('UpdatePanel'),5000);");
echo("}");
echo("setTimeout(eval('UpdatePanel'),5000);");
echo("</script>");

echo(Twitter.Timeline());

return "";
}@