# Contributing
## Table of Contents
1. [Formatting Guidelines](#formatting-guidelines)
2. [Database](#database)
    - [Updating the Database](#updating-the-database)
    - [Database Errors](#database-errors)
3. [Notes](#notes)
4. [Creating a Pull Request](#creating-all-pull-request)

## Formatting Guidelines
- Methods should follow PascalFormat
- Try to use `var` where ever possible

## Database
### Updating the Database
If changing the field names in a table be sure to add `migrationBuilder.DropTable("TableName");` in the migration script just before the table is created. This is not ideal if there is important data that needs to be kept, perhaps in the future someone can expand on this section of the contributing document.

Run the following commands
```
dotnet ef migrations add SomeMeaningfulName
dotnet ef database update
```

### Database Errors
If you changed the name of one of the models and are getting the error `The entity type 'xxxxx' requires a primary key to be defined.`, this is most likely because the ID field was not changed to fit the new name of the model you renamed. For example, say there is a model class called `Player` with primary ID called `PlayerId`, if the model was renamed to `ModelPlayer`, the primary ID also has to be renamed to `ModelPlayerId`.

If you feel the database is beyond repair, you can delete it at `AppData\Local\ENet Server\Database.db`

## Notes
- Do not forget to lock threads that are reading or writing from the same variables

## Creating a Pull Request
1. Always test the application to see if it works as intended with no additional bugs you may be adding!
2. State all the changes you made in the PR, not everyone will understand what you've done!
