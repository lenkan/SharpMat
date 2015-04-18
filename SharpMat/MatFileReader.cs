using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpMat
{
    public class MatFileReader : IDisposable
    {
        private readonly MatReader _matReader;
        private MatHeader _matHeader;

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

                List<double> values = new List<double>();
                switch (valuesTag.DataType)
                {
                    case MatDataType.MiInt8:
                        values.Add((double)_matReader.ReadByte());
                        break;
                    case MatDataType.MiUInt8:
                        values.Add((double)_matReader.ReadByte());
                        break;
                    case MatDataType.MiInt16:
                        values.Add((double)_matReader.ReadInt16());
                        break;
                    case MatDataType.MiUInt16:
                        values.Add((double)_matReader.ReadUInt16());
                        break;
                    case MatDataType.MiInt32:
                        values.Add((double)_matReader.ReadInt32());
                        break;
                    case MatDataType.MiUInt32:
                        break;
                    case MatDataType.MiSingle:
                        break;
                    case MatDataType.MiDouble:
                        break;
                    case MatDataType.MiInt64:
                        break;
                    case MatDataType.MiUInt64:
                        break;
                    case MatDataType.MiMatrix:
                        break;
                    case MatDataType.MiCompressed:
                        break;
                    case MatDataType.MiUtf8:
                        break;
                    case MatDataType.MiUtf16:
                        break;
                    case MatDataType.MiUtf32:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _matReader.Skip(valuesTag.PaddingBytes);
                return new MiMatrix(tag, flags, name, dimensions, values);
            }
            else
            {
                return null;
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
