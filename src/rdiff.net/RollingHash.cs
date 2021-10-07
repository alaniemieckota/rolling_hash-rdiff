using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdiff.net
{
    public class RollingHash
    {
        private const int OVERFLOW_GUARD = 131072; //  2 << 16
        private const int d = 13;

        private const int MIN_BLOCK_LENGTH = 2;
        private const int MAX_BLOCK_LENGTH = 2 << 24;
        private const int MIN_STRONG_SIGNATURE_LENGTH = 4;
        private const int MAX_STRONG_SIGNATURE_LENGTH = 64;


        public Signature CalculateSignature(IBytesReader reader, int blockLength, int strongSigLength)
        {
            if(blockLength < MIN_BLOCK_LENGTH || blockLength > MAX_BLOCK_LENGTH)
            {
                throw new ArgumentException($"Block length has to be in range [{MIN_BLOCK_LENGTH}, {MAX_BLOCK_LENGTH}]"
                    ,nameof(blockLength));
            }

            if (strongSigLength < MIN_STRONG_SIGNATURE_LENGTH || strongSigLength > MAX_STRONG_SIGNATURE_LENGTH)
            {
                throw new ArgumentException(
                    $"Strong signature length has to be in range [{MIN_STRONG_SIGNATURE_LENGTH}, {MAX_STRONG_SIGNATURE_LENGTH}]"
                    ,nameof(strongSigLength));
            }

            var bytesProcessed = 0;
            var resultSignature = new Signature { BlockLength = blockLength, StrongSigLength = strongSigLength };

            while (reader.GetNext(blockLength, out byte[] chunk))
            {
                var weakSignature = CalculateWeak(chunk);
                var strongSignature = this.CalculateStrongSignature(chunk, strongSigLength);

                resultSignature.WeakSigToBlock[weakSignature] = resultSignature.StrongSignatures.Count; // at which chunk the signature is calculated
                resultSignature.StrongSignatures.Add(strongSignature);
                bytesProcessed += chunk.Length;
            }

            return resultSignature; // this could be directly written to stream|file but I wanted to keep it simple and more descriptive
        }

        public string CalculateDelta(Signature mainSignature, IBytesReader modifiedBytes)
        {
            var result = new StringBuilder();
            byte leftMostByte = default;
            var cirBuffer = new Queue<byte>(mainSignature.BlockLength); // represents a wheel rolling forward through the file
            var hash = 0;
            var hashProcessedBytesCounter = 0;
            int blockIndex;

            while (true)
            {
                var nextByteAsInt = modifiedBytes.GetNextByte();
                if (nextByteAsInt == -1) // EOF
                {
                    // check if what has left in the buffer is matching last chunk
                    blockIndex = GetBlockIndexOnMatch(hash, cirBuffer.ToArray(), mainSignature);
                    if (blockIndex >= 0)
                    {
                        result.AppendLine($"Match pos: {blockIndex * mainSignature.BlockLength}, length: {mainSignature.BlockLength}");
                        cirBuffer.Clear();
                    }

                    break;
                }

                if (cirBuffer.Count > 0)
                {
                    leftMostByte = cirBuffer.Peek();
                }

                if (cirBuffer.Count == mainSignature.BlockLength)
                {
                    cirBuffer.Dequeue();
                }

                cirBuffer.Enqueue((byte)nextByteAsInt);

                if (hashProcessedBytesCounter >= mainSignature.BlockLength)
                {
                    //Rotate
                    result.AppendLine($"Need to append One byte with value {leftMostByte}: {(char)leftMostByte}");
                    hash = Rotate(hash, leftMostByte, (byte)nextByteAsInt, mainSignature.BlockLength); // remove left byte, add next byte
                }
                else
                {
                    hash = RollIn(hash, (byte)nextByteAsInt, mainSignature.BlockLength); // just add next byte from file
                }

                hashProcessedBytesCounter++;

                if (hashProcessedBytesCounter < mainSignature.BlockLength)
                {
                    continue;
                }

                // Do I have such hash in base signature?
                blockIndex = GetBlockIndexOnMatch(hash, cirBuffer.ToArray(), mainSignature);
                if (blockIndex >= 0)
                {
                    result.AppendLine($"Match pos: {blockIndex * mainSignature.BlockLength}, length: {mainSignature.BlockLength}");

                    hash = 0;
                    hashProcessedBytesCounter = 0;
                    cirBuffer.Clear();
                }
            } // while

            result.AppendLine("have to append what is left");
            result.AppendLine($"length: {cirBuffer.Count}");
            result.AppendLine($"{string.Join("", cirBuffer.Select(x => (char)x))}");

            return result.ToString();
        }

        private int GetBlockIndexOnMatch(int hash, byte[] inBuffer, Signature mainSignature)
        {
            int blockIndex;

            // Do I have such hash in base signature?
            if (mainSignature.WeakSigToBlock.TryGetValue(hash, out blockIndex))
            {
                // Does it really equal? (strong sig is here so there is no need to compare byte by byte)
                var strongSignature = this.CalculateStrongSignature(inBuffer, mainSignature.StrongSigLength);

                if (mainSignature.StrongSignatures[blockIndex] == strongSignature)
                {
                    return blockIndex;
                }
            }

            return -1;
        }

        private string CalculateStrongSignature(byte[] chunk, int signatureLength)
        {
            return Blake3.Hasher.Hash(chunk).ToString()[..signatureLength];
        }

        private int CalculateWeak(byte[] chunk)
        {
            var hash = 0;

            for (int i = 0; i < chunk.Length; i++)
            {
                hash = (hash * d + chunk[i]) % OVERFLOW_GUARD;
            }

            return hash;
        }

        private int RollIn(int hash, byte inByte, int length)
        {
            hash = (hash * d + inByte) % OVERFLOW_GUARD;

            return hash;
        }

        private int Rotate(int hash, byte outByte, byte newByte, int blockLength)
        {
            hash = ((d*(hash - outByte*PowWithModulo(d, blockLength - 1, OVERFLOW_GUARD))) + newByte) % OVERFLOW_GUARD;
            if(hash < 0)
            {
                hash = hash + OVERFLOW_GUARD;
            }
            return hash;
        }

        /// <summary>
        /// This is like
        /// </summary>
        /// <param name="x">base</param>
        /// <param name="n">power</param>
        /// <param name="modulo">by this module the remainder is applied to keep the result not to overflow</param>
        /// <returns>modulo remined of (x^n)</returns>
        private int PowWithModulo(int x, int n, int modulo)
        {
            var result = 1;
            for (int i = 0; i < n; i++)
            {
                result = (result * x) % modulo; // ((x^n) % m == (x^1 % m) * (x^2 % m) * ... * (x^n % m)) % m
            }

            return result;
        }
    }
}
