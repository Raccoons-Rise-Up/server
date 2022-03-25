@echo off
mongosh.exe --port 27017 -u "valk" -p "nimda" --authenticationDatabase "admin"
pause