# Contribute to this repository

Thanks for checking out my repository! 
This document explains the way of working and how to setup your development environment!.

Please read this before submitting PRs!

## Way of working

### Branching

The `master` branch is the primary branch of this repository. This contains the latest code that has been released.<br>
In the future, there may also be a `develop` branch which contains the code for the next feature release.

The well known [Git flow](https://gitkraken.com/learn/git/git-flow) strategy has been adopted for this repository. Few naming conventions:
- **Branch names should be kebab-case!**
- Feature branches -> `feature/<short-descriptive-name>`.
- Bugfix branches -> `bugfix/<short-descriptive-name>`.

### Pull Requests

Since `master` and `develop` are protected branches, new changes must be merged via Pull Requests.<br>
When submitting a new PR, will provide a description of the changes to make it easier to review.
Once a PRs is approved, either the reviewer or author can merge it.

**Primary merging method should be Squashing!**

### Code Guidelines

When writing code for this repository, please follow [the code guidelines](docs/CODE_GUIDELINES.MD).

## Setup dev environment

The following tools are required to develop:
* .NET SDK v10.x
    * Installing dotnet-ef is recommended.
* Microsoft SQL Server (can be containerized)
* Node v24.x

### Build the entire project
Open a terminal window in the folder containing DutchMetar.sln and run the following commands:

```powershell
# Build all
dotnet restore
dotnet build
dotnet test # Runs all unit tests

# Build specific projects
dotnet build ./src/DutchMetar.Web.Server # Web app
dotnet build ./src/DutchMetar.WorkerService # Worker service
```
To run a specific project, navigate to the folder containing the .csproj file such as `.\src\DutchMetar.WorkerService` and run `dotnet run` in the temrinal.

To build the frontend client project, navigate to DutchMetar.Web.Client and run:
```powershell
npm ci
npm run build
```
Running dotnet project `DutchMetar.Web.Server` will auto-start the frontend project.

### Manage database with EF migrations

Entityframework Core is used to create and update the SQL database using migrations.
The following example operations can be performed to help you get started

```powershell
# Create or update database
# Ensure the specified startup project contains a valid connection string!
dotnet ef database update --startup-project ..\DutchMetar.WorkerService

# Create a new migration
dotnet ef migrations add <MIGRATION_NAME> --startup-project ..\DutchMetar.WorkerService -o .\Infrastructure\Data\Migrations
```
Ensure working directory of the terminal is set to the folder containing `DutchMetar.Core.csproj`!

### Using Docker Compose
Additionally, you can use docker compose to run the entire project. See file `docker-compose.yml`.
When starting all containers at once, some may fail and auto-restart due to SQL server not getting ready on time.

Docker must be installed on your machine to for this.