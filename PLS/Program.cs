using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace PLS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Application.Parse(args);
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
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                Console.WriteLine("----------- STACK -----------");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.StackTrace);
                Console.ResetColor();
                cur = cur.InnerException;
            }
        }
    }
}
