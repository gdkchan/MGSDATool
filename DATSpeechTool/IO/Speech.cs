using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using DATSpeechTool.IO.Packet;

namespace DATSpeechTool.IO
{
    /// <summary>
    ///     Metal Gear Solid 4 speech.dat subtitle handling.
    /// </summary>
    public class Speech
    {
        /// <summary>
        ///     Represents a speech dialog with one or more subtitle segments.
        /// </summary>
        public class SpeechDialog
        {
            [XmlArrayItem("Subtitle")]
            public List<SubtitlePacket> Subtitles;

            public SpeechDialog()
            {
                Subtitles = new List<SubtitlePacket>();
            }
        }
        /// <summary>
        ///     Represents a group of subtitles.
        /// </summary>
        public class SpeechSubtitle
        {
            [XmlArrayItem("Dialog")]
            public List<SpeechDialog> Dialogs;

            public SpeechSubtitle()
            {
                Dialogs = new List<SpeechDialog>();
            }
        }

        /// <summary>
        ///     Extracts the subtitles from a *.spc file to a *.xml file.
        /// </summary>
        /// <param name="Speech">The input *.spc file path</param>
        /// <param name="OutFile">The output *.xml file path</param>
        public static void Extract(string Speech, string OutFile)
        {
            using (FileStream Input = new FileStream(Speech, FileMode.Open))
            {
                Extract(Input, OutFile);
            }
        }

        /// <summary>
        ///     Extracts the subtitles from a *.spc file to a *.xml file.
        /// </summary>
        /// <param name="Speech">The input *.spc file Stream</param>
        /// <param name="OutFile">The output *.xml file path</param>
        public static void Extract(Stream Speech, string OutFile)
        {
            SpeechSubtitle Output = new SpeechSubtitle();
            EndianBinaryReader Reader = new EndianBinaryReader(Speech, Endian.Big);

            int Index = 0;
            uint HeaderOffset = GetHeaderPosition(Reader);
            while (Speech.Position < Speech.Length)
            {
                Speech.Seek(HeaderOffset + Index * 0x20, SeekOrigin.Begin);

                uint Id = Reader.ReadUInt32();
                uint WaveOffset = Reader.ReadUInt32() << 11;
                uint WaveLength = Reader.ReadUInt32() << 11;
                float WaveSynchro = Reader.ReadSingle();
                uint SubtitleOffset = Reader.ReadUInt32() + WaveOffset;
                uint SubtitleLength = Reader.ReadUInt32();
                uint TableLength = Reader.ReadUInt32();

                if (Id == 0 || Speech.Position >= Speech.Length) break;

                if (SubtitleLength > 0)
                {
                    SpeechDialog Dialog = new SpeechDialog();
                    Speech.Seek(SubtitleOffset, SeekOrigin.Begin);

                    while (true)
                    {
                        SubtitlePacket Subtitle = SubtitlePacket.FromStream(Reader);
                        if (Subtitle != null) Dialog.Subtitles.Add(Subtitle);
                        if (Speech.Position >= SubtitleOffset + SubtitleLength || Subtitle == null) break;
                    }

                    Output.Dialogs.Add(Dialog);
                }

                Index++;
            }

            XmlSerializerNamespaces NameSpaces = new XmlSerializerNamespaces();
            NameSpaces.Add(string.Empty, string.Empty);
            XmlWriterSettings Settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            XmlSerializer Serializer = new XmlSerializer(typeof(SpeechSubtitle));
            using (FileStream OutputStream = new FileStream(OutFile, FileMode.Create))
            {
                XmlWriter Writer = XmlWriter.Create(OutputStream, Settings);
                Serializer.Serialize(Writer, Output, NameSpaces);
            }
        }

        /// <summary>
        ///     Inserts the subtitles from a *.xml into a *.spc file.
        /// </summary>
        /// <param name="Speech">The output *.spc file path</param>
        /// <param name="InFile">The input *.xml file path</param>
        public static void Insert(string Speech, string InFile)
        {
            using (FileStream Input = new FileStream(Speech, FileMode.Open))
            {
                Insert(Input, InFile);
            }
        }

        /// <summary>
        ///     Inserts the subtitles from a *.xml into a *.spc file.
        /// </summary>
        /// <param name="Speech">The output *.spc file Stream</param>
        /// <param name="InFile">The input *.xml file path</param>
        public static void Insert(Stream Speech, string InFile)
        {
            SpeechSubtitle Input;
            XmlSerializer Deserializer = new XmlSerializer(typeof(SpeechSubtitle));
            using (FileStream InputStream = new FileStream(InFile, FileMode.Open))
            {
                Input = (SpeechSubtitle)Deserializer.Deserialize(InputStream);
            }

            EndianBinaryReader Reader = new EndianBinaryReader(Speech, Endian.Big);

            using (MemoryStream Output = new MemoryStream())
            {
                MemoryStream Header = new MemoryStream();
                EndianBinaryWriter Writer = new EndianBinaryWriter(Header, Endian.Big);

                int Index = 0;
                int SubIndex = 0;
                long DataOffset = 0;
                uint HeaderOffset = GetHeaderPosition(Reader);
                while (Speech.Position < Speech.Length)
                {
                    Speech.Seek(HeaderOffset + Index * 0x20, SeekOrigin.Begin);

                    uint Id = Reader.ReadUInt32();
                    uint WaveOffset = Reader.ReadUInt32() << 11;
                    uint WaveLength = Reader.ReadUInt32() << 11;
                    float WaveSynchro = Reader.ReadSingle();
                    uint SubtitleOffset = Reader.ReadUInt32() + WaveOffset;
                    uint SubtitleLength = Reader.ReadUInt32();
                    uint TableLength = Reader.ReadUInt32();

                    if (Id == 0 || Speech.Position >= Speech.Length) break;

                    Speech.Seek(WaveOffset + 8, SeekOrigin.Begin);
                    WaveLength = Reader.ReadUInt32();
                    byte[] WaveBuffer = new byte[WaveLength];
                    Speech.Seek(WaveOffset, SeekOrigin.Begin);
                    Speech.Read(WaveBuffer, 0, WaveBuffer.Length);

                    Output.Seek(DataOffset, SeekOrigin.Begin);
                    Output.Write(WaveBuffer, 0, WaveBuffer.Length);

                    if (SubtitleLength > 0)
                    {
                        SubtitleLength = 0;

                        foreach (SubtitlePacket Subtitle in Input.Dialogs[SubIndex++].Subtitles)
                        {
                            byte[] SubtitleBuffer = SubtitlePacket.ToBytes(Subtitle);
                            SubtitleLength += (uint)SubtitleBuffer.Length;
                            Output.Write(SubtitleBuffer, 0, SubtitleBuffer.Length);
                        }
                    }

                    Speech.Seek(SubtitleOffset + SubtitleLength, SeekOrigin.Begin);
                    byte[] TableBuffer = new byte[TableLength];
                    Speech.Read(TableBuffer, 0, TableBuffer.Length);
                    Output.Write(TableBuffer, 0, TableBuffer.Length);

                    while ((Output.Position & 0x7ff) != 0) Output.WriteByte(0);

                    Writer.Write(Id);
                    Writer.Write((uint)(DataOffset >> 11));
                    Writer.Write((uint)((Output.Position - DataOffset) >> 11));
                    Writer.Write(WaveSynchro);
                    Writer.Write(WaveLength);
                    Writer.Write(SubtitleLength);
                    Writer.Write(TableLength);
                    Writer.Write(0u);

                    DataOffset = Output.Position;
                    Index++;
                }

                Output.Write(Header.ToArray(), 0, (int)Header.Length);
                while ((Output.Position & 0x7ff) != 0) Output.WriteByte(0);
                Header.Dispose();

                Speech.SetLength(0);
                Speech.Write(Output.ToArray(), 0, (int)Output.Length);
            }
        }

        /// <summary>
        ///     Gets the Address on the *.spc file where the Header is located.
        /// </summary>
        /// <param name="Reader">The *.spc file</param>
        /// <returns>The Header Address</returns>
        public static uint GetHeaderPosition(byte[] Data)
        {
            using (MemoryStream Input = new MemoryStream(Data))
            {
                EndianBinaryReader Reader = new EndianBinaryReader(Input, Endian.Big);
                return GetHeaderPosition(Reader);
            }
        }

        /// <summary>
        ///     Gets the Address on the *.spc file where the Header is located.
        /// </summary>
        /// <param name="Reader">The Reader of the Stream</param>
        /// <returns>The Header Address</returns>
        public static uint GetHeaderPosition(EndianBinaryReader Reader)
        {
            uint Position = (uint)Reader.BaseStream.Length - 0x800;
            while (true)
            {
                Reader.Seek(Position + 4, SeekOrigin.Begin);
                if (Reader.ReadUInt32() == 0) break;
                Position -= 0x800;
            }

            return Position;
        }
    }
}
