@{
var sb = new System.Text.StringBuilder();

function Log(something) {
   this.value = something;
   this.createdDate = System.DateTime.Now;
   this.updatedDate = new Date();
}

Log.prototype.getValue = function () { return this.value; }
Log.prototype.setValue = function (newValue) { this.value = newValue; this.updatedDate = new Date(); }

var log = new Log("some cool value");

vLog("value", log.getValue());
vLog("createdDate", log.createdDate);
vLog("updatedDate", log.updatedDate);

log.setValue("new value");

vLog("value", log.getValue());
vLog("createdDate", log.createdDate);
vLog("updatedDate", log.updatedDate);

vLog("Date Compare:", (vLog.createdDate < log.updatedDate));


function vLog(name, value) {
    sb.Append(name + "=[" + value + "]<br />");
}



return sb.ToString();

}@