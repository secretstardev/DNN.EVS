@{
var app = DotNetNuke.Application.DotNetNukeContext.Current.Application;

echo("<br /> Company:");
echo( app.Company );
echo("<br /> SKU:");
echo( app.SKU );
echo("<br /> Status:");
echo( app.Status );
echo("<br /> Title:");
echo( app.Title );
echo("<br /> Url:");
echo( app.Url );
echo("<br /> Version:");
echo( app.Version );
echo("<br /> Description:");
echo( app.Description ); 


 return "";

 }@