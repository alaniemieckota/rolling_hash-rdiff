using rdiff.net.logic;
using rdiff.net.models;
using Xunit;

namespace rdiff.net.tests.logic
{
    public class DeltaCalculationTests
    {
        private readonly DeltaCalculation target;

        public DeltaCalculationTests()
        {
            this.target = new DeltaCalculation();
        }

        [Theory]
        [InlineData(128, "SomeRAndom sequence of Bytest @$#%RTGREWFDSCX%^trg,c., should be long but here for demon and testing purposes this is what it is")]
        [InlineData(61, "wwwInput which latst byte does not fit nicely int block size.")]
        [InlineData(38, "2 more chars whatn  n times block size")]
        public void CalculateDelta_NoChangesInputNicelyDividesIntoBlock_ShouldMatchExpected(int expectedBytesLength, string input)
        {
            var notModifiedInput = input;
            var blockLength = 4;
            var strongSignatureLength = 16;
            var signature = new SignatureCalculation().CalculateSignature(new SequentialBytesReader(input), blockLength, strongSignatureLength);
            var expectedPosition = 0;
           
            var actual = this.target.CalculateDelta(signature, new SequentialBytesReader(notModifiedInput));

            Assert.Single(actual.Sequence);
            Assert.Equal(SequenceType.Chunks, actual.Sequence[0].ChunkType);
            var firstSequenceItem = (ChunksSequence)actual.Sequence[0];
            Assert.Equal(expectedPosition, firstSequenceItem.Position);
            Assert.Equal(expectedBytesLength, firstSequenceItem.Length);
        }

        [Fact]
        public void CalculateDelta_AppendedByteToFrontAndBack_ShouldMatchExpected()
        {
            var originalInput = "AAAAbbbbCCCCdddd";
            var modifiedInput = "XAAAAbbbbCCCCddddX";
            var xByteValue = (byte)'X';
            var blockLength = 4;
            var strongSignatureLength = 16;
            var expectedSequencesCount = 3;

            var signature = new SignatureCalculation().CalculateSignature(new SequentialBytesReader(originalInput), blockLength, strongSignatureLength);
            var actual = this.target.CalculateDelta(signature, new SequentialBytesReader(modifiedInput));

            Assert.Equal(expectedSequencesCount, actual.Sequence.Count);
            
            Assert.Equal(SequenceType.Bytes, actual.Sequence[0].ChunkType);
            var firstSequenceItem = (BytesSequence)actual.Sequence[0];
            Assert.Equal(1, firstSequenceItem.Length);
            Assert.Equal(xByteValue, firstSequenceItem.Bytes[0]);

            Assert.Equal(SequenceType.Chunks, actual.Sequence[1].ChunkType);
            var secondSequenceItem = (ChunksSequence)actual.Sequence[1];
            Assert.Equal(0, secondSequenceItem.Position);
            Assert.Equal(16, secondSequenceItem.Length);

            Assert.Equal(SequenceType.Bytes, actual.Sequence[2].ChunkType);
            var thirdSequenceItem = (BytesSequence)actual.Sequence[2];
            Assert.Equal(1, thirdSequenceItem.Length);
            Assert.Equal(xByteValue, thirdSequenceItem.Bytes[0]);
        }

        [Fact]
        public void CalculateDelta_BytesCompletelyChanged_ShouldRewriteCompletelyAndContainOnlyOneBytesSequence()
        {
            var originalInput = "AAAAbbbbCCCCdddd";
            var modifiedInput = "zzzxxxcccvvvhhhmmmmhhh";
            var blockLength = 4;
            var strongSignatureLength = 16;
            var expectedSequencesCount = 1;
            var expectedLength = modifiedInput.Length;

            var signature = new SignatureCalculation().CalculateSignature(new SequentialBytesReader(originalInput), blockLength, strongSignatureLength);
            var actual = this.target.CalculateDelta(signature, new SequentialBytesReader(modifiedInput));

            Assert.Equal(expectedSequencesCount, actual.Sequence.Count);

            Assert.Equal(SequenceType.Bytes, actual.Sequence[0].ChunkType);
            var firstSequenceItem = (BytesSequence)actual.Sequence[0];
            Assert.Equal(expectedLength, firstSequenceItem.Length);
        }

        [Fact]
        public void CalculateDelta_SecondChunkRemoved_ShouldContainsTwoChunksSequences()
        {
            var originalInput = "AAAAbbbbCCCCdddd";
            var modifiedInput = "AAAACCCCdddd";
            var blockLength = 4;
            var strongSignatureLength = 16;
            var expectedSequencesCount = 2;

            var signature = new SignatureCalculation().CalculateSignature(new SequentialBytesReader(originalInput), blockLength, strongSignatureLength);
            var actual = this.target.CalculateDelta(signature, new SequentialBytesReader(modifiedInput));

            Assert.Equal(expectedSequencesCount, actual.Sequence.Count);

            Assert.Equal(SequenceType.Chunks, actual.Sequence[0].ChunkType);
            var firstSequenceItem = (ChunksSequence)actual.Sequence[0];
            Assert.Equal(0, firstSequenceItem.Position);
            Assert.Equal(4, firstSequenceItem.Length);

            Assert.Equal(SequenceType.Chunks, actual.Sequence[1].ChunkType);
            var secondSequenceItem = (ChunksSequence)actual.Sequence[1];
            Assert.Equal(8, secondSequenceItem.Position);
            Assert.Equal(8, secondSequenceItem.Length);
        }

        [Fact]
        public void CalculateDelta_SecondChunkMovedToEnd_ShouldHave3ChunksAndMatchExpected()
        {
            var originalInput = "AAAAbbbbCCCCdddd";
            var modifiedInput = "AAAACCCCddddbbbb";
            var blockLength = 4;
            var strongSignatureLength = 16;
            var expectedSequencesCount = 3;

            var signature = new SignatureCalculation().CalculateSignature(new SequentialBytesReader(originalInput), blockLength, strongSignatureLength);
            var actual = this.target.CalculateDelta(signature, new SequentialBytesReader(modifiedInput));

            Assert.Equal(expectedSequencesCount, actual.Sequence.Count);

            Assert.Equal(SequenceType.Chunks, actual.Sequence[0].ChunkType);
            var firstSequenceItem = (ChunksSequence)actual.Sequence[0];
            Assert.Equal(0, firstSequenceItem.Position);
            Assert.Equal(4, firstSequenceItem.Length);

            Assert.Equal(SequenceType.Chunks, actual.Sequence[1].ChunkType);
            var secondSequenceItem = (ChunksSequence)actual.Sequence[1];
            Assert.Equal(8, secondSequenceItem.Position);
            Assert.Equal(8, secondSequenceItem.Length);

            Assert.Equal(SequenceType.Chunks, actual.Sequence[2].ChunkType);
            var thirdSequenceItem = (ChunksSequence)actual.Sequence[2];
            Assert.Equal(4, thirdSequenceItem.Position);
            Assert.Equal(4, thirdSequenceItem.Length);
        }
    }
}
