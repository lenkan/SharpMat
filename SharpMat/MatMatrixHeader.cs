namespace SharpMat
{
    /// <summary>
    /// Defines header information for a matrix in a .MAT file.
    /// </summary>
    public class MatMatrixHeader
    {
        private readonly MatArrayFlags _matArrayFlags = new MatArrayFlags();

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

        public MatArrayFlags MatArrayFlags
        {
            get { return _matArrayFlags; }
        }
    }
}