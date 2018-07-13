@{
echo(this.PortalId);
var tabs = DotNetNuke.Entities.Tabs.TabController.GetPortalTabs(this.PortalId, -1, false, false);
echo("<ul>");
for (var x = 0; x < tabs.Count; x++)
{
	echo("<li><a href='"+tabs[x].FullUrl+"' target='_new'>");
	echo(tabs[x].TabName);
	echo("----");
	echo(tabs[x].TabPath);
	echo("</a></li>");
}
echo("</ul>");
return "";
}@