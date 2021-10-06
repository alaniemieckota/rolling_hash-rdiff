using System;
using System.IO;

namespace rdiff.net
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            new Program().Run();
        }
        
        private void Run()
        {
            var rollingHash = new RollingHash();
            var filePath = @"c:\temp\test_file1.txt";
            var file1 = new FileBytesReader(filePath);
            var blockLength = 164;
            var strongSigLength = 32;

            var signature1 = rollingHash.CalculateSignature(file1, blockLength, strongSigLength);
            var text = File.ReadAllText(filePath); // modify text so we can calculate delta
            text = "X" + text;
            text = text + "X";
            //text = text.Substring(6);

            var delta = rollingHash.CalculateDelta(signature1, new StringBytesReader(text));

            Console.WriteLine(delta);

            Console.WriteLine("END");
        }
    }
}
