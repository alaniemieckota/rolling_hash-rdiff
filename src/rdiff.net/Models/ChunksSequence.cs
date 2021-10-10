namespace rdiff.net.models
{
    public class ChunksSequence : ISequence
    {
        public SequenceType ChunkType { get; set; } = SequenceType.Chunks;

        public int Position { get; set; }

        public int Length { get; set; }
    }
}
