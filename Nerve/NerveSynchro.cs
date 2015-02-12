using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Nerve
{
    class NerveSynchro
    {
        EventWaitHandle[] waitHandles = null;

        public NerveSynchro()
        {
            waitHandles = new EventWaitHandle[] 
            {
                new ManualResetEvent(true), // shipsLock
                new ManualResetEvent(true), // asteroidsLock
                new ManualResetEvent(true), // ownBulletsLock
                new ManualResetEvent(true), // alienBulletsLock
            };
        }

        public bool Set(GroupLock group)
        {
            return waitHandles[group.GetHashCode()].Set();            
        }

        public bool Reset(GroupLock group)
        {
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
            return waitHandles[group.GetHashCode()].WaitOne();
        }
    }

    public enum GroupLock : byte
    {
        shipsLock,
        asteroidsLock,
        ownBulletsLock,
        alienBulletsLock
    }
}
