using Moq;
using System;
using Xunit;

namespace rdiff.net.tests
{
    public class RollingHashTests
    {
        private readonly RollingHash target;

        public RollingHashTests()
        {
            this.target = new RollingHash();
        }

        [Fact]
        public void CalculateSignature_WhenInputHas8UniqueBytes_ShouldMatchExpected()
        {
            var input = new SequentialBytesReader("ABCDabcd");
            var blockLength = 4;
            var strongSignatureLength = 32;
            var expectedStronSignaturesCount = 2;
            var expectedWeakSignaturesCount = 2;

            var actual = this.target.CalculateSignature(input, blockLength, strongSignatureLength);

            Assert.NotNull(actual);
            Assert.Equal(blockLength, actual.BlockLength);
            Assert.Equal(strongSignatureLength, actual.StrongSigLength);
            Assert.Equal(expectedStronSignaturesCount, actual.StrongSignatures.Count);
            Assert.Equal(expectedWeakSignaturesCount, actual.WeakSigToBlock.Count);
        }

        [Fact]
        public void CalculateSignature_WhenBlockLengthIsBiggerThanInput_ShouldHaveOnlyOneSignature()
        {
            var input = new SequentialBytesReader("ABCDabcd");
            var blockLength = 128;
            var strongSignatureLength = 32;
            var expectedStronSignaturesCount = 1;
            var expectedWeakSignaturesCount = 1;

            var actual = this.target.CalculateSignature(input, blockLength, strongSignatureLength);

            Assert.Equal(expectedStronSignaturesCount, actual.StrongSignatures.Count);
            Assert.Equal(expectedWeakSignaturesCount, actual.WeakSigToBlock.Count);
        }

        [Fact]
        public void CalculateSignature_WhenInputHasTwoSameChunks_ShouldHaveOneWeakSignature()
        {
            var input = new SequentialBytesReader("ABCDABCD");
            var blockLength = 4;
            var strongSignatureLength = 32;
            var expectedStronSignaturesCount = 2;
            var expectedWeakSignaturesCount = 1;

            var actual = this.target.CalculateSignature(input, blockLength, strongSignatureLength);

            Assert.Equal(expectedStronSignaturesCount, actual.StrongSignatures.Count);
            Assert.Equal(expectedWeakSignaturesCount, actual.WeakSigToBlock.Count);
        }

        [Fact]
        public void CalculateSignature_WhenPredefinedInput_ShouldMatchExpected()
        {
            var input = new SequentialBytesReader(new byte[] { 1, 2, 3, 4, 5, 250, 251, 252, 253, 254, 255 });
            var blockLength = 4;
            var strongSignatureLength = 32;
            var expectedStrongSig1 = "63781d171425a36312fa058d8712d5d0";
            var expectedStrongSig2 = "fd343f2ad92220948adead189da46ae2";
            var expectedStrongSig3 = "f3a067595f7870ad55636ff81c04a72c";
            var expectedWeakSigAsKey0 = 2578;
            var expectedWeakSigAsKey1 = 56750;
            var expectedWeakSigAsKey2 = 46314;

            var actual = this.target.CalculateSignature(input, blockLength, strongSignatureLength);

            Assert.Equal(expectedStrongSig1, actual.StrongSignatures[0]);
            Assert.Equal(expectedStrongSig2, actual.StrongSignatures[1]);
            Assert.Equal(expectedStrongSig3, actual.StrongSignatures[2]);
            Assert.Equal(0, actual.WeakSigToBlock[expectedWeakSigAsKey0]);
            Assert.Equal(1, actual.WeakSigToBlock[expectedWeakSigAsKey1]);
            Assert.Equal(2, actual.WeakSigToBlock[expectedWeakSigAsKey2]);
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(1)]
        public void CalculateSignature_BlockLengthIsIncorrect_ThrowsArgumentException(int invalidBlockLength)
        {
            var validStrongSignatureLength = 32;
            var bytesReaderMock = new Mock<IBytesReader>();

            Assert.Throws<ArgumentException>(() => this.target.CalculateSignature(bytesReaderMock.Object, invalidBlockLength, validStrongSignatureLength));
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(256)]
        [InlineData(-5)]
        public void CalculateSignature_StronSignatureLengthIsIncorrect_ThrowsArumentException(int invalidStrongSignatureLength)
        {
            var validBlockLength = 2048;
            var bytesReaderMock = new Mock<IBytesReader>();

            Assert.Throws<ArgumentException>(() => this.target.CalculateSignature(bytesReaderMock.Object, validBlockLength, invalidStrongSignatureLength));
        }
    }
}
