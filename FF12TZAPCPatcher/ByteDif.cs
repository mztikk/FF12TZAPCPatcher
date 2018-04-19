namespace FF12TZAPCPatcher
{
    public class ByteDif
    {
        public readonly byte NewByte;

        public readonly long Offset;

        public readonly byte OriginalByte;

        public ByteDif(long offset, byte originalByte, byte newByte)
        {
            this.Offset = offset;
            this.OriginalByte = originalByte;
            this.NewByte = newByte;
        }
    }
}