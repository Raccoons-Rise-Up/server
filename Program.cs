using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Console;
using GameServer.Server;
using GameServer.Utils;

namespace GameServer
{
    public class Program
    {
        private static void Main(string[] args)
        {
            StartLogger();
            StartServer();
        }

        private static void StartLogger() 
        {
            new Thread(Logger.InputThread).Start();
            new Thread(Logger.MessagesThread).Start();
        }

        public static void StartServer() 
        {
            new Thread(() => ENetServer.ENetThreadWorker(25565, 100)).Start();
        }
    }
}
