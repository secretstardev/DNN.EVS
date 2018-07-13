

return Main();

function Main() {

    //var id = "81496062@N00";
    //var url = "http://api.flickr.com/services/feeds/photos_public.gne?id=[ID]&lang=en-us&format=json";
    //var jsonURL = url.replace("[ID]", id);

    var jsonUrl = "http://ws.geonames.org/citiesJSON?north=44.1&south=-9.9&east=-22.4&west=55.2&lang=en";

    
    var feed = DownloadFile(jsonURL);
    var flickr = eval(feed);


    return "done";
}
