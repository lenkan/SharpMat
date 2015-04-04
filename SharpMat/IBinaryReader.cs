namespace SharpMat
{
    /// <summary>
    /// Provides a way to read numeric and character values in a streaming manner.
    /// </summary>
    public interface IBinaryReader
    {
        /// <summary>
        /// Skips reading the given number of bytes.
        /// </summary>
        void Skip(int count);

        /// <summary>
        /// Reads a string consisting of the given number of characters.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        string ReadString(int count);
        
        /// <summary>
        /// Reads a single byte.
        /// </summary>
        byte ReadByte();

        /// <summary>
        /// Reads a single character.
        /// </summary>
        char ReadChar();

        /// <summary>
        /// Reads the given number of characters.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        char[] ReadChars(int count);

        /// <summary>
        /// Reads a <see cref="System.UInt16"/> value.
        /// </summary>
        ushort ReadUInt16();

        /// <summary>
        /// Reads a <see cref="System.UInt32"/> value.
        /// </summary>
        uint ReadUInt32();

        /// <summary>
        /// Reads a <see cref="System.UInt64"/> value.
        /// </summary>
        ulong ReadUInt64();
        
        /// <summary>
        /// Reads a <see cref="System.Int16"/> value.
        /// </summary>
        short ReadInt16();
        
        /// <summary>
        /// Reads a <see cref="System.UInt32"/> value.
        /// </summary>
        int ReadInt32();
        
        /// <summary>
        /// Reads a <see cref="System.UInt64"/> value.
        /// </summary>
        long ReadInt64();

        /// <summary>
        /// Reads a <see cref="System.Single"/> value.
        /// </summary>
        float ReadSingle();

        /// <summary>
        /// Reads a <see cref="System.Double"/> value.
        /// </summary>
        double ReadDouble();
    }
}