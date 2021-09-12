using System;
using System.IO;
using System.Threading;
using GameServer.Logging;
using GameServer.Server;
using GameServer.Utilities;

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
            new Thread(ENetServer.WorkerThread).Start();
        }
    }
}
