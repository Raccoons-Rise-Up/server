# Contributing

## Setup
### .NET
Make sure you have all the required tools to run dotnet projects. (I'm not sure myself how I setup this up, maybe someone can reinforce this part of the CONTRIBUTING.md document)

### Entity Framework Core - SQLite
Run the following commands
```
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If you should have any questions please contact me on Discord (`valk#9904`) or email me at `sebastianbelle074@protonmail.com`

## Formatting Rules
- Methods should follow PascalFormat
- If using `{}` please fully expand
- Field members should start with `m_`, static fields with `s_` (e.g. `m_VariableName`)
- Try to use `var` wherever possible

## Creating a Pull Request
1. Always test the application to see if it works as intended with no additional bugs you may be adding!
2. State all the changes you made in the PR, not everyone will understand what you've done!
