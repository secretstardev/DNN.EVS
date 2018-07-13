<textarea name="SQL" rows="20" cols="80">SELECT top 5 FirstName, LastName FROM Users</textarea>
<br />
<input type='submit' value='Execute'><br /><br />

@{=Page.IsPostBack}@

<table border='1'>

@{
var sb = new System.Text.StringBuilder();
var SQL = get("SQL");
sb.Append(SQL);
if(Page.IsPostBack && SQL!="") {

var rdr = ExecuteSql(SQL);
var x = 0;
while(rdr.Read()) {
   sb.Append("<tr>")
   for(var y=0;y<rdr.FieldCount;y++) {
     sb.Append("<td>" + GetSqlValue(rdr, y) + "</td>");
   }
   sb.Append("</tr>")
   x++;
   if(x>10) break;
}
}
return sb.ToString();
}@

</table>


