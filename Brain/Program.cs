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
            /*if (args.Length < 1)
            {
                Console.WriteLine("Enter number of port (recommend from 10000 to 13000)");
                return;
            }*/
            Int32 port = 13000; // Convert.ToInt32(args[0]);

            Brain brain = new Brain(port);
            brain.Dream();
        }
    }
}
