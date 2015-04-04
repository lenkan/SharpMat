using System;

namespace SharpMat
{
    /// <summary>
    /// Represents a .MAT-file header
    /// </summary>
    public class MatHeader
    {
        private string _descriptiveText;

        public MatHeader(string text)
            : this(text, 256, false)
        {}

        internal MatHeader(string text, short version, bool endianIndicator)
        {
            DescriptiveText = text;
            Version = version;
            EndianIndicator = endianIndicator;
        }

        /// <summary>
        /// Gets or sets the descriptive text of this file header.
        /// </summary>
        public string DescriptiveText
        {
            get { return _descriptiveText; }
            set
            {
                if (value.Length < 4)
                {
                    throw new ArgumentException("value");
                }
                if (value.Length > 115)
                {
                    throw new ArgumentException("value");
                }
                _descriptiveText = value;
            }
        }

        /// <summary>
        /// Gets a value indicating if byte-swapping is required when reading the data
        /// associated with this header.
        /// </summary>
        public bool EndianIndicator
        {
            get; private set;
        }

        /// <summary>
        /// Get a value indicating the version of the mat file format. Or similar.
        /// </summary>
        public short Version
        {
            get; private set;
        }
    }
}
