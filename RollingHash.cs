using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdiff.net
{
    public class RollingHash
    {
        private const int OVERFLOW_GUARD = 2 << 16;
        private const int PRIME = 8355967;
        private const int d = 13;

        public Signature CalculateSignature(IBytesReader reader, int blockLength, int strongSigLength)
        {
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

            Console.WriteLine($"Processed bytes: {bytesProcessed}");
            return resultSignature;
        }

        public string CalculateDelta(Signature mainSignature, IBytesReader modifiedBytes)
        {
            var result = new StringBuilder();
            byte leftMostByte = default;
            var cirBuffer = new Queue<byte>(mainSignature.BlockLength);
            var hash = 0;
            var hashProcessedBytes = 0;

            while (true)
            {
                var nextByteAsInt = modifiedBytes.GetNextByte();
                if (nextByteAsInt == -1) // EOF
                {
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

                if (hashProcessedBytes >= mainSignature.BlockLength)
                {
                    //Rotate
                    result.AppendLine($"Need to append One byte with value {leftMostByte}: {(char)leftMostByte}");
                    hash = Rotate(hash, leftMostByte, (byte)nextByteAsInt, mainSignature.BlockLength);
                }
                else
                {
                    hash = RollIn(hash, (byte)nextByteAsInt, mainSignature.BlockLength);
                }

                hashProcessedBytes++;

                if (hashProcessedBytes < mainSignature.BlockLength)
                {
                    continue;
                }

                int blockIndex;
                if (mainSignature.WeakSigToBlock.TryGetValue(hash, out blockIndex))
                {
                    var strongSignature = this.CalculateStrongSignature(cirBuffer.ToArray(), mainSignature.StrongSigLength);

                    if (mainSignature.StrongSignatures[blockIndex] == strongSignature)
                    {
                        // we have a match
                        result.AppendLine($"Match pos: {blockIndex * mainSignature.BlockLength}, length: {mainSignature.BlockLength}");

                        hash = 0;
                        hashProcessedBytes = 0;
                        cirBuffer.Clear();
                    }
                }
            } // while

            result.AppendLine("have to append what is left");
            result.AppendLine($"length: {cirBuffer.Count}");
            result.AppendLine($"{string.Join("", cirBuffer.Select(x => (char)x))}");

            return result.ToString();
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

        private int PowWithModulo(int x, int n, int modulo)
        {
            var result = 1;
            for (int i = 0; i < n; i++)
            {
                result = (result * x) % modulo;
            }

            return result;
        }
    }
}
