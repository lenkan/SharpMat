namespace SharpMat
{
    /// <summary>
    /// Defines header information for a matrix in a .MAT file.
    /// </summary>
    public class MatMatrixHeader
    {
        /// <summary>
        /// Gets a value indicating if the matrix is complex, i.e.,
        /// if it has a real part and an imaginary part.
        /// </summary>
        public bool Complex { get; set; }

        /// <summary>
        /// Gets a value indicating if the matrix is global.
        /// </summary>
        public bool Global { get; set; }

        /// <summary>
        /// Gets a value indicating if the matrix values are logical.
        /// </summary>
        public bool Logical { get; set; }

        /// <summary>
        /// Gets the <see cref="MatArrayType"/> of the array.
        /// </summary>
        public MatArrayType ArrayType { get; set; }

        /// <summary>
        /// Gets the dimensions of the matrix.
        /// <example>
        /// A 3x3 matrix will be represented by the values {3,3}.
        /// A 3x2x3 matrix will be represented by the values {3, 2, 3}.
        /// etc..
        /// </example>
        /// </summary>
        public int[] Dimensions { get; set; }

        /// <summary>
        /// Gets the name of the matrix.
        /// </summary>
        public string Name { get; set; }
    }
}