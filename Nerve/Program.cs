using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Nerve
{
    class Program
    {
        static void Main(string[] args)
        {
            String address = "109.229.50.139";
            int port = 13000;
            int portUdp = port + 10;
            Nerve nerve = new Nerve();
            nerve.Connect(address, port, portUdp);

            Console.ReadKey();
        }
    }
}
