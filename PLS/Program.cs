using System;

namespace PLS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Application.Start(args);
            }
            catch (Exception e)
            {
                PrintError(e);
            }
        }

        private static void PrintError(Exception e)
        {
            var cur = e;
            var level = 0;
            while (cur != null)
            {
                Console.WriteLine($@"[[ {level++} ]]======================================>");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(cur.Message);
                Console.ResetColor();
                Console.WriteLine("----------- STACK -----------");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(cur.StackTrace);
                Console.ResetColor();
                cur = cur.InnerException;
            }
        }
    }
}
