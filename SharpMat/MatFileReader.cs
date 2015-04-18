using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpMat
{
    public class MatFileReader : IDisposable
    {
        private readonly MatReader _matReader;
        private MatHeader _matHeader;

        public MatFileReader(Stream stream, Encoding encoding)
        {
            _matReader = new MatReader(stream, encoding);
        }

        public MatFileReader(string file)
        {
            _matReader = new MatReader(File.OpenRead(file), Encoding.Default, false);
        }

        public MatElement ReadElement()
        {
            if (_matHeader == null)
            {
                _matHeader = ReadHeader(_matReader);
            }

            _matReader.EndDecompress();
            var tag = _matReader.ReadElementTag();
            if (tag.DataType == MatDataType.MiCompressed)
            {
                _matReader.Skip(2);
                _matReader.BeginDecompress((int)tag.DataSize - 2);
                tag = _matReader.ReadElementTag();
            }

            if (tag.DataType == MatDataType.MiMatrix)
            {
                var flagsTag = _matReader.ReadElementTag();
                var flags = _matReader.ReadArrayFlags();

                var dimensionsTag = _matReader.ReadElementTag();
                int[] dimensions = _matReader.ReadInt32Array((int)dimensionsTag.DataSize / 4);
                _matReader.Skip(dimensionsTag.PaddingBytes);

                var nameTag = _matReader.ReadElementTag();
                string name = _matReader.ReadString((int)nameTag.DataSize);
                _matReader.Skip(nameTag.PaddingBytes);

                var valuesTag = _matReader.ReadElementTag();

                List<double> values = ReadValuesForTag(valuesTag).ToList();
                _matReader.Skip(valuesTag.PaddingBytes);
                
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
                    return _matReader.ReadBytes(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiInt16:
                    return _matReader.ReadInt16Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiUInt16:
                    return _matReader.ReadInt16Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiInt32:
                    return _matReader.ReadInt32Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiUInt32:
                    return _matReader.ReadUInt32Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiSingle:
                    return _matReader.ReadSingles(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiDouble:
                    return _matReader.ReadDoubles(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiInt64:
                    return _matReader.ReadInt64Array(tag.NumValues).Select(Convert.ToDouble);
                case MatDataType.MiUInt64:
                    return _matReader.ReadUInt64Array(tag.NumValues).Select(Convert.ToDouble);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_matReader != null)
                {
                    _matReader.Dispose();
                }
            }
        }
    }
}
