using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SharpMat
{
    /// <summary>
    /// Provides a way to read data from a stream representing a Matlab .MAT-file.
    /// This class is basically a <see cref="System.IO.BinaryReader"/> that addresses
    /// some of the specific needs required when reading .MAT files. Such as allowing
    /// to change the endianness, reading .MAT specific binary elements, but also to allow
    /// decompressing, see <see cref="BeginDecompress"/>, the underlying stream while reading.
    /// </summary>
    public class MatStreamReader : IBinaryReader
    {
        /// <summary>
        /// The encoding used to decode characters in the underlying stream.
        /// </summary>
        private readonly Encoding _encoding;

        /// <summary>
        /// A value indicating if the underlying stream should be disposed
        /// when this instance is disposed.
        /// </summary>
        private readonly bool _leaveOpen;

        /// <summary>
        /// The underlying stream.
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// A buffered stream that is used to decompress part of the original stream.
        /// </summary>
        private DeflateStream _inflatedStream;

        /// <summary>
        /// A value indicating if this instance has been disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Creates a new <see cref="MatStreamReader"/> instance that reads from the given
        /// <see cref="Stream"/> and uses <see cref="Encoding.Default"/> when decoding
        /// characters. The given <see cref="Stream"/> will be disposed when disposing
        /// this instance.
        /// </summary>
        /// <param name="input">The <see cref="Stream"/> to read from.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        public MatStreamReader(Stream input) : this(input, Encoding.Default)
        {}

        /// <summary>
        /// Creates a new <see cref="MatStreamReader"/> instance that reads from the given
        /// <see cref="Stream"/> and uses the given encoding when decoding
        /// characters. The given <see cref="Stream"/> will be disposed when disposing
        /// this instance.
        /// </summary>
        /// <param name="input">The <see cref="Stream"/> to read from.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use when decoding characters.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        public MatStreamReader(Stream input, Encoding encoding) : this(input, encoding, false)
        {}

        /// <summary>
        /// Creates a new <see cref="MatStreamReader"/> instance that reads from the given
        /// <see cref="Stream"/> and uses the given encoding when decoding
        /// characters. A value indicates if the given <see cref="Stream"/> should
        /// be disposed when disposing this instance.
        /// </summary>
        /// <param name="input">The <see cref="Stream"/> to read from.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use when decoding characters.</param>
        /// <param name="leaveOpen">
        /// A value indicating if the <see cref="Stream"/> should be disposed
        /// when disposing this instance. Set to true to leave it open, false to dispose it.
        /// </param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        public MatStreamReader(Stream input, Encoding encoding, bool leaveOpen)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (!input.CanRead)
            {
                throw new ArgumentException("Stream is not readable.", "input");
            }

            _stream = input;
            _encoding = encoding;
            _leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Gets or sets the endianness to use when reading numeric data.
        /// <remarks>
        /// Setting this to <see cref="SharpMat.Endianness.Default"/> will cause 
        /// the reader to use the current computer architecture endianness when reading.
        /// </remarks>
        /// </summary>
        public Endianness Endianness
        {
            get; set;
        }

        /// <summary>
        /// Gets a value indicating if byteswaps are required.
        /// </summary>
        public bool RequiresByteSwapping
        {
            get
            {
                return (Endianness == Endianness.LittleEndian && !BitConverter.IsLittleEndian) ||
                       (Endianness == Endianness.BigEndian && BitConverter.IsLittleEndian);
            }
        }

        /// <summary>
        /// Gets the inflated stream if not null, otherwise the original stream.
        /// </summary>
        private Stream CurrentStream
        {
            get
            {
                return _inflatedStream ?? _stream;
            }
        }

        /// <summary>
        /// Reads an instance of <see cref="MatArrayFlags"/>. This data is organized as
        /// two UInt32 valkues, thus reading this will read 8 bytes of data.
        /// </summary>
        public MatArrayFlags ReadArrayFlags()
        {
            uint flags = ReadUInt32();
            bool complex = (flags & 0x00000800) == 0x0800;
            bool global = (flags & 0x00000400) == 0x0400;
            bool logical = (flags & 0x0000200) == 0x0200;
            MatArrayType type = (MatArrayType)(flags & 0x000000FF);
            ReadUInt32();

            return new MatArrayFlags {Complex = complex, Global = global, Logical = logical, ArrayType = type};
        }

        /// <summary>
        /// Reads the next <see cref="MatElementTag"/> from the underlying stream.
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public MatElementTag ReadElementTag()
        {
            AssertNotDisposed();

            uint type = ReadUInt32();
            uint size;
            if (type > 256)
            {
                size = type >> 16;
                type = type & 0x0000FFFF;
            }
            else
            {
                size = ReadUInt32();
            }
            
            return new MatElementTag((MatDataType)type, size);
        }

        #region IBinaryReader

        /// <summary>
        /// Skips the given number of bytes.
        /// </summary>
        public void Skip(int count)
        {
            AssertNotDisposed();
            if (_inflatedStream == null)
            {
                _stream.Seek(count, SeekOrigin.Current);
            }
            else
            {
                for (int ii = 0; ii < count; ++ii)
                {
                    _inflatedStream.ReadByte();
                }
            }
        }

        /// <summary>
        /// Reads a string of characters from the <see cref="BinaryReader"/>. A parameter
        /// indicates how many characters that should be read. Advances the reader to point
        /// at the position after the string.
        /// </summary>
        /// <param name="count">The number of characters to read from the reader and interpret as a string.</param>
        /// <returns>The string value of the characters read.</returns>
        public string ReadString(int count)
        {
            AssertNotDisposed();
            char[] chars = ReadChars(count);
            return new string(chars);
        }

        /// <summary>
        /// Reads a single byte.
        /// </summary>
        public byte ReadByte()
        {
            AssertNotDisposed();
            return ReadByteArray(1)[0];
        }

        /// <summary>
        /// Reads an array of <see cref="System.UInt16"/> values form the underlying stream.
        /// </summary>
        public byte[] ReadBytes(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadByte);
        }

        /// <summary>
        /// Reads a <see cref="System.UInt16"/> value.
        /// </summary>
        public ushort ReadUInt16()
        {
            AssertNotDisposed();
            return ReadValue(2, BitConverter.ToUInt16);
        }

        /// <summary>
        /// Reads an array of <see cref="System.UInt16"/> values form the underlying stream.
        /// </summary>
        public ushort[] ReadUInt16Array(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadUInt16);
        }

        /// <summary>
        /// Reads a <see cref="System.UInt32"/> value.
        /// </summary>
        public uint ReadUInt32()
        {
            AssertNotDisposed();
            return ReadValue(4, BitConverter.ToUInt32);
        }
        
        /// <summary>
        /// Reads an array of <see cref="System.UInt32"/> values form the underlying stream.
        /// </summary>
        public uint[] ReadUInt32Array(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadUInt32);
        }

        /// <summary>
        /// Reads a <see cref="System.UInt64"/> value.
        /// </summary>
        public ulong ReadUInt64()
        {
            AssertNotDisposed();
            return ReadValue(8, BitConverter.ToUInt64);
        }

        /// <summary>
        /// Reads an array of <see cref="System.UInt64"/> values form the underlying stream.
        /// </summary>
        public ulong[] ReadUInt64Array(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadUInt64);
        }

        /// <summary>
        /// Reads a <see cref="System.Int16"/> value.
        /// </summary>
        public short ReadInt16()
        {
            AssertNotDisposed();
            return ReadValue(2, BitConverter.ToInt16);
        }

        /// <summary>
        /// Reads an array of <see cref="System.Int16"/> values from the underlying stream.
        /// </summary>
        public short[] ReadInt16Array(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadInt16);
        }

        /// <summary>
        /// Reads a <see cref="System.Int32"/> value.
        /// </summary>
        public int ReadInt32()
        {
            AssertNotDisposed();
            return ReadValue(4, BitConverter.ToInt32);
        }

        /// <summary>
        /// Reads an array of <see cref="System.Int32"/> values from the underlying stream.
        /// </summary>
        public int[] ReadInt32Array(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadInt32);
        }

        /// <summary>
        /// Reads a <see cref="System.Int64"/> value.
        /// </summary>
        public long ReadInt64()
        {
            AssertNotDisposed();
            return ReadValue(8, BitConverter.ToInt64);
        }

        /// <summary>
        /// Reads an array of <see cref="System.Int64"/> values form the underlying stream.
        /// </summary>
        public long[] ReadInt64Array(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadInt64);
        }

        /// <summary>
        /// Reads a <see cref="System.Single"/> value.
        /// </summary>
        public float ReadSingle()
        {
            AssertNotDisposed();
            return ReadValue(4, BitConverter.ToSingle);
        }

        /// <summary>
        /// Reads an array of <see cref="System.Single"/> values from the underlying stream.
        /// </summary>
        public float[] ReadSingles(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadSingle);
        }

        /// <summary>
        /// Reads a <see cref="System.Double"/> value.
        /// </summary>
        public double ReadDouble()
        {
            AssertNotDisposed();
            return ReadValue(8, BitConverter.ToDouble);
        }

        /// <summary>
        /// Reads an array of <see cref="System.Double"/> from the underlying stream.
        /// </summary>
        public double[] ReadDoubles(int count)
        {
            AssertNotDisposed();
            return ReadArray(count, ReadDouble);
        }

        /// <summary>
        /// Reads a single character.
        /// </summary>
        public char ReadChar()
        {
            AssertNotDisposed();
            return ReadChars(1)[0];
        }

        /// <summary>
        /// Reads the given number of characters.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        public char[] ReadChars(int count)
        {
            AssertNotDisposed();
            //BUG: Does not respect endianness
            int size = _encoding.GetByteCount(new char[1]);
            byte[] buffer = new byte[size*count];
            if (CurrentStream.Read(buffer, 0, size*count) < size*count)
            {
                throw new EndOfStreamException("Not enough data in stream.");
            }
            return _encoding.GetChars(buffer);
        }

        /// <summary>
        /// Helper method to read an array of values of the given type.
        /// </summary>
        /// <typeparam name="T">The type of values to read.</typeparam>
        /// <param name="count">The number of values to read.</param>
        /// <param name="readFunc">The function that reads one value.</param>
        /// <returns>An array of length <paramref name="count"/> with read values.</returns>
        private T[] ReadArray<T>(int count, Func<T> readFunc)
        {
            AssertNotDisposed();
            T[] result = new T[count];
            for (int ii = 0; ii < count; ++ii)
            {
                result[ii] = readFunc();
            }
            return result;
        }

        /// <summary>
        /// Helper method to read a value from the underlying stream while specifying a
        /// converter method to convert to the given type. This method uses <see cref="ReadByteArray"/>
        /// which respects if byte swapping should be performed.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="size">The number of bytes the value is made up of.</param>
        /// <param name="converter">The converter used to convert an array of bytes to the given type.</param>
        private T ReadValue<T>(int size, Func<byte[], int, T> converter)
        {
            return converter(ReadByteArray(size), 0);
        }

        /// <summary>
        /// Reads a byte array from the stream and returns it in the correct order with respect to
        /// the endianness
        /// </summary>
        private byte[] ReadByteArray(int count)
        {
            var buffer = new byte[count];
            if (CurrentStream.Read(buffer, 0, count) < count)
            {
                throw new EndOfStreamException("Not enough data in the stream.");
            }

            return RequiresByteSwapping ? buffer.Reverse().ToArray() : buffer;
        }

        #endregion

        #region Compression

        /// <summary>
        /// Creates a buffered decompressing stream and makes this instance read from that stream
        /// until <see cref="EndDecompress"/> is called.
        /// </summary>
        /// <param name="count">The number of bytes from the original stream to read into the decompressed buffer.</param>
        public void BeginDecompress(int count)
        {
            AssertNotDisposed();
            byte[] buffer = new byte[count];
            if (_stream.Read(buffer, 0, count) < count)
            {
                throw new EndOfStreamException("Not enough data in the stream.");
            }

            _inflatedStream = new DeflateStream(new MemoryStream(buffer), CompressionMode.Decompress, false);
        }

        /// <summary>
        /// Disposes the decompressing stream and makes this instance read from the original stream again.
        /// </summary>
        public void EndDecompress()
        {
            AssertNotDisposed();
            if (_inflatedStream != null)
            {
                _inflatedStream.Dispose();
                _inflatedStream = null;
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Disposes the managed resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                _isDisposed = true;
                if (!_leaveOpen && _stream != null)
                {
                    _stream.Dispose();
                }
                if (_inflatedStream != null)
                {
                    _inflatedStream.Dispose();
                }
            }
        }

        private void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("MatStreamReader");
            }
        }

        #endregion
    }
}
