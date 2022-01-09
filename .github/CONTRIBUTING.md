# Contributing
## Setup
1. .NET is installed from https://dotnet.microsoft.com/download/dotnet/5.0 (cross platform) or with Visual Studio if you check the `.NET desktop environment` workload when installing.
2. Head on over to the [web server](https://github.com/Raccoons-Rise-Up/website/blob/main/.github/CONTRIBUTING.md#setup) and make sure you generated the `private.key` and `public.key`
3. Copy this `public.key` from the web server and paste it in `bin\Debug\net5.0` (if you don't see these folders you need to run the project at least once with `dotnet run`)
4. [Setup MongoDb](#mongodb)
5. Run the project with `dotnet run`

## MongoDb
1. [Install MongoDB Community Edition](https://docs.mongodb.com/manual/tutorial/install-mongodb-on-windows/#install-mongodb-community-edition)
2. [Install MongoSh](https://docs.mongodb.com/manual/tutorial/install-mongodb-on-windows/#install-mongosh)
3. Create database directory
```
cd C:\
md "\data\db"
```
4. Start database with `"C:\Program Files\MongoDB\Server\5.0\bin\mongod.exe" --dbpath="c:\data\db" --bind_ip 127.0.0.1 --port 27017`
5. Open up a new connection to the database with MongoSh `mongosh.exe --port 27017`
6. Create a new auth user
```js
db.createUser(
  {
    user: "myAwesomeUsername",
    pwd: "abc123",
    roles: [ { role: "userAdminAnyDatabase", db: "admin" } ]
  }
)
```
7. Restart database instance with `--auth` param `"C:\Program Files\MongoDB\Server\5.0\bin\mongod.exe" --dbpath="c:\data\db" --auth --bind_ip 127.0.0.1 --port 27017`
8. Connect with MongoSh again to test it out `mongosh.exe --port 27017 -u "myAwesomeUsername" -p "abc123" --authenticationDatabase "admin"`
9. MongoDb is now ready to go, just remember to edit the connection string in the game server with the proper username and password
