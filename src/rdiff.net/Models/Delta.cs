using System.Collections.Generic;
using System.Linq;

namespace rdiff.net.models
{
    public class Delta
    {
        public List<ISequence> Sequence { get; } = new List<ISequence>();

        private readonly List<ChunkMetadata> chunksBuffer = new();
        private readonly List<byte> bytesBuffer = new();
        
        public void AddByte(byte byteToAdd)
        {
            this.FlushChunks();

            this.bytesBuffer.Add(byteToAdd);
        }

        public void AddChunk(int position, int length)
        {
            this.FlushBytes();

            this.chunksBuffer.Add(new ChunkMetadata { Position = position, Length = length });
        }

        public Delta Flush()
        {
            this.FlushChunks();
            this.FlushBytes();

            return this;
        }

        private void FlushChunks()
        {
            if (chunksBuffer.Count == 0)
            {
                return;
            }

            this.Sequence.Add(
                new ChunksSequence 
                { 
                    Position = chunksBuffer[0].Position,
                    Length = chunksBuffer.Sum(x => x.Length)
                }
            );

            chunksBuffer.Clear();
        }

        private void FlushBytes()
        {
            if (bytesBuffer.Count == 0)
            {
                return;
            }

            this.Sequence.Add(new BytesSequence { Bytes = bytesBuffer.ToArray(), Length = bytesBuffer.Count });
            bytesBuffer.Clear();
        }

        private class ChunkMetadata
        {
            public int Position { get; set; }

            public int Length { get; set; }
        }
    }
}
