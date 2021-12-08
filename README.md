# Game-Server
## Table of Contents
1. [About](#what-is-this)
2. [Setup](#setup)
4. [Server](#server)
5. [Console](#console)
    - [Features](#features-1)
    - [Controls](#controls)
    - [Commands](#commands)
    - [Logging](#logging)
6. [Issues](#issues)
7. [Contributing](#contributing)

## What is this?
A multi-threaded console / game server that handles logging, user commands, database and ENet connections for Raccoons Rise Up clients.

I have tried using several networking libs in the past, none of them worked out for me.
- Unity's 2019 LLMAPI has little documentation and unexplained behavior
- Godot's server-client model requires Godot to be built from open source which led me to several issues
- Mirror has to be created within Unity 2019 LTS, it cannot be used in a dotnet console app

The only networking lib that stood out to me is [ENet-CSharp](https://github.com/nxrighthere/ENet-CSharp), I find it to be very simple and straight forward. Since the original repository has their issues closed, you'll find that [this repository](https://github.com/SoftwareGuy/ENet-CSharp) has theirs open and there are many people willing to help you with any questions you may have.

I previously created a [game-server](https://github.com/The-MMORPG-Project/game-server) for [The MMORPG Project](https://github.com/The-MMORPG-Project/website) but it did not follow thread safety and depended on an external API for the console. This game-server follows thread safety and does not depend on any external API for the console.

[Raccoons Rise Up Roadmap](https://trello.com/b/XkhJxR2x/raccoons-rise-up)

## Setup
.NET is installed from https://dotnet.microsoft.com/download/dotnet/5.0 (cross platform) or with Visual Studio if you check the `.NET desktop environment` workload when installing.

### Public JWT Key
Create a `public.key` in `obj\Debug\net5.0` (if you don't see these folders you need to run the project at least once with `dotnet run`) with same key generated from [web-server](https://github.com/Kittens-Rise-Up/website)

Run the project with `dotnet run`

## Server
### Features
- ENet-CSharp Networking Lib
- JSON files to read / write from

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
| Command                             | Aliases     |                                                                        | Finished |
|-------------------------------------|-----------------|--------------------------------------------------------------------|----------|
| `help [cmd]`                        | `h`             | Show a list of helpful commands or info on a specific command.     | Yes      |
| `clear`                             | `cls`           | Clear the console.                                                 | Yes      |
| `exit`                              | `stop`, `quit`  | Shutdown the server properly by saving everything before exiting.  | Yes      |
| `reset <player>`                    |                 | Clear player from database or clear the entire database.           | Yes      |
| `whois <player>`                    | `who`           | Get info about a player.                                           | Yes      |
| `kick <player>`                     |                 | Kick a player.                                                     | Yes      |
| `ban <player>`                      |                 | Ban a player.                                                      | Yes      |
| `unban <player>`                    | `pardon`        | Pardon a player.                                                   | Yes      |
| `banlist`                           |                 | List all banned players.                                           | Yes      |
| `list`                              |                 | List all currently connected players.                              | Yes      |
| `whisper <player> <message>`        | `w`             | Send a private message to a player.                                | No       |
| `broadcast <message>`               | `announce`      | Broadcast a message to every player in the server.                 | No       |
| `reply <message>`                   | `r`             | Reply to the most recent player that messaged the console.         | No       |
| `setperm <permLevel> <player>`      |                 | Set one or more players permission level.                          | No       |
| `whitelist <add \| remove \| enable \| disable \| list> [player]` |             | Whitelist management.                    | No       |

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

## Issues
Please see the projects [current issues](https://github.com/Kittens-Rise-Up/server/issues)

## Contributing
Please see [CONTRIBUTING.md](https://github.com/Kittens-Rise-Up/server/blob/main/CONTRIBUTING.md)

Not all the things to do are listed in the [issues](https://github.com/Raccoons-Rise-Up/server/issues), if you want to know more of what needs to be done please talk to `valk#9904` in the [Raccoons Rise Up](https://discord.gg/cDNf8ja) discord for more info.
