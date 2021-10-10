using rdiff.net.models;
using System.Collections.Generic;

namespace rdiff.net.logic
{
    public class DeltaCalculation
    {
        public Delta CalculateDelta(Signature mainSignature, IBytesReader newData)
        {
            var result = new Delta();
            byte leftMostByte = default;
            var cirBuffer = new Queue<byte>(mainSignature.BlockLength); // represents a wheel rolling forward through the file
            var hash = 0;
            var hashProcessedBytesCounter = 0;
            int blockIndex;

            while (true)
            {
                var nextByteAsInt = newData.GetNextByte();
                if (nextByteAsInt == -1) // EOF
                {
                    // check: what has left in the buffer is matching last chunk
                    blockIndex = GetBlockIndexOnMatch(hash, cirBuffer.ToArray(), mainSignature);
                    if (blockIndex >= 0)
                    {
                        result.AddChunk(blockIndex * mainSignature.BlockLength, cirBuffer.Count);

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
                    result.AddByte(leftMostByte);
                    hash = Rotate(hash, leftMostByte, (byte)nextByteAsInt, mainSignature.BlockLength); // remove left byte, add next byte
                }
                else
                {
                    hash = RollIn(hash, (byte)nextByteAsInt, mainSignature.BlockLength); // just take next byte from file
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
                    result.AddChunk(blockIndex * mainSignature.BlockLength, mainSignature.BlockLength);

                    hash = 0;
                    hashProcessedBytesCounter = 0;
                    cirBuffer.Clear();
                }
            } // while

            if (cirBuffer.Count > 0)
            {
                foreach (var b in cirBuffer)
                {
                    result.AddByte(b);
                }
            }

            return result.Flush();
        }

        private int GetBlockIndexOnMatch(int hash, byte[] inBuffer, Signature mainSignature)
        {
            int blockIndex;

            // Do I have such hash in base signature?
            if (mainSignature.WeakSigToBlock.TryGetValue(hash, out blockIndex))
            {
                // Does it really equal? (strong sig is here so there is no need to compare byte by byte)
                var strongSignature = SignatureCalculation.CalculateStrongSignature(inBuffer, mainSignature.StrongSigLength);

                if (mainSignature.StrongSignatures[blockIndex] == strongSignature)
                {
                    return blockIndex;
                }
            }

            return -1;
        }

        private int RollIn(int hash, byte inByte, int length)
        {
            hash = (hash * Consts.D + inByte) % Consts.OVERFLOW_GUARD;

            return hash;
        }

        private int Rotate(int hash, byte outByte, byte newByte, int blockLength)
        {
            hash = ((Consts.D * (hash - outByte * PowWithModulo(Consts.D, blockLength - 1, Consts.OVERFLOW_GUARD))) + newByte) % Consts.OVERFLOW_GUARD;
            if (hash < 0)
            {
                hash = hash + Consts.OVERFLOW_GUARD;
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
        private static int PowWithModulo(int x, int n, int modulo)
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
