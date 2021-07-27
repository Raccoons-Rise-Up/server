using System;
using System.Threading;
using System.Linq;

namespace GameServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            StartLogger();
        }

        private static void StartLogger() 
        {
            new Thread(Logger.WorkerThread).Start();
        }

        public static void StartServer() 
        {
            new Thread(Server.WorkerThread).Start();
        }
    }
}
