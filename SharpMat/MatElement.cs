using System.Collections.Generic;
using System.Linq;

namespace SharpMat
{
    public class MatElement
    {
        private readonly MatElementTag _tag;

        public MatElement(MatElementTag tag)
        {
            _tag = tag;
        }

        public MatElementTag Tag
        {
            get { return _tag; }
        }
    }

    public class MiNumericElement : MatElement
    {
        public MiNumericElement(MatElementTag tag, IEnumerable<byte> data)
            : base(tag)
        {}
    }
}