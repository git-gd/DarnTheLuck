DarnTheLuck is a demo computer repair shop ticket system.
* Users are authenticated.
* Customers can create and view their own repair tickets as well as edit Ticket Notes. 
* Technician can view and update customer Ticket Statuses.
* Admin can create, read, update and delete all tickets.

DarnTheLuck uses:
* C#
* JavaScript
* ASP.NET CORE / MVC
* Razor / HTML
* MySQL
* BootStrap / CSS

SETUP:
* Create a MySQL Schema
* Update Startup.cs options.UseMySql(CONNECTION) to use a valid MySQL connection string
* Run dotnet ef migrations add Initial / dotnet ef database update
* Register a user
* The first user to run Admin/CreateRole is assigned to the Admin role
