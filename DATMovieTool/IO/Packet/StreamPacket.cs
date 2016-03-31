using System.IO;
using System.Xml.Serialization;

using MGSShared;

namespace DATMovieTool.IO.Packet
{
    /// <summary>
    ///     Represents a generic Packet.
    /// </summary>
    public class StreamPacket
    {
        [XmlIgnore]
        public PacketType Type;

        [XmlAttribute]
        public uint StreamId;

        /// <summary>
        ///     Reads a Packet from a Stream.
        /// </summary>
        /// <param name="Reader">The reader of the Stream where the data is located</param>
        /// <param name="Game">The game being tampered</param>
        /// <returns>The packet as a object</returns>
        public static StreamPacket FromStream(EndianBinaryReader Reader, MGSGame Game)
        {
            uint StreamId = Reader.ReadUInt32() & 0xff;
            Reader.Seek(-4, SeekOrigin.Current);

            switch (StreamId)
            {
                case 4: return SubtitlePacket.FromStream(Reader, Game);
                case 0xf0: return EndOfStreamPacket.FromStream(Reader);
                default: return RawPacket.FromStream(Reader);
            }
        }

        /// <summary>
        ///     Writes a Packet to a Stream.
        /// </summary>
        /// <param name="Writer">The writer of the output Stream</param>
        /// <param name="PacketText">The packet to be written</param>
        /// <param name="Game">The game being tampered</param>
        public static void ToStream(EndianBinaryWriter Writer, StreamPacket Packet, MGSGame Game)
        {
            switch (Packet.Type)
            {
                case PacketType.Subtitle: SubtitlePacket.ToStream(Writer, (SubtitlePacket)Packet, Game); break;
                case PacketType.EndOfStream: EndOfStreamPacket.ToStream(Writer, (EndOfStreamPacket)Packet); break;
                case PacketType.Raw: RawPacket.ToStream(Writer, (RawPacket)Packet); break;
            }
        }
    }
}
