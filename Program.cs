using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Console;
using GameServer.Server;
using GameServer.Utils;
using MongoDB.Driver;

namespace GameServer
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Parallel.Invoke(
                () => Logger.InputThread(), 
                () => Logger.MessagesThread(), 
                () => ENetServer.ENetThreadWorker(25565, 100));
        }
    }
}
