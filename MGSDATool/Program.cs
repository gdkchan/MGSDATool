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
            Console.WriteLine("MGS4 codec.dat text extractor/creator");
            Console.WriteLine("Version 0.1.0 Internal Test #2");
            Console.ResetColor();
            Console.Write(Environment.NewLine);

            if (args.Length < 3)
            {
                PrintUsage();
                return;
            }
            else
            {
                switch (args[0])
                {
                    case "-e": MGSData.Extract(args[1], args[2]); break;
                    case "-c": MGSData.Create(args[2], args[1]); break;
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

            Console.WriteLine("tool -e codec.dat folder  Extracts texts from a codec.dat file");
            Console.WriteLine("tool -c folder codec.dat  Creates the codec.dat from a folder");
        }
    }
}
