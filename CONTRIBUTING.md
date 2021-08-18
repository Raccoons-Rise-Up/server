# Contributing

## Setup
### .NET
Make sure all the required tools are installed to run dotnet projects. (I'm not sure myself how I setup this up, maybe someone can reinforce this part of the CONTRIBUTING.md document)

### Entity Framework Core - SQLite
Run the following commands
```
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Everything should be setup now, start the project.
```
dotnet run
```

## Notes
- Do not forget to lock threads that are reading or writing from the same variables

## Formatting Guidelines
- Methods should follow PascalFormat
- If using `{}` please fully expand
- Variables should be camelCase regardless if private or public
- Try to use `var` where ever possible

## Creating a Pull Request
1. Always test the application to see if it works as intended with no additional bugs you may be adding!
2. State all the changes you made in the PR, not everyone will understand what you've done!
