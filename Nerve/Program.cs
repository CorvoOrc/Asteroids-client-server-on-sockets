using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Nerve
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter IPv4 adress:");
            String address = Console.ReadLine();
            Console.WriteLine("Enter port:");
            int port = Convert.ToInt32(Console.ReadLine());
            int portUdp = port + 10;

            Match match = Regex.Match(address, @"^((([0-9]{1,2})|(1[0-9]{2,2})|(2[0-4][0-9])|(25[0-5])|\*)\.){3}(([0-9]{1,2})|(1[0-9]{2,2})|(2[0-4][0-9])|(25[0-5])|\*)$");
            if (!match.Success)
            {
                Console.WriteLine("IPv4 address is wrong");
                Console.ReadKey();

                return;
            }

            int minPort = 0;
            int maxPort = 65535;
            if (port < minPort || port > maxPort)
            {
                Console.WriteLine("Port is wrong");
                Console.ReadKey();

                return;
            }

            try
            {
                Nerve nerve = new Nerve();
                nerve.Connect(address, port, portUdp);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
            }

            Console.ReadKey();
        }
    }
}

