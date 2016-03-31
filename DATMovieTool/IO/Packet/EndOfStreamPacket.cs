using System.IO;

namespace DATMovieTool.IO.Packet
{
    /// <summary>
    ///     Represents a End of Stream Packet, that appears at the end of the movie streams.
    /// </summary>
    public class EndOfStreamPacket : StreamPacket
    {
        public uint Value;

        public EndOfStreamPacket()
        {
            Type = PacketType.EndOfStream;
        }

        /// <summary>
        ///     Reads a End of Stream Packet from a Stream.
        /// </summary>
        /// <param name="Reader">The reader of the Stream where the data is located</param>
        /// <param name="Game">The game being tampered</param>
        /// <returns>The packet as a object</returns>
        public static EndOfStreamPacket FromStream(EndianBinaryReader Reader)
        {
            EndOfStreamPacket Packet = new EndOfStreamPacket();

            Packet.StreamId = Reader.ReadUInt32();
            uint PacketLength = Reader.ReadUInt32();
            Packet.Value = Reader.ReadUInt32();
            uint Dummy = Reader.ReadUInt32();

            if ((Reader.BaseStream.Position & 0x7ff) != 0)
            {
                long Position = Reader.BaseStream.Position & ~0x7ff;
                Reader.Seek(Position + 0x800, SeekOrigin.Begin);
            }

            return Packet;
        }

        /// <summary>
        ///     Writes a End of Stream Packet to a Stream.
        /// </summary>
        /// <param name="Writer">The writer of the output Stream</param>
        /// <param name="PacketText">The packet to be written</param>
        /// <param name="Game">The game being tampered</param>
        public static void ToStream(EndianBinaryWriter Writer, EndOfStreamPacket Packet)
        {
            Writer.Write(Packet.StreamId);
            Writer.Write(0x10u);
            Writer.Write(Packet.Value);
            Writer.Write(0u);

            while ((Writer.BaseStream.Position & 0x7ff) != 0) Writer.Write((byte)0);
        }
    }
}
