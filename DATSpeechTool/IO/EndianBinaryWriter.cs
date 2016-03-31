using System;
using System.IO;

namespace DATSpeechTool.IO
{
    /// <summary>
    ///     Represents a Binary Writer that can be used to write Big or Little endian data.
    /// </summary>
    public class EndianBinaryWriter
    {
        /// <summary>
        ///     The Base Stream of this Writer.
        /// </summary>
        public Stream BaseStream { get; set; }

        /// <summary>
        ///     The current Endian of the Writer.
        /// </summary>
        public Endian Endian;

        /// <summary>
        ///     Creates a new instace of the Endian Binary Writer.
        /// </summary>
        /// <param name="Input">The Input Stream used to write the data</param>
        /// <param name="Endian">The Endian used on this Stream</param>
        public EndianBinaryWriter(Stream Input, Endian Endian)
        {
            BaseStream = Input;
            this.Endian = Endian;
        }

        /// <summary>
        ///     Writes a byte to the Stream.
        /// </summary>
        /// <param name="Value">The value to be written</param>
        public void Write(byte Value)
        {
            BaseStream.WriteByte(Value);
        }

        /// <summary>
        ///     Writes a unsigned 16-bits integer to the Stream.
        /// </summary>
        /// <param name="Value">The value to be written</param>
        public void Write(ushort Value)
        {
            if (Endian == Endian.Little)
            {
                BaseStream.WriteByte((byte)Value);
                BaseStream.WriteByte((byte)(Value >> 8));
            }
            else
            {
                BaseStream.WriteByte((byte)(Value >> 8));
                BaseStream.WriteByte((byte)Value);
            }
        }

        /// <summary>
        ///     Writes a signed 16-bits integer to the Stream.
        /// </summary>
        /// <param name="Value">The value to be written</param>
        public void Write(short Value)
        {
            Write((ushort)Value);
        }

        /// <summary>
        ///     Writes a unsigned 24-bits integer to the Stream.
        /// </summary>
        /// <param name="Value">The value to be written</param>
        public void Write24(uint Value)
        {
            if (Endian == Endian.Little)
            {
                BaseStream.WriteByte((byte)Value);
                BaseStream.WriteByte((byte)(Value >> 8));
                BaseStream.WriteByte((byte)(Value >> 16));
            }
            else
            {
                BaseStream.WriteByte((byte)(Value >> 16));
                BaseStream.WriteByte((byte)(Value >> 8));
                BaseStream.WriteByte((byte)Value);
            }
        }

        /// <summary>
        ///     Writes a unsigned 32-bits integer to the Stream.
        /// </summary>
        /// <param name="Value">The value to be written</param>
        public void Write(uint Value)
        {
            if (Endian == Endian.Little)
            {
                BaseStream.WriteByte((byte)Value);
                BaseStream.WriteByte((byte)(Value >> 8));
                BaseStream.WriteByte((byte)(Value >> 16));
                BaseStream.WriteByte((byte)(Value >> 24));
            }
            else
            {
                BaseStream.WriteByte((byte)(Value >> 24));
                BaseStream.WriteByte((byte)(Value >> 16));
                BaseStream.WriteByte((byte)(Value >> 8));
                BaseStream.WriteByte((byte)Value);
            }
        }

        /// <summary>
        ///     Writes a signed 32-bits integer to the Stream.
        /// </summary>
        /// <param name="Value">The value to be written</param>
        public void Write(int Value)
        {
            Write((uint)Value);
        }

        /// <summary>
        ///     Writes a 32-bits, single precision floating point value to the Stream.
        /// </summary>
        /// <param name="Value">The value to be written</param>
        public void Write(float Value)
        {
            Write(BitConverter.ToUInt32(BitConverter.GetBytes(Value), 0));
        }

        /// <summary>
        ///     Writes a Block of bytes to the Stream.
        /// </summary>
        /// <param name="Buffer">The Buffer to be written</param>
        public void Write(byte[] Buffer)
        {
            BaseStream.Write(Buffer, 0, Buffer.Length);
        }

        /// <summary>
        ///     Writes a Block of bytes to the Stream.
        /// </summary>
        /// <param name="Buffer">The Buffer to be written</param>
        /// <param name="Index">Start Index of the Array (zero based)</param>
        /// <param name="Length">Number of bytes to write</param>
        public void Write(byte[] Buffer, int Index, int Length)
        {
            BaseStream.Write(Buffer, Index, Length);
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
