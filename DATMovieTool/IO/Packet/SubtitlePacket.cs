using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using MGSShared;

namespace DATMovieTool.IO.Packet
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
        /// <param name="Game">The game being tampered</param>
        /// <returns>The entry as a object</returns>
        public static SubtitlePacketText FromStream(EndianBinaryReader Reader, MGSGame Game)
        {
            SubtitlePacketText PacketText = new SubtitlePacketText();

            PacketText.StartTime = Reader.ReadUInt32();
            PacketText.EndTime = Reader.ReadUInt32();
            uint Dummy = Reader.ReadUInt32();
            ushort TextLength = Reader.ReadUInt16();
            PacketText.LanguageId = Reader.ReadUInt16();

            byte[] TextBuffer = new byte[TextLength - 0x10];
            Reader.Read(TextBuffer, 0, TextBuffer.Length);
            PacketText.Text = MGSText.Buffer2Text(Unpad(TextBuffer), Game);
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
        /// <param name="Game">The game being tampered</param>
        public static void ToStream(EndianBinaryWriter Writer, SubtitlePacketText PacketText, MGSGame Game)
        {
            byte[] TextBuffer = new byte[0];

            if (PacketText.Text != null)
            {
                PacketText.Text = PacketText.Text.Replace("\\n", Environment.NewLine);
                TextBuffer = MGSText.Text2Buffer(PacketText.Text, Game);
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

    public class SubtitlePacket : StreamPacket
    {
        [XmlAttribute]
        public uint BaseStartTime;

        [XmlArrayItem("Text")]
        public List<SubtitlePacketText> Texts;

        public SubtitlePacket()
        {
            Type = PacketType.Subtitle;
            Texts = new List<SubtitlePacketText>();
        }

        /// <summary>
        ///     Reads a Subtitle Packet from a Stream.
        /// </summary>
        /// <param name="Reader">The reader of the Stream where the data is located</param>
        /// <param name="Game">The game being tampered</param>
        /// <returns>The packet as a object</returns>
        public new static SubtitlePacket FromStream(EndianBinaryReader Reader, MGSGame Game)
        {
            SubtitlePacket Packet = new SubtitlePacket();
            long BasePosition = Reader.BaseStream.Position;

            Packet.StreamId = Reader.ReadUInt32();
            uint PacketLength = Reader.ReadUInt32();
            long EndPosition = BasePosition + PacketLength;

            Packet.BaseStartTime = Reader.ReadUInt32();
            uint Dummy = Reader.ReadUInt32();
            uint DataLength = Reader.ReadUInt32();

            while (Reader.BaseStream.Position + 0x10 < EndPosition)
            {
                Packet.Texts.Add(SubtitlePacketText.FromStream(Reader, Game));
            }

            Reader.Seek(EndPosition, SeekOrigin.Begin);

            return Packet;
        }

        /// <summary>
        ///     Writes a Subtitle Packet to a Stream.
        /// </summary>
        /// <param name="Writer">The writer of the output Stream</param>
        /// <param name="PacketText">The packet to be written</param>
        /// <param name="Game">The game being tampered</param>
        public static void ToStream(EndianBinaryWriter Writer, SubtitlePacket Packet, MGSGame Game)
        {
            using (MemoryStream Content = new MemoryStream())
            {
                EndianBinaryWriter CWriter = new EndianBinaryWriter(Content, Writer.Endian);

                foreach (SubtitlePacketText Text in Packet.Texts) SubtitlePacketText.ToStream(CWriter, Text, Game);

                int Length = (int)Content.Length + 0x14 + 1;
                if ((Length & 0xf) != 0) Length = (Length & ~0xf) + 0x10;

                Writer.Write(Packet.StreamId);
                Writer.Write(Length);

                Writer.Write(Packet.BaseStartTime);
                Writer.Write(0u);
                Writer.Write((uint)Content.Length);

                Writer.Write(Content.ToArray());
                Writer.Write((byte)0);

                while ((Writer.BaseStream.Position & 0xf) != 0) Writer.Write((byte)0);
            }
        }
    }
}
