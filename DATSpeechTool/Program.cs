using System;
using System.IO;

using DATSpeechTool.IO;

namespace DATSpeechTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DATSpeechTool by gdkchan");
            Console.WriteLine("MGS4 speech.dat subtitle extractor/inserter");
            Console.WriteLine("Version 0.2.0");
            Console.ResetColor();
            Console.Write(Environment.NewLine);

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }
            else
            {
                switch (args[0])
                {
                    case "-xdat": Data.Extract(args[1], args[2], args[3]); break;
                    case "-cdat": Data.Create(args[1], args[2], args[3]); break;
                    case "-xspc": Speech.Extract(args[1], args[2]); break;
                    case "-ispc": Speech.Insert(args[1], args[2]); break;
                    case "-xspcall":
                        string[] SPCFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.spc");
                        foreach (string SPCFile in SPCFiles)
                        {
                            string OutFile = SPCFile.Replace(".spc", ".xml");
                            Speech.Extract(SPCFile, OutFile);
                        }
                        break;
                    case "-ispcall":
                        string[] XMLFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.xml");
                        foreach (string XMLFile in XMLFiles)
                        {
                            string OutFile = XMLFile.Replace(".xml", ".spc");
                            Speech.Insert(OutFile, XMLFile);
                        }
                        break;
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

            Console.WriteLine("tool -xdat scenerio.gcx speech.dat out_folder  Extracts DAT");
            Console.WriteLine("tool -cdat scenerio.gcx speech.dat in_folder  Creates DAT");
            Console.WriteLine("tool -xspc file.spc output.xml  Extracts subtitles from SPC");
            Console.WriteLine("tool -ispc file.spc input.xml  Inserts subtitles into SPC");
            Console.WriteLine("tool -xspcall  Extracts subtitles from all SPCs on current dir");
            Console.WriteLine("tool -ispcall  Inserts subtitles into all SPCs on current dir");
        }
    }
}
