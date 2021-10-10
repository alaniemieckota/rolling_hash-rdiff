namespace rdiff.net.models
{
    public class BytesSequence : ISequence
    {
        public BytesSequence()
        {
            this.ChunkType = SequenceType.Bytes;
        }
        
        public byte[] Bytes { get; set; }

        public int Length { get; set; }
    }
}
