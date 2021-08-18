# Contributing

## .NET
Make sure all the required tools are installed to run dotnet projects. (I'm not sure myself how I setup this up, maybe someone can reinforce this part of the CONTRIBUTING.md document)

## Entity Framework Core - SQLite
Create the database with the following commands
```
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If changing the fields in a table be sure to add `migrationBuilder.DropTable("TableName");` in the migration script just before the table is created. This is not ideal if there is important data that needs to be kept, perhaps in the future someone can expand on this section of the contributing document.

Run the following commands
```
dotnet ef migrations add SomeMeaningfulName
dotnet ef database update
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
