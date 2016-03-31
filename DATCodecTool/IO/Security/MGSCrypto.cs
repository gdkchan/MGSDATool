namespace DATCodecTool.IO.Security
{
    /// <summary>
    ///     Metal Gear Solid DAT encryption.
    /// </summary>
    class MGSCrypto
    {
        private long Key;

        /// <summary>
        ///     Initializes a new instance of the Metal Gear Solid cryptography class.
        /// </summary>
        /// <param name="Key">The 32-bits encryption key</param>
        public MGSCrypto(uint Key)
        {
            this.Key = Key;
        }

        /// <summary>
        ///     Gets the current XOR byte derived from the Key.
        /// </summary>
        /// <returns>The encryption byte</returns>
        public byte GetCipherByte()
        {
            Key = (Key * 0x7d2b89dd) + 0xcf9;
            Key = (Key << 32) >> 32;
            return (byte)(Key >> 15);
        }

        /// <summary>
        ///     Encrypts or decrypts a data array with the given key.
        /// </summary>
        /// <param name="Buffer">The buffer to be processed</param>
        /// <returns>The processed buffer</returns>
        public byte[] ProcessBuffer(byte[] Buffer)
        {
            byte[] Output = new byte[Buffer.Length];

            for (int Index = 0; Index < Buffer.Length; Index++)
            {
                Output[Index] = (byte)(Buffer[Index] ^ GetCipherByte());
            }

            return Output;
        }

        /// <summary>
        ///     Encrypts or decrypts a byte with the given key.
        /// </summary>
        /// <param name="Value">The value to be processed</param>
        /// <returns>The processed value</returns>
        public byte ProcessByte(byte Value)
        {
            return (byte)(Value ^ GetCipherByte());
        }
    }
}
