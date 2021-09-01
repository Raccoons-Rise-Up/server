using System;
using System.Threading;
using System.Linq;
using GameServer.Logging;
using GameServer.Server;

namespace GameServer
{
    public class Program
    {
        private static void Main(string[] args)
        {
            StartLogger();
            StartServer();
            #if Linux 
            Logger.Log("Built on Linux!"); 
            #elif OSX 
            Logger.Log("Built on macOS!"); 
            #elif Windows 
            Logger.Log("Built in Windows!"); 
            #endif
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
