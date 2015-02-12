using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Brain
{
    class BrainSynchro
    {
        EventWaitHandle[] waitHandles = null;

        ManualResetEvent nervesEvent = null;

        public BrainSynchro()
        {
            waitHandles = new EventWaitHandle[] 
            {
                new ManualResetEvent(true), // sendtoallLock
                new ManualResetEvent(true), // shipsLock
                new ManualResetEvent(true), // asteroidLock
                new ManualResetEvent(true), // bulletLock
                new ManualResetEvent(true), // shipSocketLock
                //new ManualResetEvent(true)  // nervesLock
            };

            nervesEvent = new ManualResetEvent(true);
        }

        public bool Set(GroupLock group)
        {
            if (group == GroupLock.nervesLock)
            {
                return nervesEvent.Set();
            }

            return waitHandles[group.GetHashCode()].Set();            
        }

        public bool Reset(GroupLock group)
        {
            if (group == GroupLock.nervesLock)
            {
                return nervesEvent.Reset();
            }

            return waitHandles[group.GetHashCode()].Reset();  
        }

        public bool SetAll()
        {
            bool flag = true;
            foreach(var handle in waitHandles)
            {
                if (!handle.Set())
                {
                    flag = false;
                }
            }

            return flag;
        }

        public bool ResetAll()
        {
            bool flag = true;
            foreach (var handle in waitHandles)
            {
                if (!handle.Reset())
                {
                    flag = false;
                }
            }

            return flag;
        }

        public bool WaitAll()
        {
            return EventWaitHandle.WaitAll(waitHandles);
        }

        public bool WaitOne(GroupLock group)
        {
            if (group == GroupLock.nervesLock)
            {
                return nervesEvent.WaitOne();
            }

            return waitHandles[group.GetHashCode()].WaitOne();
        }
    }

    public enum GroupLock : byte
    {
        sendtoallLock,
        shipsLock,
        asteroidsLock,
        bulletsLock,
        shipSocketBindLock,
        nervesLock
    }
}
