
namespace rdiff.net.logic
{
    public static class Consts
    {
        public const int OVERFLOW_GUARD = 131072;
        public const int D = 13;
        
        public const int MIN_BLOCK_LENGTH = 2;
        public const int MAX_BLOCK_LENGTH = 2 << 24;
        public const int MIN_STRONG_SIGNATURE_LENGTH = 4;
        public const int MAX_STRONG_SIGNATURE_LENGTH = 64;

        public const int DEFAULT_BLOCK_LENGTH = 1024;
        public const int DEFAULT_STRONG_SIGNATURE_LENGTH = 32;
    }
}
