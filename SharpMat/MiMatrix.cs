using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMat
{
    public class MiMatrix : MatElement
    {
        private MatArrayFlags _flags;
        private readonly int[] _dimensions;
        private readonly double[] _values;
        
        public MiMatrix(MatElementTag tag, MatArrayFlags flags, string name, IEnumerable<int> dimensions, IEnumerable<double> values) : base(tag)
        {
            _flags = flags;
            Name = name;
            _dimensions = dimensions.ToArray();
            _values = values.ToArray();
        }

        public string Name { get; private set; }

        public int NumDimensions
        {
            get { return _dimensions.Length; }
        }

        public object GetValue(params int[] coords)
        {
            if (coords.Length != NumDimensions)
            {
                throw new ArgumentException("Invalid number of coordinates");
            }

            return _values[3*coords[0] + 2*coords[1] + 1*coords[2]];
        }
    }
}