namespace SharpMat
{
    public class MatArrayFlags
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
    }
}