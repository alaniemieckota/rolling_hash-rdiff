namespace rdiff.net.models
{
    public class BytesSequence : ISequence
    {
        public SequenceType ChunkType { get; set; } = SequenceType.Bytes;

        public byte[] Bytes { get; set; }

        public int Length { get; set; }
    }
}
