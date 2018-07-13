@{
var greeting = "Welcome to our site!";
if(User!=null) greeting = "Welcome to our site, " + User.Name + "!";
var weekDay = System.DateTime.Now.DayOfWeek;
var greetingMessage = greeting + " Today is: " + weekDay;
if(UserInfo!=null) greetingMessage+= "<br />Your email address is:" + UserInfo.Email;


return greetingMessage;
}@