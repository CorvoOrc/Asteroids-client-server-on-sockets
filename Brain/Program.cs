using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Don`t entered number of port (recommend from 10000 to 13000)");
                return;
            }
            
            Int32 port = Convert.ToInt32(args[0]);

            int minPort = 0;
            int maxPort = 65535;
            if (port < minPort || port > maxPort)
            {
                return;
            }

            Console.WriteLine("Entered port {0}", port);

            try
            {
                Brain brain = new Brain(port);
                brain.Dream();
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }
    }
}
