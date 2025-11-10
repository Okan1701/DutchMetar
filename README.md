# About
> [!WARNING]  
> This project is still a work in progress!

[![Build Status](https://dev.azure.com/okan1701/DutchMetar/_apis/build/status%2FDutchMetar%20CICD?branchName=master)](https://dev.azure.com/okan1701/DutchMetar/_build/latest?definitionId=2&branchName=master)

DutchMetar is a .NET project for scraping METAR data from all available stations in the Netherlands.
The goal is to create a database that contains historic metar data.

Currently the project consists of a recurrent task that scrapes the data and stores it. The goal is to have an interactive frontend that can visualize the data.

The main purpose for this project is purely for hobbyist reasons. 

# Used technologies

The project uses the following tech stacks:
* (ASP) .NET 10
* Hangfire for .NET
* Entityframework Core
* Microsoft SQL Server
* Docker

# Setup dev environment

In order to run the project, you must install the following:
* ASP.NET 10+
    * Installing dotnet-ef is recommended.
* Microsoft SQL Server (can be containerized)

### Build the solution
Open a terminal window in the folder containing DutchMetar.sln and run the following commands:

```
dotnet restore
dotnet build
```

To run a specific project, navigate to the folder containing the .csproj file such as `.\src\DutchMetar.Hangfire.Host` and run `dotnet run` in the temrinal.

### Manage database with EF migrations

Entityframework Core is used to create and update the SQL database using migrations.
The following example operations can be performed to help you get started

```
// Create or update database
// Ensure the specified startup project contains a valid connection string!
 dotnet ef database update --startup-project ..\DutchMetar.Hangfire.Host  

// Create a new migration
dotnet ef migrations add <MIGRATION_NAME> --startup-project ..\DutchMetar.Hangfire.Host -o .\Infrastructure\Data\Migrations
```
Ensure working directory of the terminal is set to the folder containing `DutchMetar.Core.csproj`!

### Using Docker Compose
Additionally, you can use docker compose to run the entire project. See file `docker-compose.yml`.
When starting all containers at once, some may fail and auto-restart due to SQL server not getting ready on time.
