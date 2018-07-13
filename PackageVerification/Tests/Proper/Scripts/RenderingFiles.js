@{
var str = "";

str += RenderFile(path("~/DesktopModules/Dynamo/Scripts/Header.js")) + "<br /><br />";
str += RenderFile(path("~/DesktopModules/Dynamo/Scripts/HelloWorld.js")) + "<br /><br />";
str += RenderFile(path("~/DesktopModules/Dynamo/Scripts/Footer.js"));
return str;


}@