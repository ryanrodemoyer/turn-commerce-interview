using System;
using System.Diagnostics;
using TurnCommerce.BizLogic;

namespace Exercise02.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Started at {DateTime.Now}");

            LargeFileSort lfs = new LargeFileSort();
            string answer = lfs.RunAsync().Result;

            Console.WriteLine(answer);

            Console.WriteLine($"{Environment.NewLine} Finished at {DateTime.Now}.");
            Console.ReadLine();
        }
    }
}
