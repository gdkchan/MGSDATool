using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using MGSShared;

namespace DATSpeechTool.IO.Packet
{
    /// <summary>
    ///     Represents a Subtitle Packet.
    /// </summary>
    public class SubtitlePacketText
    {
        [XmlAttribute]
        public uint StartTime;

        [XmlAttribute]
        public uint EndTime;

        [XmlAttribute]
        public int LanguageId;

        [XmlText]
        public string Text;

        /// <summary>
        ///     Reads a text entry of a Subtitle Packet from a Stream.
        /// </summary>
        /// <param name="Reader">The reader of the Stream where the data is located</param>
        /// <returns>The entry as a object</returns>
        public static SubtitlePacketText FromStream(EndianBinaryReader Reader)
        {
            SubtitlePacketText PacketText = new SubtitlePacketText();

            PacketText.StartTime = Reader.ReadUInt32();
            PacketText.EndTime = Reader.ReadUInt32();
            uint Dummy = Reader.ReadUInt32();
            ushort TextLength = Reader.ReadUInt16();
            PacketText.LanguageId = Reader.ReadUInt16();

            byte[] TextBuffer = new byte[TextLength - 0x10];
            Reader.Read(TextBuffer, 0, TextBuffer.Length);
            PacketText.Text = MGSText.Buffer2Text(Unpad(TextBuffer), MGSGame.MGS4);
            PacketText.Text = PacketText.Text.Replace(Environment.NewLine, "\\n");

            return PacketText;
        }

        private static byte[] Unpad(byte[] Data)
        {
            int Length = 0;
            while (Length < Data.Length && Data[Length++] != 0);
            return ResizeBuffer(Data, Length - 1);
        }

        private static byte[] ResizeBuffer(byte[] Data, int NewSize)
        {
            byte[] NewData = new byte[NewSize];
            Buffer.BlockCopy(Data, 0, NewData, 0, NewSize);
            return NewData;
        }

        /// <summary>
        ///     Writes a text entry of a Subtitle Packet to a Stream.
        /// </summary>
        /// <param name="Writer">The writer of the output Stream</param>
        /// <param name="PacketText">The text entry to be written</param>
        public static void ToStream(EndianBinaryWriter Writer, SubtitlePacketText PacketText)
        {
            byte[] TextBuffer = new byte[0];

            if (PacketText.Text != null)
            {
                PacketText.Text = PacketText.Text.Replace("\\n", Environment.NewLine);
                TextBuffer = MGSText.Text2Buffer(PacketText.Text, MGSGame.MGS4);
            }

            int Length = TextBuffer.Length + 1;
            if ((Length & 3) != 0) Length = (Length & ~3) + 4;

            Writer.Write(PacketText.StartTime);
            Writer.Write(PacketText.EndTime);
            Writer.Write(0u);
            Writer.Write((ushort)(Length + 0x10));
            Writer.Write((ushort)PacketText.LanguageId);

            Writer.Write(TextBuffer);
            Writer.Write((byte)0);

            while ((Writer.BaseStream.Position & 3) != 0) Writer.Write((byte)0);
        }
    }

    /// <summary>
    ///     Represents a packet containing audio subtitles.
    /// </summary>
    public class SubtitlePacket
    {
        [XmlAttribute]
        public uint BaseStartTime;

        [XmlArrayItem("Text")]
        public List<SubtitlePacketText> Texts;

        public SubtitlePacket()
        {
            Texts = new List<SubtitlePacketText>();
        }

        /// <summary>
        ///     Reads a Subtitle Packet from a Byte Array.
        /// </summary>
        /// <param name="Data">The Byte Array where the data is located</param>
        /// <returns>The packet as a object</returns>
        public static SubtitlePacket FromBytes(byte[] Data)
        {
            using (MemoryStream Input = new MemoryStream(Data))
            {
                EndianBinaryReader Reader = new EndianBinaryReader(Input, Endian.Big);
                return FromStream(Reader);
            }
        }

        /// <summary>
        ///     Reads a Subtitle Packet from a Stream.
        /// </summary>
        /// <param name="Reader">The reader of the Stream where the data is located</param>
        /// <returns>The packet as a object</returns>
        public static SubtitlePacket FromStream(EndianBinaryReader Reader)
        {
            SubtitlePacket Packet = new SubtitlePacket();
            long BasePosition = Reader.BaseStream.Position;

            Reader.Endian = Endian.Little;
            uint Signature = Reader.ReadUInt32();
            uint PacketLength = Reader.ReadUInt32();
            long EndPosition = BasePosition + PacketLength;

            if (PacketLength == 0 || Signature != 0) return null;

            Packet.BaseStartTime = Reader.ReadUInt32();
            Reader.Endian = Endian.Big;
            uint Dummy = Reader.ReadUInt32();
            uint DataLength = Reader.ReadUInt32();

            while (Reader.BaseStream.Position + 0x10 < EndPosition)
            {
                Packet.Texts.Add(SubtitlePacketText.FromStream(Reader));
            }

            Reader.Seek(EndPosition, SeekOrigin.Begin);

            return Packet;
        }

        /// <summary>
        ///     Writes a Subtitle Packet to a Byte Array.
        /// </summary>
        /// <param name="Packet">The packet to be written</param>
        /// <returns>The array with the data</returns>
        public static byte[] ToBytes(SubtitlePacket Packet)
        {
            using (MemoryStream Output = new MemoryStream())
            {
                EndianBinaryWriter Writer = new EndianBinaryWriter(Output, Endian.Big);
                ToStream(Writer, Packet);
                return Output.ToArray();
            }
        }

        /// <summary>
        ///     Writes a Subtitle Packet to a Stream.
        /// </summary>
        /// <param name="Writer">The writer of the output Stream</param>
        /// <param name="Packet">The packet to be written</param>
        public static void ToStream(EndianBinaryWriter Writer, SubtitlePacket Packet)
        {
            using (MemoryStream Content = new MemoryStream())
            {
                EndianBinaryWriter CWriter = new EndianBinaryWriter(Content, Writer.Endian);

                foreach (SubtitlePacketText Text in Packet.Texts) SubtitlePacketText.ToStream(CWriter, Text);

                int Length = (int)Content.Length + 0x14 + 1;
                if ((Length & 0xf) != 0) Length = (Length & ~0xf) + 0x10;

                Writer.Endian = Endian.Little;
                Writer.Write(0u);
                Writer.Write(Length);

                Writer.Write(Packet.BaseStartTime);
                Writer.Endian = Endian.Big;
                Writer.Write(0u);
                Writer.Write((int)Content.Length);

                Writer.Write(Content.ToArray());
                Writer.Write((byte)0);

                while ((Writer.BaseStream.Position & 0xf) != 0) Writer.Write((byte)0);
            }
        }
    }
}
