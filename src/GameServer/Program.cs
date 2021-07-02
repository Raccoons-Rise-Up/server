using System.Threading;

using GameServer.Logging;
using GameServer.Networking;

namespace GameServer
{
    class Program
    {
        public static Server Server;

        public static void Main(string[] args)
        {
            new Thread(new Logger().Start).Start(); // Initialize console on thread 1
        }
    }
}