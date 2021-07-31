# Game-Server

## What is this?
A multi-threaded console / game server that handles logging, user commands, database and ENet connections for Kittens Rise Up clients.

I have tried using several networking libs in the past, none of them worked out for me.
- Unity's 2019 LLMAPI has little documentation and unexplained behavior
- Godot's server-client model requires Godot to be built from open source which led me to several issues
- Mirror has to be created within Unity 2019 LTS, it cannot be used in a dotnet console app

The only networking lib that stood out to me is [ENet-CSharp](https://github.com/nxrighthere/ENet-CSharp), I find it to be very simple and straight forward. Since the original repository has their issues closed, you'll find that [this repository](https://github.com/SoftwareGuy/ENet-CSharp) has theirs open and there are many people willing to help you with any questions you may have.

I previously created a [game-server](https://github.com/The-MMORPG-Project/game-server) for [The MMORPG Project](https://github.com/The-MMORPG-Project/website) but it did not follow thread safety and depended on an external API for the console. This game-server follows thread safety and does not depend on any external API for the console.

## Server
### Features
- ENet-CSharp Networking Lib
- Entity Framework Core with SQLite

## Console
### Features
- Console logs messages from several threads and listens for user commands at the same time
- Each character can have its own unique color (limited to the colors defined by `System.ConsoleColor`)
- Each message shows a timestamp, the name of the thread and the log level

### Controls
`Delete` - Delete the character in front of the cursor  
`Backspace` - Delete the character behind the cursor  
`Left Arrow Key` - Navigate one character to the left  
`Right Arrow Key` - Navigate one character to the right  
`Up Arrow Key` - Go to the next recent command in command history  
`Down Arrow Key` - Go to the next previous command in command history  

### Commands
`help [cmd]` - Display a list of helpful commands or get specific information about a specified command.  
`exit` - Allow the server to shutdown properly and then exit the application.  

### Logging
```cs
// Log levels
Log("Hello world");
LogWarning("Be careful!");
LogError("Oops!");

// Colors
Log("&3The &8red &yfox &bjumped &rover &4the &5fe&6n&7c&8e&9.");

// Color Codes
/*
 * Black         &0 | Dark Gray     &1 | Gray          &2 | Dark Magenta  &3
 * Dark Blue     &4 | Dark Cyan     &5 | Dark Green    &6 | Dark Yellow   &7
 * Dark Red      &8 | Red           &9 | Blue          &b | Cyan          &c
 * Green         &g | Magenta       &m | Yellow        &y | Reset         &r
 */
```

## Known Issues
Please see [Issues](https://github.com/Kittens-Rise-Up/server/issues)

## Contributing
Please see [CONTRIBUTING.md](https://github.com/Kittens-Rise-Up/server/blob/main/CONTRIBUTING.md)

Talk to `valk#9904` in the [Kittens Rise Up](https://discord.gg/cDNf8ja) discord for more info.
