# Coding Guidelines

This document describes basic guidelines on how code should be written.
This document will contain links to official web resources along with repository specific rules.
Rules in this document take precedence over rules mentioned in online resources.

### .NET / C#

For this project, the goal is to stick to the official Microsoft guidelines as much as possible.
Any exceptions will be mentioned here in this document.

#### Online resources
- [Identifier conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)
- [Coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Unit test best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

#### General rules
- Write concise and readable code. Prefer not to overengineer or use complicated patterns.
- Max 1 class/enum/innterface/etc per file. 

### Unit tests
- Use naming convention `TargetMethod_Conditions_ExpectedResult` for your unit tests. Example: `GetEntity_ValidId_ReturnsCorrectEntity()`.
- Use the well known **arrange, act, assert** pattern. 
    - It is not neccesary to comment each section.

### Design principles
The majority of the application logic is contained in the ``DutchMetar.Core` project.
The project folder structure is based on **Vertical Slices** where the application is divided into features.
Each feature focusses on a specific use case and contains everything that is required for that specific feature.

#### Guidelines
- Features should be self-contained as possible and NOT reference other feature.
    - Goal: deleting a feature should not break other parts of the application.
    - Exception: features can reference "common" folder that contain re-useable code
    - Complex features can be divided into repositories/services/models to help with readibility.
- Domain entities should never be exposed outside of the Core project. All features should return first map entities to DTO/Models.
    - These models should only contain what is actually needed from the entity.
- All domain entities should inherit the abstract `Entity` class.
    - This ensures that EF Core will correctly update meta fields.


### Angular / Typescript

... TODO