using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SharpMat
{
    /// <summary>
    /// Provides a way to read Matlab .MAT-files.
    /// </summary>
    public class MatReader : IBinaryReader
    {
        /// <summary>
        /// The position of the next data tag in the underlying stream.
        /// </summary>
        private long _nextTagPosition;

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
        /// The header from the mat file or null if it has not been read.
        /// </summary>
        private MatHeader _header;
        
        /// <summary>
        /// Creates a new <see cref="MatReader"/> instance that reads from the given
        /// <see cref="Stream"/> and uses <see cref="Encoding.Default"/> when decoding
        /// characters. The given <see cref="Stream"/> will be disposed when disposing
        /// this instance.
        /// </summary>
        /// <param name="input">The <see cref="Stream"/> to read from.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        public MatReader(Stream input) : this(input, Encoding.Default)
        {}

        /// <summary>
        /// Creates a new <see cref="MatReader"/> instance that reads from the given
        /// <see cref="Stream"/> and uses the given encoding when decoding
        /// characters. The given <see cref="Stream"/> will be disposed when disposing
        /// this instance.
        /// </summary>
        /// <param name="input">The <see cref="Stream"/> to read from.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use when decoding characters.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        public MatReader(Stream input, Encoding encoding) : this(input, encoding, false)
        {}

        /// <summary>
        /// Creates a new <see cref="MatReader"/> instance that reads from the given
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
        public MatReader(Stream input, Encoding encoding, bool leaveOpen)
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
        /// Gets a value indicating if byteswaps are required. This value is
        /// set internally when reading the header of the .MAT-files
        /// </summary>
        public bool RequiresByteSwapping
        {
            get; set;
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
        /// Reads the MAT-header from the underlying stream. Before reading, the stream will be repositioned to the
        /// beginning if not already there. Finally, the stream will be positioned at the first data element tag.
        /// </summary>
        /// <returns>A new <see cref="MatHeader"/> instance.</returns>
        public MatHeader ReadHeader()
        {
            AssertNotDisposed();

            EndDecompress();
            if (_stream.Position != 0)
            {
                _stream.Seek(0, SeekOrigin.Begin);
            }
            
            //First 116 bytes is description string
            string text = ReadString(116).Trim();

            //Skip next 8 bytes according to specification
            Skip(8);

            //Then, next two bytes is version
            short version = ReadInt16();

            //Last two bytes of header is the endian indicator.
            //If it equals 19785, the endian of the data is same as this architecture,
            //thus no swaps are required.
            switch (ReadInt16())
            {
                case 19785:
                    RequiresByteSwapping = false;
                    break;
                case 18765:
                    RequiresByteSwapping = true;
                    break;
                default:
                    throw new InvalidDataException("Invalid endian indicator in header.");
            }

            _header = new MatHeader(text, version, RequiresByteSwapping);
            _nextTagPosition = _stream.Position;
            return _header;
        }

        /// <summary>
        /// Reads the next <see cref="MatElementTag"/> from the underlying stream.
        /// If the data is compressed, this method will decompress and return the
        /// decompressed information.
        /// </summary>
        public MatElementTag ReadNextElementTag()
        {
            AssertNotDisposed();

            //If not header has been read, make sure to read it.
            if (_header == null)
            {
                ReadHeader();
            }

            //Stop decompressing if currently in decompress mode.
            EndDecompress();
            if (_stream.Position != _nextTagPosition)
            {
                _stream.Position = _nextTagPosition;
            }
            if (_stream.Position >= _stream.Length)
            {
                return null;
            }

            MatElementTag tag = ReadElementTag();

            //Set the value of next tag position.
            //BUG: If the tag is a short tag, the tag is only 4 bytes. (Not sure if an element tag can be a short tag though).
            _nextTagPosition += 8 + tag.DataSize;

            if (tag.DataType == MatDataType.MiCompressed)
            {
                //If compressed, skip zip header and decompress before reading
                _stream.Seek(2, SeekOrigin.Current);
                BeginDecompress((int)tag.DataSize - 2);
                tag = ReadElementTag();
            }

            return tag;
        }

        /// <summary>
        /// Reads the next <see cref="MatMatrixHeader"/> from the underlying stream.
        /// </summary>
        /// <exception cref="EndOfStreamException"/>
        public MatMatrixHeader ReadMatrixHeader()
        {
            AssertNotDisposed();

            //Flags
            ReadElementTag();
            uint flags = ReadUInt32();
            bool complex = (flags & 0x00000800) == 0x0800;
            bool global = (flags & 0x00000400) == 0x0400;
            bool logical = (flags & 0x0000200) == 0x0200;
            MatArrayType type = (MatArrayType)(flags & 0x000000FF);
            ReadUInt32();

            //Dimensions
            var dimensionsTag = ReadElementTag();
            int[] dimensions = ReadInt32Array((int)dimensionsTag.DataSize / 4);

            //array name
            var nameTag = ReadElementTag();
            string title = ReadString((int)nameTag.DataSize);

            return new MatMatrixHeader
            {
                Complex = complex,
                Global = global,
                Logical = logical,
                ArrayType = type,
                Dimensions = dimensions,
                Name = title
            };
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
            //TODO: Investigate uses when in decompressed state.
            _stream.Seek(count, SeekOrigin.Current);
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
        /// Reads a <see cref="System.UInt16"/> value.
        /// </summary>
        public ushort ReadUInt16()
        {
            AssertNotDisposed();
            return ReadValue(2, BitConverter.ToUInt16);
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
        /// Reads a <see cref="System.UInt64"/> value.
        /// </summary>
        public ulong ReadUInt64()
        {
            AssertNotDisposed();
            return ReadValue(8, BitConverter.ToUInt64);
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
        /// Reads a <see cref="System.UInt32"/> value.
        /// </summary>
        public int ReadInt32()
        {
            AssertNotDisposed();
            return ReadValue(4, BitConverter.ToInt32);
        }

        /// <summary>
        /// Reads a <see cref="System.UInt64"/> value.
        /// </summary>
        public long ReadInt64()
        {
            AssertNotDisposed();
            return ReadValue(8, BitConverter.ToInt64);
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
        /// Reads a <see cref="System.Double"/> value.
        /// </summary>
        public double ReadDouble()
        {
            AssertNotDisposed();
            return ReadValue(8, BitConverter.ToDouble);
        }

        /// <summary>
        /// Reads a single character.
        /// </summary>
        public char ReadChar()
        {
            AssertNotDisposed();
            return ReadChars(1)[0];
        }

        public int[] ReadInt32Array(int count)
        {
            AssertNotDisposed();
            int[] result = new int[count];
            for (int ii = 0; ii < count; ++ii)
            {
                result[ii] = ReadInt32();
            }
            return result;
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
                throw new ObjectDisposedException("MatReader");
            }
        }

        #endregion
    }
}
