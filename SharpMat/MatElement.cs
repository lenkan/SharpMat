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
}