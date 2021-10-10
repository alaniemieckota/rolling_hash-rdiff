namespace rdiff.net.models
{
    public class ChunksSequence : ISequence
    {
        public ChunksSequence()
        {
            this.ChunkType = SequenceType.Chunks;
        }

        public int Position { get; set; }

        public int Length { get; set; }
    }
}
