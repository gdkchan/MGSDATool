namespace DATMovieTool.IO.Packet
{
    /// <summary>
    ///     Represents a Raw Packet, that doesn't need to be altered and is kept in it's original form.
    /// </summary>
    public class RawPacket : StreamPacket
    {
        public byte[] Data;

        public RawPacket()
        {
            Type = PacketType.Raw;
        }

        /// <summary>
        ///     Reads a Raw Packet from a Stream.
        /// </summary>
        /// <param name="Reader">The reader of the Stream where the data is located</param>
        /// <param name="Game">The game being tampered</param>
        /// <returns>The packet as a object</returns>
        public static RawPacket FromStream(EndianBinaryReader Reader)
        {
            RawPacket Packet = new RawPacket();

            Packet.StreamId = Reader.ReadUInt32();
            Packet.Data = new byte[Reader.ReadUInt32() - 8];
            Reader.Read(Packet.Data, 0, Packet.Data.Length);

            return Packet;
        }

        /// <summary>
        ///     Writes a Raw Packet to a Stream.
        /// </summary>
        /// <param name="Writer">The writer of the output Stream</param>
        /// <param name="PacketText">The packet to be written</param>
        /// <param name="Game">The game being tampered</param>
        public static void ToStream(EndianBinaryWriter Writer, RawPacket Packet)
        {
            Writer.Write(Packet.StreamId);
            Writer.Write(Packet.Data.Length + 8);
            Writer.Write(Packet.Data);
        }
    }
}
