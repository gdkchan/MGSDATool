using System;

using MGSDATool.IO;

namespace MGSDATool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("MGSDATool by gdkchan");
            Console.WriteLine("MGS codec.dat text extractor/creator");
            Console.WriteLine("Version 0.1.1");
            Console.ResetColor();
            Console.Write(Environment.NewLine);

            if (args.Length != 4)
            {
                PrintUsage();
                return;
            }
            else
            {
                MGSData.OnStatusChanged += StatusChangedCallback;

                MGSGame Game;
                switch (args[1])
                {
                    case "-mgs3": Game = MGSGame.MGS3; break;
                    case "-mgs4": Game = MGSGame.MGS4; break;
                    default: TextOut.PrintError("Invalid game \"" + args[1] + "\" specified!"); return;
                }

                switch (args[0])
                {
                    case "-e": MGSData.Extract(args[2], args[3], Game); break;
                    case "-c": MGSData.Create(args[3], args[2], Game); break;
                    default: TextOut.PrintError("Invalid command \"" + args[0] + "\" used!"); return;
                }
            }

            Console.Write(Environment.NewLine);
            TextOut.PrintSuccess("Finished!");
        }

        private static void PrintUsage()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Usage:");
            Console.ResetColor();
            Console.Write(Environment.NewLine);

            Console.WriteLine("tool [command] [game] [input] [output]");
            Console.Write(Environment.NewLine);

            Console.WriteLine("Examples:");
            Console.Write(Environment.NewLine);

            Console.WriteLine("tool -e -mgs4 codec.dat folder  Extracts texts from a MGS4 codec.dat file");
            Console.WriteLine("tool -c -mgs4 folder codec.dat  Creates the MGS4 codec.dat from a folder");
            Console.WriteLine("tool -e -mgs3 codec.dat folder  Extracts texts from a MGS3 codec.dat file");
            Console.WriteLine("tool -c -mgs3 folder codec.dat  Creates the MGS3 codec.dat from a folder");
        }

        static bool FirstPercentage = true;
        private static void StatusChangedCallback(float Percentage)
        {
            const int BarSize = 40;
            int Progress = (int)(Percentage * BarSize);

            if (FirstPercentage)
            {
                Console.Write("[");
                for (int Index = 0; Index < BarSize; Index++) Console.Write(" ");
                Console.WriteLine("]");
                FirstPercentage = false;
            }

            Console.CursorTop--;
            Console.CursorLeft = (int)(Percentage * (BarSize - 1)) + 1;
            Console.Write(">");
            Console.CursorLeft = BarSize + 3;
            Console.WriteLine((int)(Percentage * 100) + "%");
        }
    }
}
