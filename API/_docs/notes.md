# Notes
- namespaces in C#, logical kind of folders, not physical folders, logical namespaces, where classes live inside the framework or inside our own code. 
- when you create a class it will automatically give it a namespace where physically this class resides. 

## Getting to know the API project files

-lp - specify launch profile
dotenet run -lp https
- when we install packages and or remove them we need to execute .dotnet restore.
- development mode - as much information as possible inside logs
- main entry point is Program.cs - WebApplication.CreateBuilder() create web builder instance. 
- anything before var app = builder.Buold() is considered "Services Container"
- HTTPS request pipeline
- middleware - do something with request, is this req mad by valid user, dos something on way in and way out. 
- dotenet watch run
- namespaces
- casing: C# PascalCasing
- client app uses camelCasing
- .NET 6 string? nullable feature - going to treat strings as optional as they always have been ,a dn code defensively when we use a string in our code to check to see if it has a value or not before we attemt to use it. 
- ImplicitUsings - typpical using statements, but don't really need it.
- dont be tempted to add anything to GlobalUsings.g.cs, but can add own GlobalUsings file.

<hr/>

## Entity Framework Introduction Features

- Entity? Object Relational Mapper - translate our code into SQL commands that update our tables in the database
- ADO.net code used in past .NETto retrieve data from database, fetch, convert, etc, easy to make mistakes
- Entity automates all database related activities
- When we add Entity Framework we need to add an important class DbContext - which adds as a bridge between our domain or entity classes and the databases, and this DbContext is the primary class we use to interact with our db. 
- Entity framework allows us to write Linq queries DbContext.Users.Add(new User {}). 
- Entity framework works with db providers such as SqLite, which doenst use a db server, just a file to store out db. SqLite not production worthy, but good for dev as v. portable.
- DB provider is responsbile for translating linq query into a sql command INSERT INTO Uers(Id, Name); VALUES(4, John)

## Entity Framework Features

- Allows us to query via Linked queries
- Change Tracking
- Saving
- Concurrency - optimizitic concurrency by default to protect overwriting changes made by another user since data fetched from db
- Transactions
- Caching: First level caching out of box - repeated queryingwill return data out of cache instead of hitting the database
- Built-in conventions - follows default rules which automatiaclly confugure the enttity framework schema, or model confugres db. 
- Configurations - ways to overide conventions if we wish
- Migrations - create a db schema, automatically generate our db in db server. Powerful, and conveient, Takes a look at our code and create the db schema that we need to manage our db. So don't need to create our db manually, Entity framework does this dor us. Can do Code-first way around, and then go ahead and create eneitties to match up with database, but far more convenient to use migrations

## Adding Entity Framework to poject

- NuGet - in Visual Studio
- dotnet add package Microsoft.EntityFrameworkCore.Sqlite
- rude edit - hotreload can't deal with it.
- always restart
- dotnet add package Microsoft.EntityFrameworkCore.Design - code-forst approach
- NuGet dotnet restore automatically

# Creating the Connection String

dotnet tool install --global dotnet-ef --version 7.0.5
dotnet ef
dotnet ef migrations add InitialCreate -o Data/Migrations
_InitialCreate.cs in Mirgrations folder, Up() method, and Down(), what to do when we're applyying this migration and unapplying.
User must contain ID as nullable:false, but dooesnt have to include UserName as nullable: true
dotnet ef database update
- SqlLite Studio
- [Route("[controller]"] is a placeholder for name of the controler
- Dependency Injection - service, pluck out Users, using entity framework query, tranlated into SQL query, tand then return Users from api endpoint to client. 

# Asynchronous Code

- scalability + considered best practice.

# Saving code to source control

dotnet new gitignore
dotnet new globaljson
ok to send up sqlite database file,w wpuldnt normally have a db file if using a proper db.
