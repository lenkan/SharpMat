namespace SharpMat
{
    /// <summary>
    /// Represents an element tag in a .MAT-file. A tag defines
    /// which data type and the size in number of bytes the succeeding
    /// element has.
    /// </summary>
    public class MatElementTag
    {
        /// <summary>
        /// Creates a new instance of <see cref="MatElementTag"/>.
        /// </summary>
        public MatElementTag(MatDataType dataType, uint dataSize)
        {
            DataType = dataType;
            DataSize = dataSize;
        }

        /// <summary>
        /// Gets the <see cref="MatDataType"/> of the data tag. If the tag is a compressed
        /// tag, this value should represent the decompressed tag, i.e. the value after decompressing.
        /// </summary>
        public MatDataType DataType
        {
            get; private set;
        }

        /// <summary>
        /// Gets the size, in number of bytes, of this data element. This refers
        /// to the element data, not including the size of the tag itself.
        /// </summary>
        public uint DataSize
        {
            get; private set;
        }

        /// <summary>
        /// Gets the number of values that the corresponding element contains.
        /// </summary>
        public int NumValues
        {
            get { return (int)DataSize/DataType.GetDataTypeSize(); }
        }

        /// <summary>
        /// Gets a value that determines how many bytes that are padded after
        /// the data belonging to the element that this tag refers to.
        /// </summary>
        internal int PaddingBytes
        {
            get
            {
                if (DataSize <= 4)
                {
                    return 4 - (int) DataSize;
                }
                return (int) (8 - DataSize) % 8;
            }
        }
    }
}