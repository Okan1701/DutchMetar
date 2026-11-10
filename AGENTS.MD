# AGENTS.md file

## Role of AI

AI tools should be used to assist the developer with writing code. They should not be proposing changes on their own without developer guidance and review.

Usage should be limited to specific tasks (such as writing unit tests or implementing a method). It should avoid building entire applications on its own.

## Project overview

.NET solution for storing and displaying/visualizing METAR and TAF data for airports based in the Netherlands.

Data is retrieved from KNMI Data Platform via MQTT or REST API. Retrieved data is in the form of .txt or .xml files which are further processed into relevant entities.

## Structure

- `DutchMetar.Web.Server` -> backend web project
- `DutchMetar.Web.Client` -> Angular frontend project
- `DutchMetar.WorkerService` -> .NET worker service for populating database
- `DutchMetar.Core` -> Contains core business logic + infrastructure concerns

The majority of backend logic is found in the core project. Other projects serve as "presentation / entrypoint" for the Core project.

## Repository documentation

This repo contains the following docs which are useful for understanding the repo and coding style:
- README.MD
- CONTRIBUTING.MD
- docs/CODE_GUIDELINES.MD

## General Rules

**DO:**
- After every change, perform the following checks:
    - Analyze if the actual changes comply with the user request
    - Analyze if it meets the repository code guidelines
    - Build the solution and/or run tests. Only build/test what is needed!
    - Unused imports in the code are removed.
- Only add code comments to explain specific edge cases
- When modifying existing code, check if there are relevant unit tests to update.
- If able, analyze existing similar code to get a feel for implementation guidelines and style.

**DO NOT:**
- Auto commit, push or create PRs.
    - AI should function as an assistant and should not propose changes on its own.
    - Developers should always review the output themselves and commit/push/merge themselves.
- Add useless/verbose comments in the code.
- Expose domain entities outside Core project.

## Rules for unit test

When writing unit tests, adhere to the following rules unless overriden in the user prompt:

- Always have a clear scope when generating unit tests. If the user did not provide a clear scope, ask for it if able.
- Always write tests cases for observable behavior. Do not write tests for implementation details
- Avoid testing builtin or external frameworks. Assume that they work
- When mocking dependencies, always do the least amount of mocking. Do not overengineer it.
- Do not overengineer the test cases. The fewer tests the better. 
- When fixing failing tests, never adjust the target implementation itself to make the test pass unless agreed upon with the user.
- Focus on testing actual business logic rather than testing infrastructure code.
- When using placeholder/test values in objects, try to use plausible values.

Folder structure:

- For each project that requires unit tests, a identical named test project exists ending with *.Tests or something to that efect.
- THe test project should mirror the folder structure of the target project as much as possible.
