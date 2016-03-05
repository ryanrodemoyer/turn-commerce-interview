using System;
using TurnCommerce.BizLogic;

namespace Exercise01.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Started at {DateTime.Now}");

            Sockets s = new Sockets();
            string answer = s.RunAsync().Result;

            Console.WriteLine(answer);

            Console.WriteLine($"{Environment.NewLine} Finished at {DateTime.Now}.");
            Console.ReadLine();
        }
    }
}
