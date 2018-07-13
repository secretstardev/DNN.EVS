<ul>
@{
var tc = new DotNetNuke.Entities.Tabs.TabController();
var parentID = tc.GetTab(this.TabId).ParentId;

var tabInfoList = tc.GetTabsByParentId(parentID,this.PortalId);

for(var x=0;x<tabInfoList.Count;x++) {
	var tab = tabInfoList[x];
	if(tab.IsVisible) echo("<li><a href='"+tab.FullUrl+"'>"+tab.TabName+"</a></li>");

}

return "";
}@
</ul>