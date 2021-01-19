# Jan 17, 2021

## NuGet

* Add Pomelo for MySQL support

## appsettings.json

* "DefaultConnection": "server=localhost;database=darntheluck;user=darntheluck;password=badpassword1"

## MySQL

* Create schema for project (darntheluck)
* Create user for project (darntheluck/badpassword1)

# Jan 18, 2021

## Ticket

* Initial Ticket Controller, empty Models, View setup completed
* Ticket Controller set to use [Authorize]
* Initial Ticket creation process is working
* Added Ticket Status table and linked it to Ticket
* Added Ticket List view
* Users can create, list and view details of Tickets

##### Blazor support pushed back - possibly to a second WASM project connected through DTOs

# Jan 19, 2021

## Ticket List (HTML)

* No-Wrap Bootstrap grid with text-overflow: elipse and scalable font size