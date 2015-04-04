using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpMat
{
    /// <summary>
    /// Provides a way to write Matlab .MAT-files.
    /// TODO: Under construction
    /// </summary>
    public class MatWriter
    {
        private readonly BinaryWriter _writer;

        /// <summary>
        /// Creates a new <see cref="MatWriter"/> that supports writing
        /// .MAT-file data to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        public MatWriter(Stream stream)
        {
            _writer = new BinaryWriter(stream, Encoding.UTF8, false);
        }

        /// <summary>
        /// Writes the given <see cref="MatHeader"/> to the underlying <see cref="Stream"/>.
        /// </summary>
        /// <param name="header">The <see cref="MatHeader"/> to write.</param>
        public void WriteHeader(MatHeader header)
        {
            _writer.Write(header.DescriptiveText.PadRight(116).ToCharArray());
            _writer.Write((long) 0); //Pad with zeroes
            _writer.Write(header.Version);
            _writer.Write((short) 19785);
        }

        /// <summary>
        /// Writes the given <see cref="MatElementTag"/> to the underlying <see cref="Stream"/>.
        /// </summary>
        /// <param name="tag">The tag to write.</param>
        public void WriteElementTag(MatElementTag tag)
        {
            //If small data size, write small format tag.
            if (tag.DataSize <= 4)
            {
                uint tagValue = ((tag.DataSize << 16) & 0xFFFF0000) | (uint) tag.DataType;
                _writer.Write(tagValue);
            }
            else
            {
                _writer.Write((uint) tag.DataType);
                _writer.Write(tag.DataSize);
            }
        }

        public void WriteByte(byte value)
        {
            _writer.Write(value);
        }

        public void WriteUInt32(IEnumerable<uint> values)
        {
            _writer.Write((uint)MatDataType.MiUInt32);

            long sizePosition = _writer.BaseStream.Position;
            _writer.Write((uint) 0);

            long count = 0;
            foreach (uint value in values)
            {
                _writer.Write(value);
                count++;
            }

            long finalPosition = _writer.BaseStream.Position;
            _writer.BaseStream.Seek(sizePosition, SeekOrigin.Begin);
            _writer.Write((uint)count*4);
            _writer.BaseStream.Seek(finalPosition, SeekOrigin.Begin);
        }
    }
}