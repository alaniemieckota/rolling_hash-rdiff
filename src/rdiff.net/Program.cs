using rdiff.net.logic;
using System;
using System.IO;

namespace rdiff.net
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }
        
        private void Run()
        {
            var rollingHash = new SignatureCalculation();
            var filePath = @"c:\temp\test_file1.txt";
            var file1 = new FileBytesReader(filePath);
            var blockLength = 4;
            var strongSigLength = 32;

            var signature1 = rollingHash.CalculateSignature(file1, blockLength, strongSigLength);
            var text = File.ReadAllText(filePath); // modify text so we can calculate delta
            //text = "X" + text;
            //text = text + "X";
            //text = text[..4] + text[8..text.Length];
            //text = text[..40] + te xt[55..text.Length];

            var delta = new DeltaCalculation().CalculateDelta(signature1, new SequentialBytesReader(text));

            Console.WriteLine(delta);

            Console.WriteLine("END");
        }
    }
}
