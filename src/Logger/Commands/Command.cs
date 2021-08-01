using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public abstract class Command
    {
        public virtual void Run(string[] args)
        {
            Logger.LogWarning("Unimplemented command.");
        }
    }
}
