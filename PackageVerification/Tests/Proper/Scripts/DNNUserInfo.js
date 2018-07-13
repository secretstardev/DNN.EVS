<table border='0'>
@{
if(this.UserInfo != null) {
echo("<tr><td>Display Name:</td><td>" + this.UserInfo.DisplayName + "</td></tr>");
echo("<tr><td>Email:</td><td><a href='mailto:" + this.UserInfo.Email + "'>" + this.UserInfo.Email + "</a></td></tr>");
echo("<tr><td>FullName</td><td>" + this.UserInfo.FullName+ "</td></tr>");
echo("<tr><td>IsSuperUser</td><td>" + this.UserInfo.IsSuperUser+ "</td></tr>");
echo("<tr><td>Username</td><td>" + this.UserInfo.Username+ "</td></tr>");

    if(this.UserInfo.Membership != null) {

echo("<tr><td>Is Online</td><td>" + this.UserInfo.Membership.IsOnLine+ "</td></tr>");
echo("<tr><td>CreatedDate</td><td>" + this.UserInfo.Membership.CreatedDate+ "</td></tr>");
}
}
}@
</table>
