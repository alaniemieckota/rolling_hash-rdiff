using System;
using System.IO;

namespace rdiff.net
{
    public class FileBytesReader : IBytesReader
    {
        private readonly string filePath;
        private Stream sourceStream;

        public FileBytesReader(string filePath)
        {
            if(string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException($"{nameof(filePath)} cannot be null or empty.", nameof(filePath));
            }

            this.filePath = filePath;
        }

        public bool GetNext(int size, out byte[] result)
        {
            result = new byte[size];
            var bytesRead = this.Source.Read(result, 0, size);
            result = result[..bytesRead]; // check if wont explode on 0

            return bytesRead > 0;
        }

        public int GetNextByte()
        {
            return this.Source.ReadByte();
        }

        private Stream Source
        {
            get 
            {
                if (this.sourceStream == null)
                {
                    this.sourceStream = File.OpenRead(this.filePath);
                }

                return sourceStream;
            }
        }
    }
}
