//an example of how to download JSON script, eval it into an object, and then spit out the hierarchy

var url = "http://ws.geonames.org/citiesJSON?north=44.1&south=-9.9&east=-22.4&west=55.2&lang=en";
var c = new System.Net.WebClient();
var json = c.DownloadString(url);

var cities = eval('(' + json + ')');

echo(cities[0].countrycode);

return ""