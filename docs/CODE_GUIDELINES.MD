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
- Prefer using modern language syntax unless otherwise states.
    - **Exception:** Use classic constructor style instead of newer primary constructors.

#### Unit tests
- Use naming convention `TargetMethod_Conditions_ExpectedResult` for your unit tests. Example: `GetEntity_ValidId_ReturnsCorrectEntity()`.
- Use the well known **arrange, act, assert** pattern. 
    - It is not neccesary to comment each section.

#### Design principles
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

For this project, the goal is to stick to the official Angular guidelines as much as possible.
Any exceptions will be mentioned here in this document.

#### Online resources
- [Angular style guide](https://angular.dev/style-guide)

#### Design principles

The application code is divided into a features layer and shared layer, just like the backend Core project.
Shared folder should contain all code that should be available to all features.

Folder in the features layer represent features. Each feature focusses on a specific use case and contains everything that is required for that specific feature.
Features should not reference other features except it's own child features.
Deleting a feature should - in theory - not break other parts of the application.

#### Guidelines

- Write concise and readable code. Prefer not to overengineer or use complicated patterns.
- Typescript classes should be arranged in the following order:
    - **By symbol type:** fields -> getters -> constructor -> functions
    - **Then by access modifier:** public -> protected -> private
- Do **NOT** use underscore `_` prefix for fields inside classes.
- Prefer using the newest language features where possible.
- Prefer using union string types over enums (where possible) as those are slightly easier to use in HTML templates.
- Components should always seperate into `*.ts/*.html/*.scss` files. 
    - HTML templates should not be embedded inside the *.ts file.
- Prefer using `inject()` over traditional constructor DI.
- Prefer using Signals over Rxjs for simple events inside components.
    - Rxjs and Signals can be mixed using the built-in interop features.
- Services are responsible for retrieving data from the backend API and making it available to components.
    - Services (and respective models) should always be placed in the shared folder.
    - A service should also (if possible) act as single source of truth by exposing Observables that emit the data.
        - Services should also (optionally) expose loading status observables so that consumer components can easily keep track of loading state.
    - Rxjs should be used for events within services.