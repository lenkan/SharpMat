using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpMat
{
    public class MatFileReader : IDisposable
    {
        private readonly MatStreamReader _reader;
        private MatHeader _matHeader;

        public MatFileReader(Stream stream, Encoding encoding)
        {
            _reader = new MatStreamReader(stream, encoding);
        }

        public MatFileReader(string file)
        {
            _reader = new MatStreamReader(File.OpenRead(file), Encoding.Default, false);
        }

        public MatElement ReadElement()
        {
            if (_matHeader == null)
            {
                _matHeader = ReadHeader(_reader);
            }

            _reader.EndDecompress();
            var tag = _reader.ReadElementTag();
            if (tag.DataType == MatDataType.MiCompressed)
            {
                _reader.Skip(2);
                _reader.BeginDecompress((int)tag.DataSize - 2);
                tag = _reader.ReadElementTag();
            }

            if (tag.DataType == MatDataType.MiMatrix)
            {
                var flagsTag = _reader.ReadElementTag();
                var flags = _reader.ReadArrayFlags();

                var dimensionsTag = _reader.ReadElementTag();
                int[] dimensions = _reader.ReadInt32Array((int)dimensionsTag.DataSize / 4);
                _reader.Skip(dimensionsTag.PaddingBytes);

                var nameTag = _reader.ReadElementTag();
                string name = _reader.ReadString((int)nameTag.DataSize);
                _reader.Skip(nameTag.PaddingBytes);

                var valuesTag = _reader.ReadElementTag();

                List<double> values = ReadValuesForTag(valuesTag).ToList();
                _reader.Skip(valuesTag.PaddingBytes);
                
                return new MiMatrix(tag, flags, name, dimensions, values);
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<double> ReadValuesForTag(MatElementTag tag)
        {
            switch (tag.DataType)
            {
                case MatDataType.MiInt8:
                case MatDataType.MiUInt8:
                    return _reader.ReadBytes(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiInt16:
                    return _reader.ReadInt16Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiUInt16:
                    return _reader.ReadInt16Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiInt32:
                    return _reader.ReadInt32Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiUInt32:
                    return _reader.ReadUInt32Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiSingle:
                    return _reader.ReadSingles(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiDouble:
                    return _reader.ReadDoubles(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiInt64:
                    return _reader.ReadInt64Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiUInt64:
                    return _reader.ReadUInt64Array(tag.NumValues).Select(Convert.ToDouble);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static MatHeader ReadHeader(IBinaryReader reader)
        {
            //First 116 bytes is description string
            string text = reader.ReadString(116).Trim();

            //Skip next 8 bytes according to specification
            reader.Skip(8);

            //Then, next two bytes is version
            short version = reader.ReadInt16();

            //Last two bytes of header is the endian indicator.
            //If it equals 19785, the endian of the data is same as this architecture,
            //thus no swaps are required.
            bool swap;
            switch (reader.ReadInt16())
            {
                case 19785:
                    swap = false;
                    break;
                case 18765:
                    swap = true;
                    break;
                default:
                    throw new InvalidDataException("Invalid endian indicator in header.");
            }

            return new MatHeader(text, version, swap);
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_reader != null)
                {
                    _reader.Dispose();
                }
            }
        }

        #endregion
    }
}
