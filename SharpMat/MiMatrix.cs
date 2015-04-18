using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

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
            if (coords.Length == 1)
            {
                return _values[coords[0]];
            }

            int index = 0;
            for (int ii = 0; ii < _dimensions.Length; ++ii)
            {
                int coord = 0;
                if (ii < coords.Length)
                {
                    coord = coords[ii];
                }
                if (coord >= _dimensions[ii])
                {
                    throw new ArgumentException("Index exceeds matrix dimensions.");
                }

                index += coord;
            }

            return _values[index];
        }
    }
}