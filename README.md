# SWAroutes
When I was a kid (1970's), I used to love to go to the airport in Madison, Wisconsin and get airline timetables.  One airline (probably Northwest) would publish their routes, sorted by flight number.  Of course, nobody publishes timetables anymore, but I thought it would be fun to see if I could build a similar document using data collected from the Internet.

Later, in 2009, I was lucky enough to win Southwest Airlines contest "Are you smarter than a schedule planner?"  The prize was a fantastic day at Southwest's HQ in Dallas.  I summarized my experience here:  https://www.flyertalk.com/forum/southwest-airlines-rapid-rewards/937625-great-day-southwest-airlines-airplane-lover-like-me.html

Back on the subject of an airline's routes by flight number, using C#, I wrote a program that collected information from www.flightaware.com for flights operated by Southwest Airlines.  I stored the data I collected in a MySQL database. I then wrote another program that analyzed the collected data, for the week of June 20 to June 26, 2021.  The result is shown in the file SWAroutes.txt, in this project.

Also provided is my C# code.
