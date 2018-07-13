@{
function DownloadFile(url) {
    var c = new System.Net.WebClient();
    return c.DownloadString(url);
}
}@