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
6. Switch to admin `use admin`
7. Create a new auth user
```js
db.createUser(
  {
    user: "Admin",
    pwd: "nimda",
    roles: [
      { role: "userAdminAnyDatabase", db: "admin" },
      { role: "readWriteAnyDatabase", db: "admin" }
    ]
  }
)
```
8. Optional: Connect as Admin user through shell `mongosh.exe --port 27017 -u "Admin" -p "nimda" --authenticationDatabase "admin"`
9. Create `auth.txt` in `bin/Debug/net5.0` with the following contents
```
Admin
nimda
```

[Shell Reference](https://www.mongodb.com/docs/manual/reference/mongo-shell/)  
[Database Reference](https://www.mongodb.com/docs/manual/reference/command/)  
