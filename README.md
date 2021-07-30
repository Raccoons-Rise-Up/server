# Game Server
A multi-threaded console / game server that handles logging, user commands and connections from clients.

## Features
- Console logs messages from several threads and listens for user commands at the same time
- Each character can have its own unique color (limited to the colors defined by `System.ConsoleColor`)
- Each message shows a timestamp, the name of the thread and the log level

## Usage
```cs
Log("Hello world");
LogWarning("Be careful!");
LogError("Oops!");
```

## Known Issues
Please see [Issues](https://github.com/Kittens-Rise-Up/server/issues)

## Preview
![Untitled](https://user-images.githubusercontent.com/6277739/127713984-25b46c97-aba7-47f3-846c-83be1ba0c741.png)

## Contributing
Please see [CONTRIBUTING.md](https://github.com/Kittens-Rise-Up/server/blob/main/CONTRIBUTING.md)

Talk to `valk#9904` in the [Kittens Rise Up](https://discord.gg/cDNf8ja) discord for more info.
