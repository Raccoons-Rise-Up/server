# Server

## What is this?
A multi-threaded console / game server that handles logging, user commands and ENet connections for Kittens Rise Up clients.

I previously created a [game-server](https://github.com/The-MMORPG-Project/game-server) for [The MMORPG Project](https://github.com/The-MMORPG-Project/website) but it did not follow thread safety and depended on an external API for the UI. This game-server follows thread safety and does not depend on any external API for the console.

## Features
- Console logs messages from several threads and listens for user commands at the same time
- Each character can have its own unique color (limited to the colors defined by `System.ConsoleColor`)
- Each message shows a timestamp, the name of the thread and the log level
- Delete characters with backspace, use left and right arrow keys for more control, navigate through command history with up and down arrow keys

## Commands
`help [cmd]` - Display a list of helpful commands or get specific information about a specified command.  
`exit` - Allow the server to shutdown properly and then exit the application.  

## Logging
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
