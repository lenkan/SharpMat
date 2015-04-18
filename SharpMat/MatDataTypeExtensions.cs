namespace SharpMat
{
    public static class MatDataTypeExtensions
    {
        public static int GetDataTypeSize(this MatDataType type)
        {
            switch (type)
            {
                case MatDataType.MiInt8:
                case MatDataType.MiUInt8:
                    return 1;
                case MatDataType.MiInt16:
                case MatDataType.MiUInt16:
                    return 2;
                case MatDataType.MiInt32:
                case MatDataType.MiUInt32:
                case MatDataType.MiSingle:
                    return 4;
                case MatDataType.MiDouble:
                case MatDataType.MiInt64:
                case MatDataType.MiUInt64:
                    return 8;
                default:
                    return -1;
            }
        }
    }
}