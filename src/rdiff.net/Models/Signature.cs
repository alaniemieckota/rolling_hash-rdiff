using System.Collections.Generic;

namespace rdiff.net.models
{
    public class Signature
    {
        public int BlockLength { get; set; } = 1024;

        public int StrongSigLength { get; set; } = 64;

        public Dictionary<int, int> WeakSigToBlock { get; set; } = new();

        public List<string> StrongSignatures = new();
    }
}
