@{
var mc = new DotNetNuke.Entities.Modules.ModuleController();
var module = mc.GetModule(this.ModuleId);
echo("<br /> ModuleName:");
echo(module.DesktopModule.ModuleName);
echo("<br /> Description:");
echo(module.DesktopModule.Description);
echo("<br /> CreatedOnDate:");
echo(module.DesktopModule.CreatedOnDate);
echo("<br /> FolderName:");
echo(module.DesktopModule.FolderName);
echo("<br /> FriendlyName:");
echo(module.DesktopModule.FriendlyName);
echo("<br /> Version:");
echo(module.DesktopModule.Version);
return "";

}@