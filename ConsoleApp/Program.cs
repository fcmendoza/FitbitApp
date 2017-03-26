using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.ResetColor();
                new FitbitService().Start();
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //Console.WriteLine("\nDone. Press any key to exit.");
            //Console.ReadLine();
        }
    }
}
