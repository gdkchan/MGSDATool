using System;
using System.IO;
using System.Text;

namespace DATSpeechTool.IO
{
    /// <summary>
    ///     Handles the data from the Speech.dat file.
    /// </summary>
    class Data
    {
        const uint MGS4_TableOffset = 0xd63e3;

        /// <summary>
        ///     Extracts data from a Speech.dat.
        /// </summary>
        /// <param name="Scenerio">The path to the Scenerio.gcx file with the pointer table</param>
        /// <param name="Speech">The path to the Speech.dat file with the speech data</param>
        /// <param name="OutFolder">The output folder</param>
        public static void Extract(string Scenerio, string Speech, string OutFolder)
        {
            using (FileStream ScenerioIn = new FileStream(Scenerio, FileMode.Open))
            {
                using (FileStream SpeechIn = new FileStream(Speech, FileMode.Open))
                {
                    Extract(ScenerioIn, SpeechIn, OutFolder);
                }
            }
        }

        /// <summary>
        ///     Extracts data from a Speech.dat.
        /// </summary>
        /// <param name="Scenerio">The Stream of the Scenerio.gcx file with the pointer table</param>
        /// <param name="Speech">The Stream of the Speech.dat file with the speech data</param>
        /// <param name="OutFolder">The output folder</param>
        public static void Extract(Stream Scenerio, Stream Speech, string OutFolder)
        {
            Directory.CreateDirectory(OutFolder);
            BinaryReader Reader = new BinaryReader(Scenerio);

            Scenerio.Seek(MGS4_TableOffset, SeekOrigin.Begin);

            while (Scenerio.Position < Scenerio.Length)
            {
                string Name = GetName(Reader);
                if (Name == null) break;
                uint SPCOffset = GetValue(Reader);
                uint SPCLength = GetValue(Reader);
                uint SPCHeaderOffset = GetValue(Reader) + SPCOffset;
                switch (Reader.ReadByte()) //???
                {
                    case 1: Reader.ReadUInt16(); break;
                    case 2: Reader.ReadByte(); break;
                }

                Console.WriteLine("Extracting \"" + Name + ".spc\"...");

                string FileName = Path.Combine(OutFolder, Name + ".spc");
                Speech.Seek(SPCOffset, SeekOrigin.Begin);
                byte[] Buffer = new byte[SPCLength];
                Speech.Read(Buffer, 0, Buffer.Length);
                File.WriteAllBytes(FileName, Buffer);
            }
        }

        private static string GetName(BinaryReader Reader)
        {
            if (Reader.ReadByte() != 7) return null;
            byte NameLength = Reader.ReadByte();
            byte[] NameBuffer = new byte[NameLength];
            Reader.Read(NameBuffer, 0, NameBuffer.Length);
            Reader.ReadByte();

            return Encoding.UTF8.GetString(NameBuffer).TrimEnd('\0');
        }

        private static uint GetValue(BinaryReader Reader)
        {
            if (Reader.ReadByte() != 0xa) return 0;
            uint Value = Reader.ReadUInt32() << 11;

            return Value;
        }

        /// <summary>
        ///     Creates a Speech.dat from a folder.
        /// </summary>
        /// <param name="Scenerio">The path to the Scenerio.gcx file with the pointer table</param>
        /// <param name="Speech">The path to the Speech.dat file with the speech data</param>
        /// <param name="InFolder">The input folder</param>
        public static void Create(string Scenerio, string Speech, string InFolder)
        {
            using (FileStream ScenerioIn = new FileStream(Scenerio, FileMode.Open))
            {
                using (FileStream SpeechIn = new FileStream(Speech, FileMode.Open))
                {
                    Create(ScenerioIn, SpeechIn, InFolder);
                }
            }
        }

        /// <summary>
        ///     Creates a Speech.dat from a folder.
        /// </summary>
        /// <param name="Scenerio">The Stream of the Scenerio.gcx file with the pointer table</param>
        /// <param name="Spc">The Stream of the Speech.dat file with the speech data</param>
        /// <param name="InFolder">The input folder</param>
        public static void Create(Stream Scenerio, Stream Spc, string InFolder)
        {
            BinaryReader Reader = new BinaryReader(Scenerio);
            BinaryWriter Writer = new BinaryWriter(Scenerio);

            Scenerio.Seek(MGS4_TableOffset, SeekOrigin.Begin);
            Spc.SetLength(0);

            while (Scenerio.Position < Scenerio.Length)
            {
                string Name = GetName(Reader);
                if (Name == null) break;

                string FileName = Path.Combine(InFolder, Name + ".spc");

                if (File.Exists(FileName))
                {
                    byte[] Buffer = File.ReadAllBytes(FileName);

                    SetValue(Writer, (uint)Spc.Position);
                    SetValue(Writer, (uint)Buffer.Length);
                    SetValue(Writer, Speech.GetHeaderPosition(Buffer));

                    Console.WriteLine("Inserting \"" + Name + ".spc\"...");

                    Spc.Write(Buffer, 0, Buffer.Length);
                }
                else
                {
                    uint SPCOffset = GetValue(Reader);
                    uint SPCLength = GetValue(Reader);
                    uint SPCHeaderOffset = GetValue(Reader) + SPCOffset;
                }

                switch (Reader.ReadByte()) //???
                {
                    case 1: Reader.ReadUInt16(); break;
                    case 2: Reader.ReadByte(); break;
                }
            }
        }

        private static void SetValue(BinaryWriter Writer, uint Value)
        {
            Writer.Write((byte)0xa);
            Writer.Write(Value >> 11);
        }
    }
}
