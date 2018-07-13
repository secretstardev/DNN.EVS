@{
var from = get("fromemail");
if(from == null || from == "") from = this.UserInfo.Email;

}@

<fieldset><legend>Provide Feedback</legend>
Your Email:<br /><input type='text' value='@{=from}@' name='fromemail'><br />
Subject:<br /><input type='text' value='@{=get("subject")}@' name='subject'><br />
Body:<br /><textarea name='body' rows='10' cols='50'>@{=get("body")}@</textarea><br />
<input type='submit' value='Send Feedback'><br />
</fieldset>


@{
if(Page.IsPostBack) {
    var fromemail = get("fromemail");
    var subject = get("subject");
    var body= get("body");
  if(fromemail!="" && subject!="" && body!="") {
      var val = DotNetNuke.Services.Mail.Mail.SendMail(fromemail, "admin@localhost", "", subject, body, "", "", "", "", "", "");
      if(val=="") val = "Your message was sent.";      
      echo(val);
      }

}

}@
