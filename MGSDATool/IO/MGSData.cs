using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using MGSDATool.IO.Security;

namespace MGSDATool.IO
{
    /// <summary>
    ///     Metal Gear Solid DAT extraction/creation.
    /// </summary>
    class MGSData
    {
        const string Padding = "FON";
        const string TextSeparator = "\r\n\r\n";

        /// <summary>
        ///     Extracts the texts and other data inside the stage.dat file from MGS.
        /// </summary>
        /// <param name="Data">The full path to the file with the data to be extracted</param>
        /// <param name="OutputFolder">The output folder where the extracted data will be placed</param>
        public static void Extract(string Data, string OutputFolder)
        {
            using (MemoryStream Input = new MemoryStream(File.ReadAllBytes(Data)))
            {
                Extract(Input, OutputFolder);
            }
        }

        /// <summary>
        ///     Extracts the texts and other data inside the stage.dat file from MGS.
        /// </summary>
        /// <param name="Data">The Stream with the data to be extracted</param>
        /// <param name="OutputFolder">The output folder where the extracted data will be placed</param>
        public static void Extract(Stream Data, string OutputFolder)
        {
            BinaryReader Reader = new BinaryReader(Data);

            List<uint> AddressTable = new List<uint>();

            uint Delimiter = Reader.ReadUInt32();
            Data.Seek(0, SeekOrigin.Begin);
            while (Data.Position < Data.Length)
            {
                uint Position = (uint)Data.Position;
                uint Value = Reader.ReadUInt32();
                if (Value == Delimiter) AddressTable.Add(Position);
            }

            AddressTable.Add((uint)Data.Length);

            for (int Index = 0; Index < AddressTable.Count - 1; Index++)
            {
                string OutputPath = Path.Combine(OutputFolder, string.Format("Text_{0:D5}", Index));
                if (!Directory.Exists(OutputPath)) Directory.CreateDirectory(OutputPath);

                uint HeaderLength = 0;
                Data.Seek(AddressTable[Index], SeekOrigin.Begin);
                while (Reader.ReadInt32() != -1) HeaderLength += 4;
                uint BaseOffset = (uint)Data.Position;

                uint ScriptOffset = Reader.ReadUInt32() + BaseOffset;
                uint DialogsTableOffset = Reader.ReadUInt32() + BaseOffset;
                uint DialogsTextOffset = Reader.ReadUInt32() + BaseOffset;
                uint SectionLength = Reader.ReadUInt32();
                uint Key = Reader.ReadUInt32();

                MGSCrypto Crypto = new MGSCrypto(Key);
                StringBuilder Text = new StringBuilder();

                int TextIndex = 0;
                bool HasText = true;
                while (HasText)
                {
                    Data.Seek(DialogsTableOffset + TextIndex++ * 4, SeekOrigin.Begin);
                    uint Offset = (Reader.ReadUInt32() & 0x7fffffff) + DialogsTextOffset;
                    uint NextOffset = (Reader.ReadUInt32() & 0x7fffffff) + DialogsTextOffset;
                    if (Data.Position > DialogsTextOffset)
                    {
                        NextOffset = ScriptOffset;
                        HasText = false;
                    }

                    Data.Seek(Offset, SeekOrigin.Begin);
                    byte[] TextBuffer = new byte[NextOffset - Offset];
                    Reader.Read(TextBuffer, 0, TextBuffer.Length);
                    TextBuffer = Unpad(Crypto.ProcessBuffer(TextBuffer));

                    Text.Append(MGS2Text(Encoding.UTF8.GetString(TextBuffer)));
                    Text.Append(TextSeparator);
                }

                Data.Seek(ScriptOffset, SeekOrigin.Begin);
                byte[] Script = new byte[AddressTable[Index + 1] - ScriptOffset];
                Reader.Read(Script, 0, Script.Length);

                string HeaderFile = Path.Combine(OutputPath, "Header.bin");
                string TextFile = Path.Combine(OutputPath, "Text.txt");
                string ScriptFile = Path.Combine(OutputPath, "Script.bin");

                Data.Seek(AddressTable[Index], SeekOrigin.Begin);
                byte[] Header = new byte[HeaderLength];
                Reader.Read(Header, 0, Header.Length);
                File.WriteAllBytes(HeaderFile, Header);

                File.WriteAllText(TextFile, Text.ToString().TrimEnd());
                File.WriteAllBytes(ScriptFile, Script);
            }
        }

        private static string MGS2Text(string Text)
        {
            //Make text editable with notepad
            Text = Text.Replace(((char)0xa).ToString(), Environment.NewLine);
            Text = Text.Replace(((char)0).ToString(), "[end]");

            return Text;
        }

        private static byte[] Unpad(byte[] Data)
        {
            int Length = Data.Length;
            for (int Index = Data.Length - 1; Index >= 0; Index--)
            {
                if (Data[Index] != 0) Length--; else break;
            }

            byte[] NewData = new byte[Length];
            Buffer.BlockCopy(Data, 0, NewData, 0, Length);
            return NewData;
        }

        /// <summary>
        ///     Creates a stage.dat file from a folder with the extracted data.
        /// </summary>
        /// <param name="Data">The full path to the output file where the newly created data will be placed</param>
        /// <param name="InputFolder">The input folder with the extracted data</param>
        public static void Create(string Data, string InputFolder)
        {
            using (FileStream Output = new FileStream(Data, FileMode.Create))
            {
                Create(Output, InputFolder);
            }
        }

        /// <summary>
        ///     Creates a stage.dat file from a folder with the extracted data.
        /// </summary>
        /// <param name="Data">The output stream where the newly created data will be placed</param>
        /// <param name="InputFolder">The input folder with the extracted data</param>
        public static void Create(Stream Data, string InputFolder)
        {
            BinaryWriter Writer = new BinaryWriter(Data);

            string[] Folders = Directory.GetDirectories(InputFolder);

            foreach (string Folder in Folders)
            {
                string HeaderFile = Path.Combine(Folder, "Header.bin");
                string TextFile = Path.Combine(Folder, "Text.txt");
                string ScriptFile = Path.Combine(Folder, "Script.bin");

                if (File.Exists(HeaderFile) && File.Exists(TextFile) && File.Exists(ScriptFile))
                {
                    const uint Key = 0x4805d94d;
                    MGSCrypto Crypto = new MGSCrypto(Key);
                    Writer.Write(File.ReadAllBytes(HeaderFile));
                    Writer.Write(-1);

                    using (MemoryStream TextBlock = new MemoryStream())
                    {
                        BinaryWriter TextWriter = new BinaryWriter(TextBlock);

                        string Text = File.ReadAllText(TextFile);
                        string[] Texts = Text.Split(new string[] { TextSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        int BaseTextOffset = 0x14 + Texts.Length * 4;
                        int TextOffset = 0;

                        for (int Index = 0; Index < Texts.Length; Index++)
                        {
                            TextBlock.Seek(0x14 + Index * 4, SeekOrigin.Begin);
                            TextWriter.Write((uint)(TextOffset | 0x80000000));

                            TextBlock.Seek(BaseTextOffset + TextOffset, SeekOrigin.Begin);
                            byte[] Buffer = Encoding.UTF8.GetBytes(Text2MGS(Texts[Index]));
                            TextWriter.Write(Crypto.ProcessBuffer(Buffer));
                            TextOffset += Buffer.Length;
                        }

                        int PadIndex = 0;
                        while ((TextBlock.Position & 3) != 0)
                        {
                            byte Encrypted = Crypto.ProcessByte((byte)Padding[PadIndex++]);
                            TextBlock.WriteByte(Encrypted);
                        }

                        uint ScriptOffset = (uint)TextBlock.Position;
                        if (Texts.Length == 0) ScriptOffset = 0x14;

                        TextBlock.Seek(0, SeekOrigin.Begin);
                        TextWriter.Write(ScriptOffset);
                        TextWriter.Write(0x14);
                        TextWriter.Write(BaseTextOffset);
                        TextWriter.Write(ScriptOffset);
                        TextWriter.Write(Key);

                        Writer.Write(TextBlock.ToArray());
                    }

                    Writer.Write(File.ReadAllBytes(ScriptFile));
                    while ((Data.Position & 0xf) != 0) Data.WriteByte(0);
                }
            }
        }

        private static string Text2MGS(string Text)
        {
            //Undo the changes made by MGS2Text
            Text = Text.Replace(Environment.NewLine, ((char)0xa).ToString());
            Text = Text.Replace("[end]", ((char)0).ToString());

            return Text;
        }
    }
}
