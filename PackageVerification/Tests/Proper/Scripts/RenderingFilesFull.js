@{
var header = (RenderFile(path("~/DesktopModules/Dynamo/Scripts/Header.js")));
var footer =(RenderFile(path("~/DesktopModules/Dynamo/Scripts/Footer.js")));

return header + "<br />body<br />" + footer;
}@