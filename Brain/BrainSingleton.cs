using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Brain
{
    class BrainSingleton
    {
        static Mutex mut = null;
        const String mutexName = "Brain_Singleton";

        public static void TryCheck()
        {
            if (Mutex.TryOpenExisting(mutexName, out mut))
            {
                throw new Exception("Brain already run");
            }

            mut = new Mutex(true, mutexName);
        }
    }
}
