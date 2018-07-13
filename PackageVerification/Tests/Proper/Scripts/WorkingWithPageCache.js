@{
var cacheItem;
cacheItem = cacheGet("item");

if(!isNull(cacheItem)) {
    echo("Cache hit:" +cacheItem);
} else  {
    var date = System.DateTime.Now.ToString();
    echo("caching:" + date);
    cacheAdd("item", date, 5);
}
   
return "";
}@