using rdiff.net.models;
using System;

namespace rdiff.net.logic
{
    public class SignatureCalculation
    {
        public Signature CalculateSignature(IBytesReader reader, int blockLength, int strongSigLength)
        {
            if(blockLength < Consts.MIN_BLOCK_LENGTH || blockLength > Consts.MAX_BLOCK_LENGTH)
            {
                throw new ArgumentException($"Block length has to be in range [{Consts.MIN_BLOCK_LENGTH}, {Consts.MAX_BLOCK_LENGTH}]"
                    ,nameof(blockLength));
            }

            if (strongSigLength < Consts.MIN_STRONG_SIGNATURE_LENGTH || strongSigLength > Consts.MAX_STRONG_SIGNATURE_LENGTH)
            {
                throw new ArgumentException(
                    $"Strong signature length has to be in range [{Consts.MIN_STRONG_SIGNATURE_LENGTH}, {Consts.MAX_STRONG_SIGNATURE_LENGTH}]"
                    ,nameof(strongSigLength));
            }

            var bytesProcessed = 0;
            var resultSignature = new Signature { BlockLength = blockLength, StrongSigLength = strongSigLength };

            while (reader.GetNext(blockLength, out byte[] chunk))
            {
                var weakSignature = CalculateWeak(chunk);
                var strongSignature = CalculateStrongSignature(chunk, strongSigLength);

                resultSignature.WeakSigToBlock[weakSignature] = resultSignature.StrongSignatures.Count; // at which chunk the signature is calculated
                resultSignature.StrongSignatures.Add(strongSignature);
                bytesProcessed += chunk.Length;
            }

            return resultSignature; // this could be directly written to stream|file but I wanted to keep it simple and more descriptive
        }

        public static string CalculateStrongSignature(byte[] chunk, int signatureLength)
        {
            return Blake3.Hasher.Hash(chunk).ToString()[..signatureLength];
        }

        private int CalculateWeak(byte[] chunk)
        {
            var hash = 0;

            for (int i = 0; i < chunk.Length; i++)
            {
                hash = (hash * Consts.D + chunk[i]) % Consts.OVERFLOW_GUARD;
            }

            return hash;
        }
    }
}
