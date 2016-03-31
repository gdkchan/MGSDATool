using System;

namespace DATMovieTool
{
    class TextOut
    {
        public static void Print(string Message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Message);
            Console.ResetColor();
        }

        public static void PrintSuccess(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Message);
            Console.ResetColor();
        }

        public static void PrintWarning(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Message);
            Console.ResetColor();
        }

        public static void PrintError(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Message);
            Console.ResetColor();
        }
    }
}
