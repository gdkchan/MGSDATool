using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using MGSDATool.IO.Security;
using MGSDATool.Properties;

namespace MGSDATool.IO
{
    /// <summary>
    ///     Metal Gear Solid DAT extraction/creation.
    /// </summary>
    class MGSData
    {
        const string Padding = "FON";
        const string TextSeparator = "\r\n------\r\n";

        public delegate void StatusChanged(float Percentage);
        public static event StatusChanged OnStatusChanged;

        private static string[] Table;

        /// <summary>
        ///     Extracts the texts and other data inside the codec.dat file from MGS.
        /// </summary>
        /// <param name="Data">The full path to the file with the data to be extracted</param>
        /// <param name="OutputFolder">The output folder where the extracted data will be placed</param>
        public static void Extract(string Data, string OutputFolder, MGSGame Game)
        {
            using (MemoryStream Input = new MemoryStream(File.ReadAllBytes(Data)))
            {
                Extract(Input, OutputFolder, Game);
            }
        }

        /// <summary>
        ///     Extracts the texts and other data inside the codec.dat file from MGS.
        /// </summary>
        /// <param name="Data">The Stream with the data to be extracted</param>
        /// <param name="OutputFolder">The output folder where the extracted data will be placed</param>
        public static void Extract(Stream Data, string OutputFolder, MGSGame Game)
        {
            if (Game == MGSGame.MGS3) Table = GetTable();
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
                uint ScriptHeaderOffset = Reader.ReadUInt32() + BaseOffset;
                uint Key = Reader.ReadUInt32();

                //Read header (with Script pointers)
                Data.Seek(AddressTable[Index], SeekOrigin.Begin);
                byte[] Header = new byte[HeaderLength];
                Reader.Read(Header, 0, Header.Length);

                MGSCrypto Crypto = new MGSCrypto(Key);
                StringBuilder Text = new StringBuilder();

                //Load texts
                if (DialogsTableOffset != DialogsTextOffset)
                {
                    int TextIndex = 0;
                    bool HasText = true;
                    while (HasText)
                    {
                        Data.Seek(DialogsTableOffset + TextIndex++ * 4, SeekOrigin.Begin);
                        uint Offset = (Reader.ReadUInt32() & 0x7fffffff) + DialogsTextOffset;
                        uint NextOffset = (Reader.ReadUInt32() & 0x7fffffff) + DialogsTextOffset;
                        if (Data.Position > DialogsTextOffset)
                        {
                            NextOffset = ScriptHeaderOffset;
                            HasText = false;
                        }

                        Data.Seek(Offset, SeekOrigin.Begin);
                        byte[] TextBuffer = new byte[NextOffset - Offset];
                        Reader.Read(TextBuffer, 0, TextBuffer.Length);
                        TextBuffer = UnpadText(Crypto.ProcessBuffer(TextBuffer));

                        Text.Append(Buffer2Text(TextBuffer, Game));
                        Text.Append(TextSeparator);
                    }
                }

                if (Game == MGSGame.MGS3)
                {
                    //This is probably not a header, but I don't know what this data is, so...
                    string ScriptHeaderFile = Path.Combine(OutputPath, "ScriptHeader.bin");

                    Data.Seek(ScriptHeaderOffset, SeekOrigin.Begin);
                    uint ScriptHeaderLength = ScriptOffset - ScriptHeaderOffset;
                    byte[] ScriptHeader = new byte[ScriptHeaderLength];
                    Reader.Read(ScriptHeader, 0, ScriptHeader.Length);

                    File.WriteAllBytes(ScriptHeaderFile, ScriptHeader);
                }

                //Load script
                Data.Seek(ScriptOffset, SeekOrigin.Begin);
                uint ScriptLength = AddressTable[Index + 1] - ScriptOffset;
                byte[] Script = new byte[ScriptLength];
                Reader.Read(Script, 0, Script.Length);

                string HeaderFile = Path.Combine(OutputPath, "Header.bin");
                string TextFile = Path.Combine(OutputPath, "Text.txt");
                string ScriptFile = Path.Combine(OutputPath, "Script.bin");

                File.WriteAllBytes(HeaderFile, Header);
                File.WriteAllText(TextFile, Text.ToString());
                File.WriteAllBytes(ScriptFile, Script);

                if (OnStatusChanged != null)
                {
                    float Percentage = (float)(Index + 1) / (AddressTable.Count - 1);
                    OnStatusChanged(Percentage);
                }
            }
        }

        private static string Buffer2Text(byte[] Data, MGSGame Game)
        {
            switch (Game)
            {
                case MGSGame.MGS3:
                    StringBuilder Output = new StringBuilder();

                    for (int Index = 0; Index < Data.Length; Index++)
                    {
                        if (Data[Index] < 0x20 || Data[Index] > 0x7e)
                        {
                            if (Data[Index] == 0xa)
                                Output.Append(Environment.NewLine);
                            else if (Data[Index] == 0)
                                Output.Append("[end]");
                            else
                            {
                                int Value = (Data[Index] << 8) | Data[++Index];

                                if (Table[Value] == null)
                                    Output.Append("\\x" + Value.ToString("X4"));
                                else
                                    Output.Append(Table[Value]);
                            }
                        }
                        else
                            Output.Append((char)Data[Index]);
                    }

                    return Output.ToString();

                case MGSGame.MGS4: return MGS2Text(Encoding.UTF8.GetString(Data));
            }

            return null;
        }

        private static string MGS2Text(string Text)
        {
            //Make text editable with notepad
            Text = Text.Replace(((char)0xa).ToString(), Environment.NewLine);
            Text = Text.Replace(((char)0).ToString(), "[end]");

            return Text;
        }

        private static byte[] UnpadText(byte[] Data)
        {
            int Length = Data.Length;
            while (Length > 0 && Data[--Length] != 0);
            return ResizeBuffer(Data, Length + 1);
        }

        private static byte[] ResizeBuffer(byte[] Data, int NewSize)
        {
            byte[] NewData = new byte[NewSize];
            Buffer.BlockCopy(Data, 0, NewData, 0, NewSize);
            return NewData;
        }

        /// <summary>
        ///     Creates a codec.dat file from a folder with the extracted data.
        /// </summary>
        /// <param name="Data">The full path to the output file where the newly created data will be placed</param>
        /// <param name="InputFolder">The input folder with the extracted data</param>
        public static void Create(string Data, string InputFolder, MGSGame Game)
        {
            using (FileStream Output = new FileStream(Data, FileMode.Create))
            {
                Create(Output, InputFolder, Game);
            }
        }

        /// <summary>
        ///     Creates a codec.dat file from a folder with the extracted data.
        /// </summary>
        /// <param name="Data">The output stream where the newly created data will be placed</param>
        /// <param name="InputFolder">The input folder with the extracted data</param>
        public static void Create(Stream Data, string InputFolder, MGSGame Game)
        {
            if (Game == MGSGame.MGS3) Table = GetTable();
            BinaryWriter Writer = new BinaryWriter(Data);

            string[] Folders = Directory.GetDirectories(InputFolder);

            int CurrentFolder = 0;
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

                    byte[] ScriptHeader = null;
                    uint ScriptHeaderLength = 0;
                    if (Game == MGSGame.MGS3)
                    {
                        string ScriptHeaderFile = Path.Combine(Folder, "ScriptHeader.bin");

                        if (File.Exists(ScriptHeaderFile))
                        {
                            ScriptHeader = File.ReadAllBytes(ScriptHeaderFile);
                            ScriptHeaderLength = (uint)ScriptHeader.Length;
                        }
                    }

                    using (MemoryStream TextBlock = new MemoryStream())
                    {
                        BinaryWriter TextWriter = new BinaryWriter(TextBlock);

                        string Text = File.ReadAllText(TextFile);
                        string[] Texts = Text.Split(new string[] { TextSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        int BaseTextOffset = 0x14 + Texts.Length * 4;
                        int TextOffset = 0;

                        TextBlock.Seek(BaseTextOffset, SeekOrigin.Begin);
                        for (int Index = 0; Index < Texts.Length; Index++)
                        {
                            TextBlock.Seek(0x14 + Index * 4, SeekOrigin.Begin);
                            TextWriter.Write((uint)(TextOffset | 0x80000000));

                            TextBlock.Seek(BaseTextOffset + TextOffset, SeekOrigin.Begin);
                            byte[] Buffer = Text2Buffer(Texts[Index], Game);
                            TextWriter.Write(Crypto.ProcessBuffer(Buffer));
                            TextOffset += Buffer.Length;
                        }

                        int PadIndex = 0;
                        while ((TextBlock.Position & 3) != 0)
                        {
                            byte Encrypted = Crypto.ProcessByte((byte)Padding[PadIndex++]);
                            TextBlock.WriteByte(Encrypted);
                        }

                        uint ScriptHeaderOffset = (uint)TextBlock.Position;
                        uint ScriptOffset = ScriptHeaderOffset + ScriptHeaderLength;

                        TextBlock.Seek(0, SeekOrigin.Begin);
                        TextWriter.Write(ScriptOffset);
                        TextWriter.Write(0x14);
                        TextWriter.Write(BaseTextOffset);
                        TextWriter.Write(ScriptHeaderOffset);
                        TextWriter.Write(Key);

                        Writer.Write(TextBlock.ToArray());
                    }

                    if (ScriptHeaderLength > 0) Writer.Write(ScriptHeader);
                    Writer.Write(File.ReadAllBytes(ScriptFile));

                    while ((Data.Position & 0xf) != 0) Data.WriteByte(0);
                }

                if (OnStatusChanged != null)
                {
                    float Percentage = (float)++CurrentFolder / Folders.Length;
                    OnStatusChanged(Percentage);
                }
            }
        }

        private static byte[] Text2Buffer(string Text, MGSGame Game)
        {
            switch (Game)
            {
                case MGSGame.MGS3:
                    Text = Text2MGS(Text);

                    using (MemoryStream Output = new MemoryStream())
                    {
                        for (int Index = 0; Index < Text.Length; Index++)
                        {
                            if (Index < Text.Length - 1 && Text.Substring(Index, 2) == "\\x")
                            {
                                string Hex = Text.Substring(Index + 2, 4);
                                ushort Value = ushort.Parse(Hex, NumberStyles.HexNumber);
                                Output.WriteByte((byte)(Value >> 8));
                                Output.WriteByte((byte)Value);
                                Index += 5;
                            }
                            else
                            {
                                bool InRange = Text[Index] < 0x20 || Text[Index] > 0x7e;
                                if (InRange && Text[Index] != 0 && Text[Index] != 0xa)
                                {
                                    int Value = Array.IndexOf(Table, Text.Substring(Index, 1));

                                    if (Value > -1)
                                    {
                                        Output.WriteByte((byte)(Value >> 8));
                                        Output.WriteByte((byte)Value);
                                    }
                                }
                                else
                                    Output.WriteByte((byte)Text[Index]);
                            }
                        }

                        return Output.ToArray();
                    }

                case MGSGame.MGS4: return Encoding.UTF8.GetBytes(Text2MGS(Text));
            }

            return null;
        }

        private static string Text2MGS(string Text)
        {
            //Undo the changes made by MGS2Text
            Text = Text.Replace(Environment.NewLine, ((char)0xa).ToString());
            Text = Text.Replace("[end]", ((char)0).ToString());

            return Text;
        }

        private static string[] GetTable()
        {
            string[] Table = new string[0x10000];
            string[] LineBreaks = new string[] { "\n", "\r\n" };
            string[] TableElements = Resources.CharacterTable.Split(LineBreaks, StringSplitOptions.RemoveEmptyEntries);

            foreach (string Element in TableElements)
            {
                int Position = Element.IndexOf("=");
                int Value = Convert.ToInt32(Element.Substring(0, Position), 16);
                string Character = Element.Substring(Position + 1, Element.Length - Position - 1);

                Table[Value] = Character;
            }

            return Table;
        }
    }
}
