@{
var title = cacheGet("title");

if(isNull(title)) {
    var url = "http://www.rssweather.com/wx/ca//vancouver+automatic+weather+reporting+system/rss.php";
    var feed = Rss.RssFeed.Read(url);
    var channel = feed.Channels[0];
    title = channel.Items[0].Title;
    cacheAdd("title", title, 10);
}

return title;

}@