using System.Timers;

namespace GameServer.Networking.Utils
{
    class GameTimer
    {
        private System.Timers.Timer timer;

        public GameTimer(double interval, ElapsedEventHandler function) 
        {
            timer = new System.Timers.Timer(interval);
            timer.Elapsed += function;
            timer.AutoReset = true;
        }

        public void Start() 
        {
            timer.Start();
        }

        public void Stop() 
        {
            timer.Stop();
        }

        public void Dispose() 
        {
            timer.Dispose();
        }
    }
}