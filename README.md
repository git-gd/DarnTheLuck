DarnTheLuck is a demo computer repair shop ticket system.
* Users are authenticated with ASP.NET Core Identity.
* Authenticated Users can be assigned to Technician and Admin Roles
* Users can create and view their own Tickets.
* Users can update their Ticket notes if the Ticket status is not "Shipped".
* Technicians can view all User Tickets.
* Technicians can update all User Ticket statuses and Ticket technician fields.
* Admin can create, read, and delete all tickets.
* Users can sort and search Tickets.
* Searching a Ticket highlights the search terms.
* Ticket lists are paginated.
* Users can generate shareable codes that grant View access to their Tickets.
* Users can consume shareable codes generated by other Users.
* Users can manage their own shareable codes.
* Email authorization and password recovery is done using Mailjet.

DarnTheLuck uses:
* C#
* JavaScript / AJAX
* ASP.NET CORE / MVC
* Razor / HTML
* MySQL
* BootStrap / CSS

SETUP:
* Create a MySQL Schema
* Update Startup.cs options.UseMySql(CONNECTION) to use a valid MySQL connection string
* Run dotnet ef migrations add Initial / dotnet ef database update
* Register a user
* The first user to run /Config is assigned to the Admin role
