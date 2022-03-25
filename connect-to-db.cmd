@echo off
mongosh.exe --port 27017 -u "Admin" -p "nimda" --authenticationDatabase "admin"
pause