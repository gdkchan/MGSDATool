using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using DATMovieTool.IO;
using DATMovieTool.IO.Packet;

using MGSShared;

namespace DATMovieTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("DATMovieTool by gdkchan");
            Console.WriteLine("MGS movie.dat subtitle extractor/inserter");
            Console.WriteLine("Version 0.1.3");
            Console.ResetColor();
            Console.Write(Environment.NewLine);

            if (args.Length != 4)
            {
                PrintUsage();
                return;
            }
            else
            {
                MGSGame Game;
                switch (args[1])
                {
                    case "-mgs3": Game = MGSGame.MGS3; break;
                    case "-mgs4": Game = MGSGame.MGS4; break;
                    default: TextOut.PrintError("Invalid game \"" + args[1] + "\" specified!"); return;
                }

                switch (args[0])
                {
                    case "-e": Extract(args[2], args[3], Game); break;
                    case "-i": Insert(args[2], args[3], Game); break;
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

            Console.WriteLine("tool [command] [game] movie.dat [folder]");
            Console.Write(Environment.NewLine);

            Console.WriteLine("Examples:");
            Console.Write(Environment.NewLine);

            Console.WriteLine("tool -e -mgs4 movie.dat folder  Extracts subtitles from a MGS4 movie.dat file");
            Console.WriteLine("tool -i -mgs4 movie.dat folder  Creates the MGS4 movie.dat from a folder");
            Console.WriteLine("tool -e -mgs3 movie.dat folder  Extracts subtitles from a MGS3 movie.dat file");
            Console.WriteLine("tool -i -mgs3 movie.dat folder  Creates the MGS3 movie.dat from a folder");
        }

        public class MovieSubtitle
        {
            [XmlArrayItem("Subtitle")]
            public List<SubtitlePacket> Subtitles;

            public MovieSubtitle()
            {
                Subtitles = new List<SubtitlePacket>();
            }
        }

        /// <summary>
        ///     Extracts the subtitles from a movie.dat.
        /// </summary>
        /// <param name="Movie">The movie.dat file path</param>
        /// <param name="Output">The output folder</param>
        /// <param name="Game">The game being tampered (MGS3 or MGS4)</param>
        private static void Extract(string Movie, string Output, MGSGame Game)
        {
            Directory.CreateDirectory(Output);
            MGSText.Initialize();

            using (FileStream Input = new FileStream(Movie, FileMode.Open))
            {
                MovieSubtitle Out = new MovieSubtitle();
                EndianBinaryReader Reader = null;

                switch (Game)
                {
                    case MGSGame.MGS3: Reader = new EndianBinaryReader(Input, Endian.Little); break;
                    case MGSGame.MGS4: Reader = new EndianBinaryReader(Input, Endian.Big); break;
                }

                int Index = 0;
                while (Input.Position < Input.Length)
                {
                    StreamPacket Packet = StreamPacket.FromStream(Reader, Game);

                    switch (Packet.Type)
                    {
                        case PacketType.Subtitle: Out.Subtitles.Add((SubtitlePacket)Packet); break;
                        case PacketType.EndOfStream:
                            string XmlName = string.Format("Subtitle_{0:D5}.xml", Index++);
                            string FileName = Path.Combine(Output, XmlName);

                            XmlSerializerNamespaces NameSpaces = new XmlSerializerNamespaces();
                            NameSpaces.Add(string.Empty, string.Empty);
                            XmlWriterSettings Settings = new XmlWriterSettings
                            {
                                Encoding = Encoding.UTF8,
                                Indent = true
                            };

                            XmlSerializer Serializer = new XmlSerializer(typeof(MovieSubtitle));
                            using (FileStream OutputStream = new FileStream(FileName, FileMode.Create))
                            {
                                XmlWriter Writer = XmlWriter.Create(OutputStream, Settings);
                                Serializer.Serialize(Writer, Out, NameSpaces);
                            }

                            Out.Subtitles.Clear();
                            break;
                    }

                    ReportProgress((float)Input.Position / Input.Length);
                }
            }
        }

        /// <summary>
        ///     Inserts extracted subtitles into a movie.dat.
        /// </summary>
        /// <param name="Movie">The movie.dat file path</param>
        /// <param name="Input">The input folder with subtitles in XML</param>
        /// <param name="Game">The game being tampered (MGS3 or MGS4)</param>
        private static void Insert(string Movie, string Input, MGSGame Game)
        {
            string[] Files = Directory.GetFiles(Input);
            MGSText.Initialize();

            string NewFile = Path.GetTempFileName();
            FileStream In = new FileStream(Movie, FileMode.Open);
            FileStream Out = new FileStream(NewFile, FileMode.Create);

            Endian Endian = Endian.Default;
            switch (Game)
            {
                case MGSGame.MGS3: Endian = Endian.Little; break;
                case MGSGame.MGS4: Endian = Endian.Big; break;
            }

            EndianBinaryReader Reader = new EndianBinaryReader(In, Endian);
            EndianBinaryWriter Writer = new EndianBinaryWriter(Out, Endian);

            int Index = 0;
            int SubIndex = 0;
            MovieSubtitle Subtitle = GetSubtitle(Files[0]);
            while (In.Position < In.Length)
            {
                StreamPacket Packet = StreamPacket.FromStream(Reader, Game);

                switch (Packet.Type)
                {
                    case PacketType.Subtitle: SubtitlePacket.ToStream(Writer, Subtitle.Subtitles[SubIndex++], Game); break;
                    case PacketType.EndOfStream:
                        if (++Index < Files.Length) Subtitle = GetSubtitle(Files[Index]);
                        SubIndex = 0;
                        break;
                }

                if (Packet.Type != PacketType.Subtitle) StreamPacket.ToStream(Writer, Packet, Game);

                ReportProgress((float)In.Position / In.Length);
            }

            In.Close();
            Out.Close();

            File.Delete(Movie);
            File.Move(NewFile, Movie);
            File.Delete(NewFile);
        }

        private static MovieSubtitle GetSubtitle(string FileName)
        {
            XmlSerializer Deserializer = new XmlSerializer(typeof(MovieSubtitle));
            using (FileStream InputStream = new FileStream(FileName, FileMode.Open))
            {
                return (MovieSubtitle)Deserializer.Deserialize(InputStream);
            }
        }

        static bool FirstPercentage = true;
        private static void ReportProgress(float Percentage)
        {
            const int BarSize = 40;
            int Progress = (int)(Percentage * BarSize);

            if (FirstPercentage)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                for (int Index = 0; Index < BarSize; Index++) Console.Write(" ");
                Console.ResetColor();
                Console.CursorTop++;
                FirstPercentage = false;
            }

            Console.CursorTop--;

            if (Percentage > 0)
            {
                Console.CursorLeft = (int)(Percentage * (BarSize - 1));
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(" ");
                Console.ResetColor();
            }

            Console.CursorLeft = BarSize + 1;
            Console.WriteLine((int)(Percentage * 100) + "%");
        }

        public static void ClearLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
