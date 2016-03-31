using System;
using System.IO;

namespace DATSpeechTool.IO
{
    /// <summary>
    ///     Represents a Binary Reader that can be used to read Big or Little endian data.
    /// </summary>
    public class EndianBinaryReader
    {
        /// <summary>
        ///     The Base Stream of this Reader.
        /// </summary>
        public Stream BaseStream { get; set; }

        /// <summary>
        ///     The current Endian of the Reader.
        /// </summary>
        public Endian Endian;

        /// <summary>
        ///     Creates a new instace of the Endian Binary Reader.
        /// </summary>
        /// <param name="Input">The Input Stream used to read the data</param>
        /// <param name="Endian">The Endian used on this Stream</param>
        public EndianBinaryReader(Stream Input, Endian Endian)
        {
            BaseStream = Input;
            this.Endian = Endian;
        }

        /// <summary>
        ///     Reads a byte from the Stream.
        /// </summary>
        /// <returns>The byte at the current position</returns>
        public byte ReadByte()
        {
            return (byte)BaseStream.ReadByte();
        }

        /// <summary>
        ///     Reads a unsigned 16-bits integer from the Stream.
        /// </summary>
        /// <returns>The integer at the current position</returns>
        public ushort ReadUInt16()
        {
            if (Endian == Endian.Little)
                return (ushort)
                    (BaseStream.ReadByte() |
                    (BaseStream.ReadByte() << 8));
            else
                return (ushort)
                    ((BaseStream.ReadByte() << 8) |
                    BaseStream.ReadByte());
        }

        /// <summary>
        ///     Reads a signed 16-bits integer from the Stream.
        /// </summary>
        /// <returns>The integer at the current position</returns>
        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        /// <summary>
        ///     Reads a unsigned 24-bits integer from the Stream.
        /// </summary>
        /// <returns>The integer at the current position</returns>
        public uint ReadUInt24()
        {
            if (Endian == Endian.Little)
                return (uint)
                    (BaseStream.ReadByte() |
                    (BaseStream.ReadByte() << 8) |
                    (BaseStream.ReadByte() << 16));
            else
                return (uint)
                    ((BaseStream.ReadByte() << 16) |
                    (BaseStream.ReadByte() << 8) |
                    BaseStream.ReadByte());
        }

        /// <summary>
        ///     Reads a unsigned 32-bits integer from the Stream.
        /// </summary>
        /// <returns>The integer at the current position</returns>
        public uint ReadUInt32()
        {
            if (Endian == Endian.Little)
                return (uint)
                    (BaseStream.ReadByte() |
                    (BaseStream.ReadByte() << 8) |
                    (BaseStream.ReadByte() << 16) |
                    (BaseStream.ReadByte() << 24));
            else
                return (uint)
                    ((BaseStream.ReadByte() << 24) |
                    (BaseStream.ReadByte() << 16) |
                    (BaseStream.ReadByte() << 8) |
                    BaseStream.ReadByte());
        }

        /// <summary>
        ///     Reads a signed 32-bits integer from the Stream.
        /// </summary>
        /// <returns>The integer at the current position</returns>
        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        /// <summary>
        ///     Reads a 32-bits, single precision floating point value from the Stream.
        /// </summary>
        /// <returns>The float at the current position</returns>
        public float ReadSingle()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadUInt32()), 0);
        }

        /// <summary>
        ///     Reads a Block of bytes from the Stream to a Buffer.
        /// </summary>
        /// <param name="Buffer">The Byte Array to store the data into</param>
        /// <param name="Index">Start Index of the Array (zero based)</param>
        /// <param name="Length">Number of bytes to read</param>
        public void Read(byte[] Buffer, int Index, int Length)
        {
            BaseStream.Read(Buffer, Index, Length);
        }

        /// <summary>
        ///     Seeks to a given position of the Stream.
        /// </summary>
        /// <param name="Offset">The Offset to Seek to, based on the Origin</param>
        /// <param name="Origin">From where to start counting the Offset</param>
        public void Seek(long Offset, SeekOrigin Origin)
        {
            BaseStream.Seek(Offset, Origin);
        }

        /// <summary>
        ///     Closes the underlying Stream.
        /// </summary>
        public void Close()
        {
            BaseStream.Close();
        }
    }
}
