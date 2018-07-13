<ol>
@{
var tabInfoList = DotNetNuke.Entities.Tabs.TabController.GetTabsByParent(this.TabId,this.PortalId);

for(var x=0;x<tabInfoList.Count;x++) {
	var tab = tabInfoList[x];
	if(tab.IsVisible) echo("<li><a href='"+tab.FullUrl+"'>"+tab.TabName+"</a></li>");

}

return "";
}@
</ol>
